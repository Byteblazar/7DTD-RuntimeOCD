using Challenges;
using HarmonyLib;
using System.Reflection;
using System.Xml.Linq;

namespace RuntimeOCD
{
    public class RuntimeOCD : IModApi
    {
        public void InitMod(Mod mod)
        {
            new Harmony(GetType().ToString()).PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(ChallengesFromXml))]
    internal class HarmonyPatches_ChallengesFromXml
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ChallengesFromXml.ParseChallengeCategory))]
        static bool ParseChallengeCategory(ref XElement e)
        {
            if (!e.TryGetAttribute((XName)"name", out var name)) return true;
            if (ChallengeCategory.s_ChallengeCategories.ContainsKey(name))
            {
                ChallengeCategory.s_ChallengeCategories.Remove(name);
                OcdManager.Instance.Log.Info($"Challenge category '{name}' collision intercepted");
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(XmlPatchMethods))]
    internal class HarmonyPatches_XmlPatchMethods
    {
        private static OcdManager Ocd => OcdManager.Instance;

        // Appends new nodes as the children of the selected nodes or
        // Appends a new value to an attribute in the selected node
        [HarmonyPrefix]
        [HarmonyPatch(nameof(XmlPatchMethods.AppendByXPath))]
        static bool Prefix_AppendByXPath(
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
            return true;
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
        static bool Prefix_PrependByXPath(
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
            return true;
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
        static bool Prefix_InsertAfterByXPath(
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
            return true;
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
        static bool Prefix_InsertBeforeByXPath(
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
            return true;
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
        static bool Prefix_RemoveByXPath(
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
            return true;
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
        static bool Prefix_SetByXPath(
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
            return true;
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
        static bool Prefix_SetAttributeByXPath(
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
            return true;
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
        static bool Prefix_RemoveAttributeByXPath(
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
            return true;
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
