/*
	This file is part of RuntimeOCD.

	RuntimeOCD is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

	RuntimeOCD is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

	You should have received a copy of the GNU General Public License along with RuntimeOCD. If not, see <https://www.gnu.org/licenses/>. 
*/

using System.Xml.Linq;

namespace RuntimeOCD
{
	internal partial class ConflictDetector
	{
		private void TestSiblingCollision(XElement sibling)
		{
			if (!ModdedElements.Contains(sibling)) return;
			XElementEvaluator evaluator = ModdedElements[sibling];
			foreach (XElement toAdd in PatchInfo.PatchSourceElement.Elements())
			{
				if (evaluator.IsNamesakeOf(toAdd) && evaluator.GetOtherModNames(PatchInfo.PatchingMod.Name, out HashSet<string> modNamesOfSiblings))
				{
					Log.Info($"((SC)) {toAdd.Name} '{toAdd.GetAttribute("name")}' from <{PatchInfo.PatchingMod.Name}> was already added or modified by the following mods:");
					Log.Info(modNamesOfSiblings, "       ");
					Log.Info($"       Source {PatchInfo.PatchSourceElement.GetElementString()}...");
				}
			}
		}
		private void TestForcedParenthood(XElement parent)
		{
			HashSet<string> moddedAncestors = new();
			foreach (XElement ancestor in parent.AncestorsAndSelf())
			{
				if (ModdedElements.Contains(ancestor)
					&& ModdedElements[ancestor].GetOtherModNames(PatchInfo.PatchingMod.Name, out HashSet<string> modNamesOfAncestors))
				{
					moddedAncestors.UnionWith(modNamesOfAncestors);
				}
			}

			if (moddedAncestors.Any())
			{
				Log.Info($"((FP)) <{PatchInfo.PatchingMod.Name}> is adding descendants to elements that were added or modified by the following mods:");
				Log.Info(moddedAncestors, "       ");
				Log.Info($"       Source {PatchInfo.PatchSourceElement.GetElementString()}...");
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
