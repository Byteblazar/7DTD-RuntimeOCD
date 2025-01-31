using System.Xml.Linq;

namespace RuntimeOCD
{
    internal partial class ConflictDetector
    {
        private void AnalyzeNewChildren() =>
            AnalyzeMatchedElements(
                parentSelector: match => match is XElement parent ? parent : null,
                elementProcessor: parent =>
                {
                    foreach (var sibling in parent.Elements())
                        TestSiblingCollision(sibling);

                    TestForcedParenthood(parent);
                });

        private void AnalyzeNewSiblings() =>
            AnalyzeMatchedElements(
                parentSelector: match => match is XElement sibling ? sibling.Parent : null,
                elementProcessor: parent =>
                {
                    foreach (var sibling in parent.Elements())
                        TestSiblingCollision(sibling);

                    TestForcedParenthood(parent);
                });

        private void AnalyzeRemovedElements() =>
            AnalyzeMatchedElements(
                parentSelector: match => match as XElement,
                elementProcessor: xmatch =>
                {
                    foreach (XElement toRemove in xmatch.DescendantsAndSelf())
                    {
                        if (ModdedElements.Contains(toRemove)
                            && ModdedElements[toRemove].GetOtherModNames(PatchInfo.PatchingMod.Name, out HashSet<string> modNamesOfRemovedElement))
                        {
                            Log.Warn($"((R)) <{PatchInfo.PatchingMod.Name}> is removing {toRemove.Name} '{toRemove.GetAttribute("name")}' which was added or modified by the following mod(s):");
                            Log.Warn(modNamesOfRemovedElement, "      ");
                            Log.Warn($"      Source {PatchInfo.PatchSourceElement.GetElementString()}...");
                        }
                    }
                });

        private void AnalyzeModifiedNodes() =>
            AnalyzeMatchedElements(
                parentSelector: match =>
                {
                    if (match is XElement element)
                        return element;
                    if (match is XAttribute attribute)
                        return attribute.Parent;
                    return null;
                },
                elementProcessor: xmatch =>
                {
                    if (IsUsingSetToReplaceNodes())
                    {
                        foreach (XElement toModify in xmatch.DescendantsAndSelf())
                        {
                            if (ModdedElements.Contains(toModify)
                                && ModdedElements[toModify].GetOtherModNames(PatchInfo.PatchingMod.Name, out HashSet<string> modNamesOfAffectedElements))
                            {
                                Log.Warn($"((EO)) <{PatchInfo.PatchingMod.Name}> is overwriting {toModify.Name} '{toModify.GetAttribute("name")}' which was added or modified by the following mod(s):");
                                Log.Warn(modNamesOfAffectedElements, "       ");
                                Log.Warn($"       Source {PatchInfo.PatchSourceElement.GetElementString()}...");
                            }
                        }
                    }
                    else if (ModdedElements.Contains(xmatch)
                                && ModdedElements[xmatch].GetOtherModNames(PatchInfo.PatchingMod.Name, out HashSet<string> modNamesOfAffectedElements))
                    {
                        Log.Info($"((AO)) <{PatchInfo.PatchingMod.Name}> is modifying {xmatch.Name} '{xmatch.GetAttribute("name")}' which was added or modified by the following mod(s):");
                        Log.Info(modNamesOfAffectedElements, "       ");
                        Log.Info($"       Source {PatchInfo.PatchSourceElement.GetElementString()}...");
                    }
                });
    }
}
