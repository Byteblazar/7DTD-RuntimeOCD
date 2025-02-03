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
        // These two methods merely fulfill the purpose of tracking which elements are modded and which ones are not
        // which given the many potential things that can happen, is not as simple as it sounds...
        private void TakeSnapshotOfMatchedElements()
        {
            switch (PatchInfo.MethodType)
            {
                case XMLPatchMethod.Append:
                case XMLPatchMethod.Prepend:
                    // MatchList is the parents of the XElements to be added, or the target XElements if it's adding attributes to them instead
                    foreach (XObject match in MatchList)
                    {
                        if (match is XElement xElement)
                            ComparisonSet.Merge(xElement.Elements());
                        else if (match is XAttribute xAttribute)
                            ComparisonSet.Add(xAttribute.Parent);
                    }
                    break;

                case XMLPatchMethod.InsertAfter:
                case XMLPatchMethod.InsertBefore:
                    // MatchList is the immediate siblings (XElement) of the nodes to be added                    
                    foreach (XObject match in MatchList)
                    {
                        if (match is XElement xElement)
                            ComparisonSet.Merge(xElement.Parent.Elements());
                    }
                    break;

                case XMLPatchMethod.Remove:
                    // MatchList is the XElements to be removed (which may or may not have modded descendants)
                    /*
                    foreach (XObject match in MatchList)
                    {
                        if (match is XElement xElement)
                            foreach (XElement toRemove in xElement.DescendantsAndSelf())
                            {
                                if(ModdedElements.Contains(toRemove))
                                    ComparisonSet.Add(toRemove);
                            }
                    }*/
                    break;

                case XMLPatchMethod.Set:
                case XMLPatchMethod.SetAttribute:
                case XMLPatchMethod.RemoveAttribute:
                    // MatchList is the XElements to be modified
                    // when not setting an attribute, MatchList will be the parents of the nodes to be replaced with Set (which may or may not have modded descendants)
                    if (IsUsingSetToReplaceNodes())
                    {
                        /*
                        foreach (XObject match in MatchList)
                        {
                            if (match is XElement xElement)
                                ComparisonSet.Merge(xElement.Descendants());
                        }*/
                        break;
                    }

                    foreach (XObject match in MatchList)
                        ComparisonSet.Add(match);

                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        //---------------------------------------------------------------------------------------------------
        private void UpdateModdedElements()
        {
            if (PatchInfo.MatchesState(State))
            {
                switch (PatchInfo.MethodType)
                {
                    case XMLPatchMethod.Append:
                    case XMLPatchMethod.Prepend:
                        // MatchList is the parents of the XElements to be added, or the target XElements if it's adding attributes to them instead
                        foreach (XObject match in MatchList)
                        {
                            if (match is XElement parent)
                            {
                                // ComparisonSet contains all siblings, so we have to look for new elements among them
                                foreach (XElement child in parent.Elements())
                                {
                                    if (!ComparisonSet.Contains(child))
                                    {
                                        foreach (XElement descendant in child.DescendantsAndSelf())
                                        {
                                            XElementEvaluator newEvaluator = new(descendant);
                                            newEvaluator.SetModdedBy(PatchInfo.PatchingMod.Name);
                                            ModdedElements.Add(newEvaluator);
                                        }
                                    }
                                }
                            }
                            // ComparisonSet contains the target elements themselves
                            else if (match is XAttribute xAttribute && !ModdedElements.Contains(xAttribute.Parent))
                            {
                                XElementEvaluator newEvaluator = new(xAttribute.Parent);
                                newEvaluator.SetModdedBy(PatchInfo.PatchingMod.Name);
                                ModdedElements.Add(newEvaluator);
                            }
                        }
                        break;

                    case XMLPatchMethod.InsertAfter:
                    case XMLPatchMethod.InsertBefore:
                        // MatchList is the immediate siblings (XElement) of the nodes to be added
                        // ComparisonSet is all of the siblings of the added nodes
                        foreach (XObject match in MatchList)
                        {
                            if (match is XElement xElement)
                            {
                                foreach (XElement sibling in xElement.Parent.Elements())
                                {
                                    if (!ComparisonSet.Contains(sibling))
                                    {
                                        foreach (XElement descendant in sibling.DescendantsAndSelf())
                                        {
                                            XElementEvaluator newEvaluator = new(descendant);
                                            newEvaluator.SetModdedBy(PatchInfo.PatchingMod.Name);
                                            ModdedElements.Add(newEvaluator);
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case XMLPatchMethod.Remove:
                        // MatchList should be empty, otherwise it would be the XElements to be removed (which may or may not have modded descendants)
                        // ComparisonSet is the previous descendants of the targets, and the targets themselves
                        SanitizeModdedElements();
                        break;

                    case XMLPatchMethod.Set:
                    case XMLPatchMethod.SetAttribute:
                    case XMLPatchMethod.RemoveAttribute:
                        // MatchList is the XElements to be modified
                        // when not setting an attribute, MatchList will be the parents of the nodes to be replaced with Set (which may or may not have modded descendants)
                        if (IsUsingSetToReplaceNodes())
                        {
                            // ComparisonSet is the descendants of the matched XElements, if any
                            foreach (XObject match in MatchList)
                            {
                                if (match is XElement parent)
                                {
                                    SanitizeModdedElements();
                                    foreach (XElement descendant in parent.Descendants())
                                    {
                                        XElementEvaluator newEvaluator = new(descendant);
                                        newEvaluator.SetModdedBy(PatchInfo.PatchingMod.Name);
                                        ModdedElements.Add(newEvaluator);
                                    }
                                }
                            }
                            break;
                        }
                        // ComparisonSet is the modded XElements, if any
                        foreach (XElementEvaluator evaluator in ComparisonSet)
                        {
                            evaluator.SetModdedBy(PatchInfo.PatchingMod.Name);
                            ModdedElements.Add(evaluator);
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            ComparisonSet.Clear();
        }
    }
}
