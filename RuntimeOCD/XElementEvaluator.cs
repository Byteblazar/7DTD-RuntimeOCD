using System.Xml.Linq;

namespace RuntimeOCD
{
    public class XElementEvaluator
    {
        private readonly HashSet<string> _moddedBy;
        public XElementEvaluator(XElement element)
        {
            Element = element;
            _moddedBy = new();
        }
        public XElement Element { get; }

        public bool IsNamesakeOf(XElement x)
        {
            return x.Name == Element.Name
                && Element.TryGetAttribute("name", out string myNameAttribute)
                && x.TryGetAttribute("name", out string itsNameAttribute)
                && myNameAttribute == itsNameAttribute;
        }
        public bool SetModdedBy(string modName)
        {
            return _moddedBy.Add(modName);
        }
        public bool GetOtherModNames(string modName, out HashSet<string> otherMods)
        {
            otherMods = new HashSet<string>(_moddedBy);
            otherMods.Remove(modName);
            return otherMods.Any();
        }
    }
}
