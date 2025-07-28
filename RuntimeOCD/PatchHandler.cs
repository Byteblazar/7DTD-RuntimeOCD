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

using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RuntimeOCD
{
	public abstract class PatchHandler : IXmlPatchHandler
	{
		public virtual string Name { get { return "Component"; } }
		public virtual PatchInfo? PatchInfo { get; set; }
		public virtual List<XObject>? MatchList { get; set; }
		public virtual Logger? Log { get; set; }

		public abstract void Run(PatchInfo patchInfo);

		public virtual bool IsUsingSetToReplaceNodes()
		{
			return PatchInfo.MethodType == XMLPatchMethod.Set && !Regex.IsMatch(PatchInfo.XPath, @".*/@\w+$");
		}

		public virtual bool TryGetXAttribute(XObject obj, string attributeName, out XAttribute? xAttribute)
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

		public virtual bool TryAppendToAttribute(XObject obj, string attributeName, string attributeValue, char separator = ';')
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
