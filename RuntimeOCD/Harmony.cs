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

using Challenges;
using HarmonyLib;
using System.Reflection;
using System.Xml.Linq;

namespace RuntimeOCD
{
	public class RuntimeOCD : IModApi
	{
		public static Harmony? harmony;
		public void InitMod(Mod mod)
		{
			harmony = new Harmony(GetType().ToString());
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}
	/*
	[HarmonyPatch(typeof(XUiC_MainMenu), nameof(XUiC_MainMenu.OnOpen))]
	public class HarmonyPatches_MainMenu
	{
		private static void Postfix(XUiC_MainMenu __instance)
		{
			OcdManager ocd = OcdManager.Instance;
		}
	}
	*/

	public class ChallengesFromXml_Patches
	{
		public static void Prefix_ParseChallengeCategory(ref XElement e)
		{
			if (!e.TryGetAttribute((XName)"name", out var name)) return;
			if (ChallengeCategory.s_ChallengeCategories.ContainsKey(name))
			{
				ChallengeCategory.s_ChallengeCategories.Remove(name);
				OcdManager.Instance.Log.Info($"Challenge category '{name}' collision intercepted");
			}
		}
	}

	[HarmonyPatch(typeof(XmlPatchMethods))]
	public class XmlPatchMethods_Patches
	{
		private static OcdManager Ocd => OcdManager.Instance;

		// Appends new nodes as the children of the selected nodes or
		// Appends a new value to an attribute in the selected node
		[HarmonyPrefix]
		[HarmonyPatch(nameof(XmlPatchMethods.AppendByXPath))]
		static void Prefix_AppendByXPath(
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			ref object __state)
		{
			Ocd.ProcessPatchInfo(
				HarmonyPatchType.Prefix,
				XMLPatchMethod.Append,
				ref _targetFile,
				ref _xpath,
				ref _patchSourceElement,
				_patchingMod,
				ref __result,
				ref __state);
		}

		[HarmonyPostfix]
		[HarmonyPatch(nameof(XmlPatchMethods.AppendByXPath))]
		static void Postfix_AppendByXPath(
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			object __state)
		{
			Ocd.ProcessPatchInfo(
				HarmonyPatchType.Postfix,
				XMLPatchMethod.Append,
				ref _targetFile,
				ref _xpath,
				ref _patchSourceElement,
				_patchingMod,
				ref __result,
				ref __state);
		}

		// Prepends new nodes as the children of the selected nodes or
		// Prepends a new value to an attribute in the selected node
		[HarmonyPrefix]
		[HarmonyPatch(nameof(XmlPatchMethods.PrependByXPath))]
		static void Prefix_PrependByXPath(
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			ref object __state)
		{
			Ocd.ProcessPatchInfo(
				HarmonyPatchType.Prefix,
				XMLPatchMethod.Prepend,
				ref _targetFile,
				ref _xpath,
				ref _patchSourceElement,
				_patchingMod,
				ref __result,
				ref __state);
		}
		[HarmonyPostfix]
		[HarmonyPatch(nameof(XmlPatchMethods.PrependByXPath))]
		static void Postfix_PrependByXPath(
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			object __state)
		{
			Ocd.ProcessPatchInfo(
				HarmonyPatchType.Postfix,
				XMLPatchMethod.Prepend,
				ref _targetFile,
				ref _xpath,
				ref _patchSourceElement,
				_patchingMod,
				ref __result,
				ref __state);
		}

		// Inserts new nodes after the selected nodes as siblings
		[HarmonyPrefix]
		[HarmonyPatch(nameof(XmlPatchMethods.InsertAfterByXPath))]
		static void Prefix_InsertAfterByXPath(
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			ref object __state)
		{
			Ocd.ProcessPatchInfo(
				HarmonyPatchType.Prefix,
				XMLPatchMethod.InsertAfter,
				ref _targetFile,
				ref _xpath,
				ref _patchSourceElement,
				_patchingMod,
				ref __result,
				ref __state);
		}
		[HarmonyPostfix]
		[HarmonyPatch(nameof(XmlPatchMethods.InsertAfterByXPath))]
		static void Postfix_InsertAfterByXPath(
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			object __state)
		{
			Ocd.ProcessPatchInfo(
				HarmonyPatchType.Postfix,
				XMLPatchMethod.InsertAfter,
				ref _targetFile,
				ref _xpath,
				ref _patchSourceElement,
				_patchingMod,
				ref __result,
				ref __state);
		}

		// Inserts new nodes before the selected nodes as siblings
		[HarmonyPrefix]
		[HarmonyPatch(nameof(XmlPatchMethods.InsertBeforeByXPath))]
		static void Prefix_InsertBeforeByXPath(
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			ref object __state)
		{
			Ocd.ProcessPatchInfo(
				HarmonyPatchType.Prefix,
				XMLPatchMethod.InsertBefore,
				ref _targetFile,
				ref _xpath,
				ref _patchSourceElement,
				_patchingMod,
				ref __result,
				ref __state);
		}
		[HarmonyPostfix]
		[HarmonyPatch(nameof(XmlPatchMethods.InsertBeforeByXPath))]
		static void Postfix_InsertBeforeByXPath(
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			object __state)
		{
			Ocd.ProcessPatchInfo(
				HarmonyPatchType.Postfix,
				XMLPatchMethod.InsertBefore,
				ref _targetFile,
				ref _xpath,
				ref _patchSourceElement,
				_patchingMod,
				ref __result,
				ref __state);
		}

