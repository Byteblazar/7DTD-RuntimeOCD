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
	public sealed class ExplosionBuffsMerger : PropertyMergerBase
	{
		private static ExplosionBuffsMerger? _instance;
		private static readonly object _lock = new();

		public static ExplosionBuffsMerger Instance
		{
			get
			{
				if (_instance == null)
					lock (_lock)
						_instance ??= new ExplosionBuffsMerger();
				return _instance;
			}
		}

		private ExplosionBuffsMerger()
			: base(componentName: "ExplosionBuffs", targetPropertyName: "Explosion.Buffs", delimiter: ',')
		{ }

		public override string Name => "ExplosionBuffs";
	}
}
