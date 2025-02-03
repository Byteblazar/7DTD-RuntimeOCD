/*
	This file is part of RuntimeOCD.

	RuntimeOCD is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

	RuntimeOCD is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

	You should have received a copy of the GNU General Public License along with RuntimeOCD. If not, see <https://www.gnu.org/licenses/>. 
*/

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
		public object State { get; }
		public bool MatchesState(object state)
		{
			return state != null && ReferenceEquals(State, state);
		}
	}
}