		// Removes the selected nodes and all of their contents/descendants
		[HarmonyPrefix]
		[HarmonyPatch(nameof(XmlPatchMethods.RemoveByXPath))]
		static void Prefix_RemoveByXPath(
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			ref object __state)
		{
			Ocd.ProcessPatchInfo(
				HarmonyPatchType.Prefix,
				XMLPatchMethod.Remove,
				ref _targetFile,
				ref _xpath,
				ref _patchSourceElement,
				_patchingMod,
				ref __result,
				ref __state);
		}
		[HarmonyPostfix]
		[HarmonyPatch(nameof(XmlPatchMethods.RemoveByXPath))]
		static void Postfix_RemoveByXPath(
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			object __state)
		{
			Ocd.ProcessPatchInfo(
				HarmonyPatchType.Postfix,
				XMLPatchMethod.Remove,
				ref _targetFile,
				ref _xpath,
				ref _patchSourceElement,
				_patchingMod,
				ref __result,
				ref __state);
		}

		// Modifies the selected nodes or attributes (but does not add new ones)
		[HarmonyPrefix]
		[HarmonyPatch(nameof(XmlPatchMethods.SetByXPath))]
		static void Prefix_SetByXPath(
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			ref object __state)
		{
			Ocd.ProcessPatchInfo(
				HarmonyPatchType.Prefix,
				XMLPatchMethod.Set,
				ref _targetFile,
				ref _xpath,
				ref _patchSourceElement,
				_patchingMod,
				ref __result,
				ref __state);
		}
		[HarmonyPostfix]
		[HarmonyPatch(nameof(XmlPatchMethods.SetByXPath))]
		static void Postfix_SetByXPath(
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			object __state)
		{
			Ocd.ProcessPatchInfo(
				HarmonyPatchType.Postfix,
				XMLPatchMethod.Set,
				ref _targetFile,
				ref _xpath,
				ref _patchSourceElement,
				_patchingMod,
				ref __result,
				ref __state);
		}

		// Adds or modifies attributes on the selected nodes
		[HarmonyPrefix]
		[HarmonyPatch(nameof(XmlPatchMethods.SetAttributeByXPath))]
		static void Prefix_SetAttributeByXPath(
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			ref object __state)
		{
			Ocd.ProcessPatchInfo(
				HarmonyPatchType.Prefix,
				XMLPatchMethod.SetAttribute,
				ref _targetFile,
				ref _xpath,
				ref _patchSourceElement,
				_patchingMod,
				ref __result,
				ref __state);
		}
		[HarmonyPostfix]
		[HarmonyPatch(nameof(XmlPatchMethods.SetAttributeByXPath))]
		static void Postfix_SetAttributeByXPath(
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			object __state)
		{
			Ocd.ProcessPatchInfo(
				HarmonyPatchType.Postfix,
				XMLPatchMethod.SetAttribute,
				ref _targetFile,
				ref _xpath,
				ref _patchSourceElement,
				_patchingMod,
				ref __result,
				ref __state);
		}

		// Removes an attribute in the selected nodes
		[HarmonyPrefix]
		[HarmonyPatch(nameof(XmlPatchMethods.RemoveAttributeByXPath))]
		static void Prefix_RemoveAttributeByXPath(
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			ref object __state)
		{
			Ocd.ProcessPatchInfo(
				HarmonyPatchType.Prefix,
				XMLPatchMethod.RemoveAttribute,
				ref _targetFile,
				ref _xpath,
				ref _patchSourceElement,
				_patchingMod,
				ref __result,
				ref __state);
		}
		[HarmonyPostfix]
		[HarmonyPatch(nameof(XmlPatchMethods.RemoveAttributeByXPath))]
		static void Postfix_RemoveAttributeByXPath(
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			object __state)
		{
			Ocd.ProcessPatchInfo(
				HarmonyPatchType.Postfix,
				XMLPatchMethod.RemoveAttribute,
				ref _targetFile,
				ref _xpath,
				ref _patchSourceElement,
				_patchingMod,
				ref __result,
				ref __state);
		}

		/*
        // Performs add or remove operations on CSV data in the selected node
        [HarmonyPrefix]
        [HarmonyPatch(nameof(XmlPatchMethods.CsvOperationsByXPath))]
        static bool Prefix_CsvOperationsByXPath(
            ref XmlFile _targetFile,
            ref string _xpath,
            ref XElement _patchSourceElement,
            Mod _patchingMod)
        {
        }
        // Conditionally applies any kind of patch. Should still fire the other events if conditions are true, so ignored.
        [HarmonyPrefix]
        [HarmonyPatch(nameof(XmlPatchMethods.Conditional))]
        static bool Prefix_Conditional(
            ref XmlFile _targetFile,
            ref string _xpath,
            ref XElement _patchSourceElement,
            Mod _patchingMod)
        {
        }

        // Includes XML patches as sort of modules. No need to patch.
        [HarmonyPrefix]
        [HarmonyPatch(nameof(XmlPatchMethods.Include))]
        static bool Prefix_Include(
            ref XmlFile _targetFile,
            ref string _xpath,
            ref XElement _patchSourceElement,
            Mod _patchingMod)
        {
        }
        */
	}
}
