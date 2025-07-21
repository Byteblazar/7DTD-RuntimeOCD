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
	public partial class ConflictDetector
	{
		public class ConflictsTally
		{
			public long AO { get; set; } = 0;
			public long EO { get; set; } = 0;
			public long FP { get; set; } = 0;
			public long R { get; set; } = 0;
			public long SC { get; set; } = 0;
			public void CompareTo(ConflictsTally prev)
			{
				Logger log = Instance.Log;
				log.Info($" R  = {R} {FormatDiff(R - prev.R)}");
				log.Info($" EO = {EO} {FormatDiff(EO - prev.EO)}");
				log.Info($" SC = {SC} {FormatDiff(SC - prev.SC)}");
				log.Info($" AO = {AO} {FormatDiff(AO - prev.AO)}");
				log.Info($" FP = {FP} {FormatDiff(FP - prev.FP)}");
			}
			public string FormatDiff(long diff)
			{
				if (diff == 0) return string.Empty;
				return diff < 0 ? $"<color=#40ff40>(-{diff})</color>" : $"<color=#ff4040>(+{diff})</color>";
			}
		}
		private void TestSiblingCollision(XElement sibling)
		{
			if (!ModdedElements.Contains(sibling)) return;
			XElementEvaluator evaluator = ModdedElements[sibling];
			Mod patcher = PatchInfo.PatchingMod;
			XElement patch = PatchInfo.PatchSourceElement;
			foreach (XElement toAdd in patch.Elements())
			{
				if (evaluator.IsNamesakeOf(toAdd) && evaluator.GetOtherModNames(patcher.Name, out HashSet<string> modNamesOfSiblings))
				{
					Tally.SC++;
					string logFile = $"ConflictDetector_(SC)_Sibling_Collisions\\{patcher.FolderName}__{patcher.VersionString}.txt";
					Log.AddLine($"((SC)) {toAdd.Name} '{toAdd.GetAttribute("name")}' from <{patcher.Name}> was already added or modified by the following mods:", logFile);
					Log.AddLine(modNamesOfSiblings, "       ", logFile);
					Log.AddLine($"       Source {patch.GetElementString()}...", logFile);
					foreach (var modName in modNamesOfSiblings)
					{
						Mod patched = ModManager.GetMod(modName);
						logFile = $"ConflictDetector_(SC)_Sibling_Collisions\\{patched.FolderName}__{patched.VersionString}.txt";
						Log.AddLine($"{patcher.Name} added a {toAdd.Name} '{toAdd.GetAttribute("name")}', which was already added or modified by {modName} before.", logFile);
						Log.AddLine($"       Source {patch.GetElementString()}...", logFile);
					}
				}
			}
		}
		private void TestForcedParenthood(XElement parent)
		{
			Mod patcher = PatchInfo.PatchingMod;
			XElement patch = PatchInfo.PatchSourceElement;
			HashSet<string> moddedAncestors = new();
			foreach (XElement ancestor in parent.AncestorsAndSelf())
			{
				if (ModdedElements.Contains(ancestor)
					&& ModdedElements[ancestor].GetOtherModNames(patcher.Name, out HashSet<string> modNamesOfAncestors))
				{
					moddedAncestors.UnionWith(modNamesOfAncestors);
				}
			}

			if (moddedAncestors.Any())
			{
				Tally.FP++;
				string logFile = $"ConflictDetector_(FP)_Forced_Parenthood\\{patcher.FolderName}__{patcher.VersionString}.txt";
				Log.AddLine($"((FP)) <{patcher.Name}> is adding descendants to elements that were added or modified by the following mods:", logFile);
				Log.AddLine(moddedAncestors, "       ", logFile);
				Log.AddLine($"       Source {patch.GetElementString()}...", logFile);
				foreach (var modName in moddedAncestors)
				{
					Mod patched = ModManager.GetMod(modName);
					logFile = $"ConflictDetector_(FP)_Forced_Parenthood\\{patched.FolderName}__{patched.VersionString}.txt";
					Log.AddLine($"{patcher.Name} added descendants to elements that were added by {modName}.", logFile);
					Log.AddLine($"       Source {patch.GetElementString()}...", logFile);
				}
			}
		}
		private int SanitizeModdedElements()
		{
			int removed = 0;
			var keysToCheck = new List<XElement>(ModdedElements.Keys);
			var toRemove = new HashSet<XElement>();

			foreach (XElement moddedElement in keysToCheck)
			{
				if (moddedElement.Parent == null || moddedElement.Document == null)
				{
					toRemove.Add(moddedElement);
				}
			}

			foreach (XElement moddedElement in toRemove)
			{
				if (ModdedElements.Remove(moddedElement))
					removed++;
			}
			return removed;
		}

	}
}
