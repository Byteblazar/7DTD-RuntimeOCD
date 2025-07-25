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
	public sealed partial class ConflictDetector : PatchHandler
	{
		public override string Name { get { return "Conflict Detector"; } }
		private static ConflictDetector? _instance;
		private static readonly object _lock = new();
		private ConflictDetector()
		{
			Log = new Logger(componentName: Name, hostOnly: true);
			Done = false;
			ModEvents.GameStartDone.RegisterHandler((ref ModEvents.SGameStartDoneData data) =>
			{
				if (Done) return;
				Metadata meta = OcdManager.Instance.Meta;
				Log.Info("Purging references from memory.");
				ModdedElements.Clear();
				ComparisonSet.Clear();
				Log.Info($"<color=#00ffb3>Writing log files to {Config.LogsPath}</color>");
				Log.WriteLogFiles();
				Tally.CompareTo(meta.LastTally);
				meta.LastTally = Tally;
				meta.Save();
				Tally = new ConflictsTally();
				Done = true;
			});
		}
		public static ConflictDetector Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_lock)
					{
						_instance ??= new ConflictDetector();
					}
				}
				return _instance;
			}
		}
		public EvaluatorSet ModdedElements { get; } = new();
		private EvaluatorSet ComparisonSet { get; } = new();
		private object? State { get; set; }
		private ConflictsTally Tally { get; set; } = new();
		private bool Done { get; set; }

		public override void Run(PatchInfo args)
		{
			if (Done) return;
			PatchInfo = args;
			if (!PatchInfo.TargetFile.GetXpathResults(PatchInfo.XPath, out List<XObject> matches)) return;
			MatchList = matches;

			if (PatchInfo.PatchType == HarmonyPatchType.Prefix)
			{
				TakeSnapshotOfMatchedElements();

				switch (PatchInfo.MethodType)
				{
					case XMLPatchMethod.Remove:
						// MatchList is the XElements to be removed (which may or may not have modded descendants)
						AnalyzeRemovedElements();
						break;
					case XMLPatchMethod.Set:
					case XMLPatchMethod.SetAttribute:
					case XMLPatchMethod.RemoveAttribute:
						// MatchList is the XElements to be modified
						// when not setting an attribute, MatchList will be the parents of the nodes to be replaced with Set (which may or may not have modded descendants)
						AnalyzeModifiedNodes();
						break;
					case XMLPatchMethod.InsertAfter:
					case XMLPatchMethod.InsertBefore:
						// MatchList is the immediate siblings (XElement) of the nodes to be added
						AnalyzeNewSiblings();
						break;
					case XMLPatchMethod.Append:
					case XMLPatchMethod.Prepend:
						// conflicts when adding children
						if (MatchList[0] is XElement xElement)
							// it's appending/prepending elements
							AnalyzeNewChildren();
						//else if (MatchList[0] is XAttribute xAttribute)
						// it's appending/prepending attributes (should not be a conflict)
						//AnalyzeModifiedNodes();
						break;

					default:
						throw new NotImplementedException();
				}
			}

			else if (PatchInfo.PatchType == HarmonyPatchType.Postfix)
				UpdateModdedElements();

			MatchList.Clear();
			State = PatchInfo.State;
		}
	}
}
