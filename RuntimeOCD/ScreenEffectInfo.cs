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
	public class ScreenEffectInfo : MinEventInfo
	{
		public float Intensity { get; set; }
		public float Fade { get; set; }
		public ScreenEffectInfo(string name, float intensity, float fade, string source = "hardcoded") : base(name: name, source: source)
		{
			Intensity = intensity;
			Fade = fade;
		}
	}
}
