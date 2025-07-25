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
	public sealed class BuffsWhenWalkedOnMerger : PatchHandler
	{
		const string targetPropertyName = "BuffsWhenWalkedOn";
		public override string Name { get { return "BuffsWhenWalkedOn"; } }
		private static BuffsWhenWalkedOnMerger? _instance;
		private static readonly object _lock = new();
		private bool _merged = false;

		public static BuffsWhenWalkedOnMerger Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_lock)
					{
						_instance ??= new BuffsWhenWalkedOnMerger();
					}
				}
				return _instance;
			}
		}
		private BuffsWhenWalkedOnMerger()
		{
			Log = new Logger(componentName: Name, hostOnly: true);
			ModEvents.GameStartDone.RegisterHandler((ref ModEvents.SGameStartDoneData data) =>
			{
				Log.WriteLogFiles();
			});
		}

		private object? State { get; set; }
		private bool Merged
		{
			get
			{
				if (_merged)
				{
					_merged = false;
					if (PatchInfo.MatchesState(State))
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
					State = PatchInfo.State;
				}
				_merged = value;
			}
		}

		public override void Run(PatchInfo args)
		{
			PatchInfo = args;
			if (
				Merged ||
				PatchInfo.PatchType == HarmonyPatchType.Postfix ||
				!PatchInfo.TargetFile.GetXpathResults(PatchInfo.XPath, out List<XObject> matches)
			) return;

			MatchList = matches;

			/*
            * APPEND/PREPENDs a <property name="BuffsWhenWalkedOn"> -> we want these merged into the appropriate children of the matched elements instead of appended
            * INSERTAFTER/INSERTBEFORE a <property name="BuffsWhenWalkedOn"> -> same as above, except the matched elements are siblings
            * SETATTRIBUTE -> we care about this case because it can add new attributes
            * 
            * APPEND/PREPENDs an attribute -> we don't care about this case because 1) nobody does this and 2) it should be fine even if they did
            * SETs an attribute -> we don't care about this case either because SET can't add new attributes, and blocks don't have that attribute by default
            */

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
				default: throw new NotImplementedException();
			}

			MatchList.Clear();
		}

		private void MergeElements()
		{
			// This method merges BuffsWhenWalkedOn properties before mods try to insert them into matching nodes that already have that property.
			XElement source = PatchInfo.PatchSourceElement;
			HashSet<XElement> toRemove = new();
			bool hadElements = source.Elements().Any();

			foreach (XElement patchChild in source.Elements())
			{
				if (!patchChild.TryGetAttribute("name", out string patchPropertyName) || patchPropertyName != targetPropertyName)
					continue;

				if (!patchChild.TryGetAttribute("value", out string patchPropertyValue))
					continue;

				AnalyzeMatchedElements(
					parentSelector: match =>
					{
						if (match is XElement xmatch)
							return PatchInfo.MethodType == XMLPatchMethod.Append || PatchInfo.MethodType == XMLPatchMethod.Prepend
							? xmatch
							: xmatch.Parent;
						return null;
					},
					elementProcessor: parent =>
					{
						int count = 0;
						foreach (XElement child in parent.Elements())
						{
							if (child.TryGetAttribute("name", out string nameAV) && nameAV == targetPropertyName)
							{
								TryAppendToAttribute(parent, "value", patchPropertyValue);
								count++;
							}
						}
						if (count == 0)
						{
							XElement prop = new("property");
							prop.SetAttributeValue("name", targetPropertyName);
							prop.SetAttributeValue("value", patchPropertyValue);
							parent.Add(prop);
						}
						else
						{
							string logFile = $"BuffsWhenWalkedOn\\{PatchInfo.PatchingMod.FolderName}.txt";
							Log.AddLine($"buff(s) from {PatchInfo.PatchingMod.Name} merged into block '{parent.GetAttribute("name")}'", logFile);
						}
					});
				toRemove.Add(patchChild);
			}
			foreach (XElement patchChild in toRemove)
			{
				patchChild.Remove();
			}
			if (hadElements && !source.Elements().Any())
			{
				Merged = true;
			}
		}

		private void MergeAttribute()
		{
			XElement source = PatchInfo.PatchSourceElement;

			if (source.FirstNode is not XText firstNode || source.GetAttribute("name") != "value") return;

			string value = firstNode.Value.Trim();
			bool hasBuffsWhenWalkedOnAttribute = false;

			foreach (XElement match in MatchList)
			{
				if (match.TryGetAttribute("name", out string nameAV) && nameAV == targetPropertyName)
				{
					hasBuffsWhenWalkedOnAttribute = true;
					break;
				}
			}

			if (hasBuffsWhenWalkedOnAttribute)
			{
				AnalyzeMatchedElements(
					parentSelector: match =>
					{
						return match as XElement;
					},
					elementProcessor: parent =>
					{
						if (parent.TryGetAttribute("name", out string nameAV) && nameAV == targetPropertyName)
						{
							if (!TryAppendToAttribute(parent, "value", value))
							{
								parent.SetAttributeValue("value", value);
							}
							string logFile = $"BuffsWhenWalkedOnMerger\\{PatchInfo.PatchingMod.FolderName}.txt";
							Log.AddLine($"buff(s) from {PatchInfo.PatchingMod.Name} merged into block '{parent.Parent?.GetAttribute("name")}'", logFile);
							Merged = true;
						}
						else
							parent.SetAttributeValue("value", value);
					});
			}
		}
	}
}
