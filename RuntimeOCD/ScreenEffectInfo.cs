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

namespace RuntimeOCD
{
	public class ScreenEffectInfo
	{
		public float Intensity { get; set; }
		public float FadeTime { get; set; }
		public string ID { get; set; }
		public ScreenEffectInfo(string name, float intensity, float fadeTime)
		{
			Intensity = intensity;
			FadeTime = fadeTime;
			if (MinEventActionModifyScreenEffect_Patches.MSEParams == null)
				ID = "hardcoded";
			else
			{
				MinEventParams p = MinEventActionModifyScreenEffect_Patches.MSEParams;
				ID = $"{name}{p.ParentType}§{p.Buff?.buffName}§{p.Instigator?.entityId}§{p.Self?.entityId}§{p.ItemValue?.GetItemId()}§{p.ItemInventoryData?.item?.GetItemName()}";
			}
		}
	}
}
