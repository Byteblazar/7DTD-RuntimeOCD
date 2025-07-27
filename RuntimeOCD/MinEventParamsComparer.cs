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
	public class MinEventParamsComparer
	{
		public virtual string GetID(MinEventParams p, string name = "")
		{
			if (p == null) return "hardcoded";

			string id = $"{name}";

			if (ReflectionHelpers.TryGetMemberValue<MinEffectController.SourceParentType>(p, "ParentType", out var pType))
			{
				id += $"@{pType}";
				switch (pType)
				{
					case MinEffectController.SourceParentType.BuffClass:
						if(ReflectionHelpers.TryGetNestedMemberValue<string>(p, "Buff", "BuffName", out var buffName) && !string.IsNullOrEmpty(buffName))
							id += $"@{buffName}"; // prop
						break;
					case MinEffectController.SourceParentType.ItemClass:
						if (ReflectionHelpers.TryGetNestedMemberValue<ItemClass>(p, "ItemInventoryData", "item", out var item) && !string.IsNullOrEmpty(item.GetItemName()))
							id += $"@{item.GetItemName()}";
						else if (ReflectionHelpers.TryGetMemberValue<ItemValue>(p, "ItemValue", out var itemValue) && !string.IsNullOrEmpty($"{itemValue.GetItemId()}"))
							id += $"@{itemValue.GetItemId()}";
						break;
					default:
						if(ReflectionHelpers.TryGetNestedMemberValue<int>(p, "Instigator", "entityId", out var entityId) && !string.IsNullOrEmpty($"{entityId}"))
							id += $"@{entityId}";
						if (ReflectionHelpers.TryGetNestedMemberValue<int>(p, "Self", "entityId", out var selfId) && !string.IsNullOrEmpty($"{selfId}"))
							id += $"@{selfId}";
						break;
				}
			}
			else
				throw new Exception("RuntimeOCD has no idea what the hell this is. Contact Byteblazar asap.");

			return id;
		}
	}
}
