/*
	This file is part of RuntimeOCD.

	RuntimeOCD is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

    RuntimeOCD is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along with RuntimeOCD. If not, see <https://www.gnu.org/licenses/>. 
*/

using System.Xml.Linq;

namespace RuntimeOCD
{
    internal sealed class BuffsWhenWalkedOnMerger : PatchHandler
    {
        public override string Name { get { return "BuffsWhenWalkedOn"; } }
        private static BuffsWhenWalkedOnMerger _instance;
        private static readonly object _lock = new();
        private bool _merged = false;

        private BuffsWhenWalkedOnMerger()
        {
            Log = new Logger(OcdManager.Name, Name);
        }
        internal static BuffsWhenWalkedOnMerger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new BuffsWhenWalkedOnMerger();
                        }
                    }
                }
                return _instance;
            }
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
                    MergeChildren();
                    break;
                case XMLPatchMethod.InsertAfter:
                case XMLPatchMethod.InsertBefore:
                    MergeSiblings();
                    break;
                case XMLPatchMethod.SetAttribute:
                    MergeAttribute();
                    break;
                default: throw new NotImplementedException();
            }

            MatchList.Clear();
        }

        private void MergeChildren()
        {
            // This method merges BuffsWhenWalkedOn properties before mods try to append/prepend them to matching nodes that already have that property.
            XElement source = PatchInfo.PatchSourceElement;
            string targetPropertyName = "BuffsWhenWalkedOn";

            foreach (XElement patchChild in source.Elements())
            {
                if (!patchChild.TryGetAttribute("name", out string patchPropertyName) || patchPropertyName != targetPropertyName)
                    continue;

                if (!patchChild.TryGetAttribute("value", out string patchPropertyValue))
                    continue;

                AnalyzeMatchedElements(
                    parentSelector: match =>
                    {
                        if (match is XElement parentElement)
                        {
                            foreach (XElement child in parentElement.Elements())
                            {
                                if (child.TryGetAttribute((XName)"name", out string value) && value == targetPropertyName)
                                    return child;
                            }
                        }
                        return null;
                    },
                    elementProcessor: matchedChild =>
                    {
                        if (!TryAppendToAttribute(matchedChild, "value", patchPropertyValue))
                        {
                            matchedChild.SetAttributeValue("value", patchPropertyValue);
                        }
                        Log.Info($"buff(s) from {PatchInfo.PatchingMod.Name} merged into block '{matchedChild.Parent?.GetAttribute((XName)"name")}'", false);
                        Merged = true;
                    });
            }
        }

        private void MergeSiblings()
        {
            // This method merges nodes with BuffsWhenWalkedOn properties when mods try to insert them as siblings of other nodes
            // It checks the children of the parent of each element in MatchList to avoid conflicts
            XElement source = PatchInfo.PatchSourceElement;
            string targetPropertyName = "BuffsWhenWalkedOn";

            foreach (XElement patchChild in source.Elements())
            {
                if (!patchChild.TryGetAttribute("name", out string patchPropertyName) || patchPropertyName != targetPropertyName)
                    continue;

                if (!patchChild.TryGetAttribute("value", out string patchPropertyValue))
                    continue;

                AnalyzeMatchedElements(
                    parentSelector: match =>
                    {
                        if (match is XElement sibling && sibling.Parent is XElement parent)
                        {
                            foreach (XElement siblingChild in parent.Elements())
                            {
                                if (siblingChild.TryGetAttribute((XName)"name", out string value) && value == targetPropertyName)
                                    return siblingChild;
                            }
                        }
                        return null;
                    },
                    elementProcessor: matchedChild =>
                    {
                        if (!TryAppendToAttribute(matchedChild, "value", patchPropertyValue))
                        {
                            matchedChild.SetAttributeValue("value", patchPropertyValue);
                        }
                        Log.Info($"buff(s) from {PatchInfo.PatchingMod.Name} merged into block '{matchedChild.Parent?.GetAttribute((XName)"name")}'", false);
                        Merged = true;
                    });
            }
        }

        private void MergeAttribute()
        {
            XElement source = PatchInfo.PatchSourceElement;

            if (source.FirstNode is not XText firstNode || source.GetAttribute((XName)"name") != "value") return;
            string targetPropertyName = "BuffsWhenWalkedOn";
            string sourceString = PatchInfo.PatchSourceElement.GetElementString();

            string value = firstNode.Value.Trim();

            AnalyzeMatchedElements(
                parentSelector: match =>
                {
                    if (match is XElement xmatch && xmatch.HasAttribute((XName)targetPropertyName))
                        return xmatch;
                    return null;
                },
                elementProcessor: target =>
                {
                    if (TryAppendToAttribute(target, "value", value))
                    {
                        Log.Info($"buff(s) from {PatchInfo.PatchingMod.Name} merged into block '{target.Parent?.GetAttribute((XName)"name")}'", false);
                        Merged = true;
                    }
                });

        }

    }
}
