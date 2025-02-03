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
		internal static bool IsHost => GameManager.IsDedicatedServer || SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;

		private OcdManager()
		{
			Log = new Logger(Name);

			// this is a 2ⁿ-keyed sparse lookup table of stacks (FILO queues) of IXmlPatchHandlers
			Handlers = new Dictionary<int, Dictionary<int, List<IXmlPatchHandler>>>();

			RegisterXMLPatchHandler(3, 255, ConflictDetector.Instance);
			RegisterXMLPatchHandler(3, 79, BuffsWhenWalkedOnMerger.Instance);
		}

		internal static OcdManager Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_lock)
					{
						if (_instance == null)
						{
							_instance = new OcdManager();
						}
					}
				}
				return _instance;
			}
		}
		internal Logger Log { get; }

		internal readonly Dictionary<int, Dictionary<int, List<IXmlPatchHandler>>> Handlers;

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
					Log.Info($"Queueing ({(HarmonyPatchType)(Math.Log(hbm_lowestBit, 2))} {(XMLPatchMethod)(Math.Log(xbm_lowestBit, 2))}) {handler.Name}", false);

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
			if (_targetFile == null
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

			if (Handlers[hbm]?[xbm] is not List<IXmlPatchHandler> handlerStack) return;

			foreach (var handler in handlerStack)
			{
				handler.Run(patchInfo);
				if (string.IsNullOrWhiteSpace(patchInfo.XPath)) return;
			}
		}
	}
}
