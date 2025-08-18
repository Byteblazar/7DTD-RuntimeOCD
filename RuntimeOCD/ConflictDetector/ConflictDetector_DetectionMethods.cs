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
		private void AnalyzeNewChildren()
		{
			var elementMatches = (
				MatchList?.Where(m => m is XElement)
						  .Cast<XElement>()
						  .ToList()
			) ?? new List<XElement>();

			foreach (var parent in elementMatches)
			{
				foreach (var sibling in parent.Elements())
					TestSiblingCollision(sibling);

				TestForcedParenthood(parent);
			}
		}

		private void AnalyzeNewSiblings()
		{
			var elementMatches = (
				MatchList?
					.OfType<XElement>()
					.Select(e => e.Parent)
					.Where(p => p != null)
					.Cast<XElement>()
					.ToList()
			) ?? new List<XElement>();

			foreach (var parent in elementMatches)
			{
				foreach (var sibling in parent.Elements())
					TestSiblingCollision(sibling);

				TestForcedParenthood(parent);
			}
		}

		private void AnalyzeRemovedElements()
		{
			var elementMatches = (
				MatchList?.Where(m => m is XElement)
						  .Cast<XElement>()
						  .ToList()
			) ?? new List<XElement>();

			Mod patcher = PatchInfo.PatchingMod;
			XElement patch = PatchInfo.PatchSourceElement;
			foreach (var xmatch in elementMatches)
			{
				foreach (XElement toRemove in xmatch.DescendantsAndSelf())
				{
					if (ModdedElements.Contains(toRemove)
						&& ModdedElements[toRemove].GetOtherModNames(patcher.Name, out HashSet<string> modNamesOfRemovedElement))
					{
						Tally.R++;
						string logFile = $"ConflictDetector_(R)_Removals\\{patcher.FolderName}__{patcher.VersionString}.txt";
						Log.AddLine($"((R)) {patcher.Name} is removing <{toRemove.Name}> '{toRemove.GetAttribute("name")}' which was added or modified by the following mod(s):", logFile);
						Log.AddLine(modNamesOfRemovedElement, "      ", logFile);
						Log.AddLine($"      Source {patch.GetElementString()}...", logFile);
						foreach (var modName in modNamesOfRemovedElement)
						{
							Mod patched = ModManager.GetMod(modName);
							logFile = $"ConflictDetector_(R)_Removals\\{patched.FolderName}__{patched.VersionString}.txt";
							Log.AddLine($"<{toRemove.Name}> '{toRemove.GetAttribute("name")}', which was added or modified by {modName}, was then REMOVED by {patcher.Name}", logFile);
							Log.AddLine($"      Source {patch.GetElementString()}...", logFile);
						}
					}
				}
			}
		}

		private void AnalyzeModifiedNodes()
		{
			var elementMatches = (
				MatchList?
					.Select(match =>
						match switch
						{
							XElement elt => elt,
							XAttribute attr => attr.Parent,
							_ => null
						})
					.Where(e => e != null)
					.Cast<XElement>()
					.ToList()
			) ?? new List<XElement>();

			Mod patcher = PatchInfo.PatchingMod;
			XElement patch = PatchInfo.PatchSourceElement;

			foreach (var xmatch in elementMatches)
			{
				if (IsUsingSetToReplaceNodes())
				{
					foreach (XElement toModify in xmatch.DescendantsAndSelf())
					{
						if (ModdedElements.Contains(toModify)
							&& ModdedElements[toModify]
								   .GetOtherModNames(patcher.Name, out HashSet<string> modNames))
						{
							Tally.EO++;
							string logFile = $"ConflictDetector_(EO)_Element_Overrides\\{patcher.FolderName}__{patcher.VersionString}.txt";
							Log.AddLine($"((EO)) {patcher.Name} is overwriting <{toModify.Name}> '{toModify.GetAttribute("name")}' which was added or modified by the following mod(s):", logFile);
							Log.AddLine(modNames, "       ", logFile);
							Log.AddLine($"       Source {patch.GetElementString()}...", logFile);

							foreach (var other in modNames)
							{
								Mod otherMod = ModManager.GetMod(other);
								logFile = $"ConflictDetector_(EO)_Element_Overrides\\{otherMod.FolderName}__{otherMod.VersionString}.txt";
								Log.AddLine($"<{toModify.Name}> '{toModify.GetAttribute("name")}', which was added or modified by {other}, was then OVERWRITTEN by {patcher.Name}", logFile);
								Log.AddLine($"       Source {patch.GetElementString()}...", logFile);
							}
						}
					}
				}
				else
				{
					if (ModdedElements.Contains(xmatch)
						&& ModdedElements[xmatch]
							   .GetOtherModNames(patcher.Name, out HashSet<string> modNames))
					{
						Tally.AO++;
						string logFile = $"ConflictDetector_(AO)_Attribute_Overrides\\{patcher.FolderName}__{patcher.VersionString}.txt";
						Log.AddLine($"((AO)) {patcher.Name} is modifying <{xmatch.Name}> '{xmatch.GetAttribute("name")}' which was added or modified by the following mod(s):", logFile);
						Log.AddLine(modNames, "       ", logFile);
						Log.AddLine($"       Source {patch.GetElementString()}...", logFile);

						foreach (var other in modNames)
						{
							Mod otherMod = ModManager.GetMod(other);
							logFile = $"ConflictDetector_(AO)_Attribute_Overrides\\{otherMod.FolderName}__{otherMod.VersionString}.txt";
							Log.AddLine($"<{xmatch.Name}> '{xmatch.GetAttribute("name")}', which was added or modified by {other}, was then MODIFIED by {patcher.Name}", logFile);
							Log.AddLine($"       Source {patch.GetElementString()}...", logFile);
						}
					}
				}
			}
		}
	}
}
