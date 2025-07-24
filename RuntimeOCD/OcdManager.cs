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

using HarmonyLib;
using System.Xml.Linq;

namespace RuntimeOCD
{
	public enum HarmonyPatchType
	{
		Prefix, //1
		Postfix //2
	}
	public enum XMLPatchMethod
	{
		Append,         //1
		Prepend,        //2
		InsertAfter,    //4
		InsertBefore,   //8

		Remove,         //16
		Set,            //32
		SetAttribute,   //64
		RemoveAttribute,//128

		Csv,            //256
		Conditional,    //512
		Include         //1024
	}
	public sealed class OcdManager
	{
		private static OcdManager? _instance;
		private static readonly object _lock = new();
		public const string Name = "RuntimeOCD";
		public static bool IsHost => GameManager.IsDedicatedServer || SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		public static OcdManager Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_lock)
					{
						_instance ??= new OcdManager();
					}
				}
				return _instance;
			}
		}
		private OcdManager()
		{
			Cfg = Config.Load();
			Meta = Metadata.Load();
			Log = new Logger();

			// this is a 2ⁿ-keyed sparse lookup table of stacks (FILO queues) of IXmlPatchHandlers
			Handlers = new Dictionary<int, Dictionary<int, List<IXmlPatchHandler>>>();

			if (Cfg.DetectConflicts)
			{
				string? lohash = null;
				if (Cfg.DetectConflictsOnlyWhenModsChanged)
					lohash = Metadata.GetLoadOrderHash();

				if (Meta.LastLoadOrder != lohash)
				{
					Meta.LastLoadOrder = lohash ?? string.Empty;
					Meta.Save();
					RegisterXMLPatchHandler(3, 255, ConflictDetector.Instance);
				}
			}

			if (Cfg.MergeBuffsWhenWalkedOn)
				RegisterXMLPatchHandler(3, 79, BuffsWhenWalkedOnMerger.Instance);

			if (Cfg.PreventChallengeCategoryCollisions)
			{
				var original = AccessTools.Method(typeof(ChallengesFromXml), nameof(ChallengesFromXml.ParseChallengeCategory), new Type[] { typeof(XElement) });
				var prefix = AccessTools.Method(typeof(ChallengesFromXml_Patches), nameof(ChallengesFromXml_Patches.Prefix_ParseChallengeCategory));
				RuntimeOCD.harmony?.Patch(
					original,
					prefix: new HarmonyMethod(prefix));
			}

			if (Cfg.ScreenEffectsCompatibility)
			{
				ModEvents.MainMenuOpened.RegisterHandler(ScreenEffects_Patches.ScreenEffectsReset);
				var original = AccessTools.Method(typeof(ScreenEffects), nameof(ScreenEffects.SetScreenEffect), new Type[] { typeof(string), typeof(float), typeof(float) });
				var prefix = AccessTools.Method(typeof(ScreenEffects_Patches), nameof(ScreenEffects_Patches.Prefix_SetScreenEffect));
				RuntimeOCD.harmony?.Patch(
					original,
					prefix: new HarmonyMethod(prefix));

				original = AccessTools.Method(typeof(MinEventActionModifyScreenEffect), nameof(MinEventActionModifyScreenEffect.Execute), new Type[] { typeof(MinEventParams) });
				prefix = AccessTools.Method(typeof(MinEventActionModifyScreenEffect_Patches), nameof(MinEventActionModifyScreenEffect_Patches.Prefix_Execute));
				var postfix = AccessTools.Method(typeof(MinEventActionModifyScreenEffect_Patches), nameof(MinEventActionModifyScreenEffect_Patches.Postfix_Execute));
				RuntimeOCD.harmony?.Patch(
					original,
					prefix: new HarmonyMethod(prefix),
					postfix: new HarmonyMethod(postfix));
			}

			Cfg.Save();
		}

		public Config Cfg { get; }
		public Metadata Meta { get; }
		public Logger Log { get; }
		public bool LoadOrderChanged { get; }
		public Dictionary<int, Dictionary<int, List<IXmlPatchHandler>>> Handlers { get; }

		public void RegisterXMLPatchHandler(
			HarmonyPatchType harmonyPatchType,
			XMLPatchMethod XMLpatchMethod,
			IXmlPatchHandler handler)
		{
			int harmonyPatchTypeBitMask = 1 << (int)harmonyPatchType;
			int XMLpatchMethodBitMask = 1 << (int)XMLpatchMethod;

			RegisterXMLPatchHandler(harmonyPatchTypeBitMask, XMLpatchMethodBitMask, handler);
		}

		public void RegisterXMLPatchHandler(int harmonyPatchTypeBitMask, int XMLpatchMethodBitMask, IXmlPatchHandler handler)
		{
			int hbm = harmonyPatchTypeBitMask;

			while (hbm > 0)
			{
				int hbm_lowestBit = hbm & -hbm;

				if (!Handlers.TryGetValue(hbm_lowestBit, out var patchMethodDict))
				{
					patchMethodDict = new Dictionary<int, List<IXmlPatchHandler>>();
					Handlers[hbm_lowestBit] = patchMethodDict;
				}

				int xbm = XMLpatchMethodBitMask;

				while (xbm > 0)
				{
					int xbm_lowestBit = xbm & -xbm;

					if (!patchMethodDict.TryGetValue(xbm_lowestBit, out var handlerStack))
					{
						handlerStack = new List<IXmlPatchHandler>();
						patchMethodDict[xbm_lowestBit] = handlerStack;
					}

					handlerStack.Insert(0, handler);
					Log.Info($"Queueing ({(HarmonyPatchType)(Math.Log(hbm_lowestBit, 2))} {(XMLPatchMethod)(Math.Log(xbm_lowestBit, 2))}) {handler.Name}");

					xbm &= (xbm - 1);
				}

				hbm &= (hbm - 1);
			}
		}

		internal void ProcessPatchInfo(
			HarmonyPatchType _patchType,
			XMLPatchMethod _methodType,
			ref XmlFile _targetFile,
			ref string _xpath,
			ref XElement _patchSourceElement,
			Mod _patchingMod,
			ref int __result,
			ref object __state
			)
		{
			if (
				Handlers.Count == 0
				|| _targetFile == null
				|| _patchSourceElement == null
				|| string.IsNullOrWhiteSpace(_patchingMod?.Name)
				|| string.IsNullOrWhiteSpace(_xpath))
			{
				return;
			}

			if (_patchType == HarmonyPatchType.Prefix)
				__state = new();
			else if (__state == null)
				Log.Error($"Another client-side mod seems to be preventing RuntimeOCD from running properly. Yikes.");

			PatchInfo patchInfo = new PatchInfo(_methodType, _patchType, _targetFile, _xpath, _patchSourceElement, _patchingMod, __result, __state);
			RunHandlers(patchInfo);
			_targetFile = patchInfo.TargetFile;
			_xpath = patchInfo.XPath;
			_patchSourceElement = patchInfo.PatchSourceElement;
			__result = patchInfo.Result;
		}

		private void RunHandlers(PatchInfo patchInfo)
		{
			int hbm = 1 << (int)patchInfo.PatchType;
			int xbm = 1 << (int)patchInfo.MethodType;

			if (!Handlers.TryGetValue(hbm, out Dictionary<int, List<IXmlPatchHandler>> d1)) return;
			if (!d1.TryGetValue(xbm, out List<IXmlPatchHandler> d2)) return;

			foreach (var handler in d2)
			{
				handler.Run(patchInfo);
				if (string.IsNullOrWhiteSpace(patchInfo.XPath)) return;
			}
		}
	}
}
