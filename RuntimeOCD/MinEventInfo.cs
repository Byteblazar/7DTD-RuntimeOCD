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
	public class MinEventInfo
	{
		public virtual string ID { get; set; }
		public MinEventInfo(string name, string source = "hardcoded")
		{
			ID = $"{name}@{source}";
		}
		public static string GetSource(ref XAttribute _attribute)
		{
			string id = string.Empty;
			string name = string.Empty;
			string nameAttribute = string.Empty;
			XElement? ancestor = _attribute.Parent;

			while (ancestor != null)
			{
				switch (ancestor?.Name?.ToString())
				{
					case "buff":
					case "entity_class":
					case "item":
					case "item_modifier":
					case "perk":
						name = ancestor.Name.ToString();
						ancestor.TryGetAttribute("name", out nameAttribute);
						ancestor = null;
						break;
					default:
						ancestor = ancestor?.Parent;
						continue;
				}
			}

			if (!string.IsNullOrWhiteSpace(nameAttribute))
				id = $"{nameAttribute}@{name}";

			return id;
		}
	}
}
