using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RuntimeOCD
{
    public abstract class PatchHandler : IXmlPatchHandler
    {
        public virtual string Name { get { return "Component"; } }
        protected virtual PatchInfo PatchInfo { get; set; }
        protected virtual List<XObject>? MatchList { get; set; }
        protected virtual Logger Log { get; set; }

        public abstract void Run(PatchInfo patchInfo);

        protected virtual bool IsUsingSetToReplaceNodes()
        {
            return PatchInfo.MethodType == XMLPatchMethod.Set && !Regex.IsMatch(PatchInfo.XPath, @".*/@\w+$");
        }

        protected virtual void AnalyzeMatchedElements(Func<XObject, XElement?> parentSelector, Action<XElement> elementProcessor)
        {
            foreach (XObject match in MatchList)
            {
                var parent = parentSelector(match);
                if (parent != null)
                {
                    elementProcessor(parent);
                }
            }
        }

        protected virtual bool TryGetXAttribute(XObject obj, string attributeName, out XAttribute? xAttribute)
        {
            switch (obj)
            {
                case XElement xElement:
                    xAttribute = xElement.Attribute(attributeName);
                    return xAttribute != null;
                case XAttribute attr when attr.Name == attributeName:
                    xAttribute = attr;
                    return true;
                default:
                    xAttribute = null;
                    return false;
            }
        }

        protected virtual bool TryAppendToAttribute(XObject obj, string attributeName, string attributeValue, char separator = ';')
        {
            if (TryGetXAttribute(obj, attributeName, out XAttribute? xAttribute))
            {
                HashSet<string> attributes = new(xAttribute.Value.Split(separator));
                attributes.UnionWith(attributeValue.Split(separator));

                xAttribute.SetValue(string.Join(separator.ToString(), attributes));
                return true;
            }
            return false;
        }
    }
}
