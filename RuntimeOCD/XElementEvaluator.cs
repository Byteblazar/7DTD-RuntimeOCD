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
			otherMods = new(_moddedBy);
			otherMods.Remove(modName);
			return otherMods.Any();
		}
	}
}
