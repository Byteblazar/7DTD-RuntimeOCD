/*
 * RuntimeOCD
 * Copyright © 2025 Byteblazar <byteblazar@protonmail.com> * 
 * 
 * 
 * This file is part of RuntimeOCD.
 * 
 * RuntimeOCD is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * RuntimeOCD is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with RuntimeOCD. If not, see <https://www.gnu.org/licenses/>. 
 * 
*/

using System.Xml.Linq;

namespace RuntimeOCD
{
	public abstract class PropertyMergerBase : PatchHandler
	{
		private readonly string _targetPropertyName;
		private readonly char _delimiter;
		private bool _merged;
		private object? _state;

		protected PropertyMergerBase(string componentName, string targetPropertyName, char delimiter = ';')
		{
			Log = new Logger(componentName: componentName, hostOnly: true);
			_targetPropertyName = targetPropertyName;
			_delimiter = delimiter;
			ModEvents.GameStartDone.RegisterHandler((ref ModEvents.SGameStartDoneData data) => Log.WriteLogFiles());
		}

		private bool Merged
		{
			get
			{
				if (_merged)
				{
					_merged = false;
					if (PatchInfo.MatchesState(_state))
					{
						PatchInfo.Result = 1;
						return true;
					}
				}
				return false;
			}
			set
			{
				if (!_merged && value)
				{
					PatchInfo.XPath = "Byteblazar_Was_Here";
					_state = PatchInfo.State;
				}
				_merged = value;
			}
		}

		public override void Run(PatchInfo args)
		{
			PatchInfo = args;
			if (Merged
			 || PatchInfo.PatchType == HarmonyPatchType.Postfix
			 || !PatchInfo.TargetFile.GetXpathResults(PatchInfo.XPath, out List<XObject> matches))
				return;

			MatchList = matches;

			switch (PatchInfo.MethodType)
			{
				case XMLPatchMethod.Append:
				case XMLPatchMethod.Prepend:
				case XMLPatchMethod.InsertAfter:
				case XMLPatchMethod.InsertBefore:
					MergeElements();
					break;
				case XMLPatchMethod.SetAttribute:
					MergeAttribute();
					break;
				default:
					throw new NotImplementedException();
			}

			MatchList.Clear();
		}

		private void MergeElements()
		{
			var source = PatchInfo.PatchSourceElement;
			var toRemove = new HashSet<XElement>();
			bool hadElements = source.Elements().Any();

			foreach (var patchChild in source.Elements().OfType<XElement>())
			{
				if (!patchChild.TryGetAttribute("name", out var name) || name != _targetPropertyName) continue;
				if (!patchChild.TryGetAttribute("value", out var value)) continue;

				var parents = MatchList
					.OfType<XElement>()
					.Select(x =>
						PatchInfo.MethodType == XMLPatchMethod.Append || PatchInfo.MethodType == XMLPatchMethod.Prepend
						? x
						: x.Parent!)
					.Where(x => x != null)
					.ToList();

				foreach (var parent in parents)
				{
					int count = 0;
					foreach (var child in parent.Elements())
					{
						if (child.TryGetAttribute("name", out var n) && n == _targetPropertyName)
						{
							// instance method on PatchHandler:
							TryAppendToAttribute(parent, "value", value, _delimiter);
							count++;
						}
					}

					if (count == 0)
					{
						var prop = new XElement("property",
							new XAttribute("name", _targetPropertyName),
							new XAttribute("value", value));
						parent.Add(prop);
					}
					else
					{
						string logFile = $"{Name}Merger\\{PatchInfo.PatchingMod.FolderName}.txt";
						Log.AddLine(
							$"value(s) from {PatchInfo.PatchingMod.Name} merged into '{parent.GetAttribute("name")}'",
							logFile
						);
					}
				}

				toRemove.Add(patchChild);
			}

			foreach (var rem in toRemove) rem.Remove();
			if (hadElements && !source.Elements().Any())
				Merged = true;
		}

		private void MergeAttribute()
		{
			var source = PatchInfo.PatchSourceElement;
			if (source.FirstNode is not XText first || source.GetAttribute("name") != "value") return;
			string value = first.Value.Trim();

			bool has = MatchList
				.OfType<XElement>()
				.Any(e => e.TryGetAttribute("name", out var n) && n == _targetPropertyName);
			if (!has) return;

			foreach (var elem in MatchList.OfType<XElement>())
			{
				if (elem.TryGetAttribute("name", out var n) && n == _targetPropertyName)
				{
					if (!TryAppendToAttribute(elem, "value", value, _delimiter))
						elem.SetAttributeValue("value", value);

					string logFile = $"{Name}Merger\\{PatchInfo.PatchingMod.FolderName}.txt";
					Log.AddLine(
						$"value(s) from {PatchInfo.PatchingMod.Name} merged into '{elem.Parent?.GetAttribute("name")}'",
						logFile
					);
					Merged = true;
				}
				else
				{
					elem.SetAttributeValue("value", value);
				}
			}
		}
	}
}
