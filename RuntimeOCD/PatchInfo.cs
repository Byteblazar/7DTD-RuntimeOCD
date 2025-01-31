using System.Xml.Linq;

namespace RuntimeOCD
{
    public class PatchInfo
    {
        public PatchInfo(
            XMLPatchMethod _methodType,
            HarmonyPatchType _patchType,
            XmlFile _targetFile,
            string _xpath,
            XElement _patchSourceElement,
            Mod _patchingMod,
            int __result,
            object __state)
        {
            MethodType = _methodType;
            PatchType = _patchType;
            TargetFile = _targetFile;
            XPath = _xpath;
            PatchSourceElement = _patchSourceElement;
            PatchingMod = _patchingMod;
            Result = __result;
            State = __state;
        }
        public XMLPatchMethod MethodType { get; }
        public HarmonyPatchType PatchType { get; }
        public XmlFile TargetFile { get; set; }
        public string XPath { get; set; }
        public XElement PatchSourceElement { get; set; }
        public Mod PatchingMod { get; set; }
        public int Result { get; set; }
        public object State {  get; }
        public bool MatchesState(object state)
        {
            return state != null && ReferenceEquals(State, state);
        }
    }
}
