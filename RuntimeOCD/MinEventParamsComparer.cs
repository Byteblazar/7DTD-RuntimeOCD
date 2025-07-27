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
	public abstract class MinEventParamsComparer
	{
		public virtual string GetID(MinEventParams p, string name = "")
		{
			if (p == null) return "hardcoded";

			string id = $"{name}";

			if (typeof(MinEventParams).HasField("ParentType"))
			{
				id += $"@{p.ParentType}";
				switch (p.ParentType)
				{
					case MinEffectController.SourceParentType.BuffClass:
						if (typeof(MinEventParams).HasField("Buff") && !string.IsNullOrEmpty(p.Buff?.BuffName)) id += $"@{p.Buff?.BuffName}"; // prop
						break;
					case MinEffectController.SourceParentType.ItemClass:
						if (typeof(MinEventParams).HasField("ItemInventoryData") && typeof(ItemInventoryData).HasField("item") && !string.IsNullOrEmpty($"{p.ItemInventoryData?.item?.GetItemName()}")) id += $"@{p.ItemInventoryData?.item?.GetItemName()}";
						else if (typeof(MinEventParams).HasField("ItemValue") && !string.IsNullOrEmpty($"{p.ItemValue?.GetItemId()}")) id += $"@{p.ItemValue?.GetItemId()}";
						break;
					default:
						if (typeof(MinEventParams).HasField("Instigator") && !string.IsNullOrEmpty($"{p.Instigator?.entityId}")) id += $"@{p.Instigator?.entityId}";
						else if (typeof(MinEventParams).HasField("Self") && !string.IsNullOrEmpty($"{p.Self?.entityId}")) id += $"@{p.Self?.entityId}";
						break;
				}
			}
			else
			{
				if (typeof(MinEventParams).HasField("Buff") && !string.IsNullOrEmpty(p.Buff?.BuffName)) id += $"@{p.Buff?.BuffName}"; // prop
				else if (typeof(MinEventParams).HasField("Instigator") && !string.IsNullOrEmpty($"{p.Instigator?.entityId}")) id += $"@{p.Instigator?.entityId}";
				else if (typeof(MinEventParams).HasField("Self") && !string.IsNullOrEmpty($"{p.Self?.entityId}")) id += $"@{p.Self?.entityId}";
				else if (typeof(MinEventParams).HasField("ItemInventoryData") && typeof(ItemInventoryData).HasField("item") && !string.IsNullOrEmpty($"{p.ItemInventoryData?.item?.GetItemName()}")) id += $"@{p.ItemInventoryData?.item?.GetItemName()}";
				else if (typeof(MinEventParams).HasField("ItemValue") && !string.IsNullOrEmpty($"{p.ItemValue?.GetItemId()}")) id += $"@{p.ItemValue?.GetItemId()}";
			}
			return id;
		}
	}
}
