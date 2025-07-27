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
				ID = $"{name}";
				if (typeof(MinEventParams).HasField("ParentType")) ID += $"{p.ParentType}";

				if (typeof(MinEventParams).HasField("Buff") && !string.IsNullOrEmpty(p.Buff?.BuffName)) ID += $"{p.Buff?.BuffName}"; // prop
				else if (typeof(MinEventParams).HasField("Instigator") && !string.IsNullOrEmpty($"{p.Instigator?.entityId}")) ID += $"{p.Instigator?.entityId}";
				else if (typeof(MinEventParams).HasField("Self") && !string.IsNullOrEmpty($"{p.Self?.entityId}")) ID += $"{p.Self?.entityId}";
				else if (typeof(MinEventParams).HasField("ItemInventoryData") && typeof(ItemInventoryData).HasField("item") && !string.IsNullOrEmpty($"{p.ItemInventoryData?.item?.GetItemName()}")) ID += $"{p.ItemInventoryData?.item?.GetItemName()}";
				else if (typeof(MinEventParams).HasField("ItemValue") && !string.IsNullOrEmpty($"{p.ItemValue?.GetItemId()}")) ID += $"{p.ItemValue?.GetItemId()}";
			}
		}
	}
}
