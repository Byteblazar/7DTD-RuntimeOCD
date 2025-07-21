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

using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace RuntimeOCD
{
	public class Metadata
	{
		public string LastLoadOrder { get; set; } = string.Empty;
		public ConflictDetector.ConflictsTally LastTally { get; set; } = new();
		public static Metadata Load()
		{
			if (!Directory.Exists(Config.LogsPath)) Directory.CreateDirectory(Config.LogsPath);
			if (!File.Exists(Path))
				return new Metadata();

			string json = File.ReadAllText(Path);
			return JsonConvert.DeserializeObject<Metadata>(json) ?? new Metadata();
		}

		public void Save()
		{
			var json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(Path, json);
		}

		public static string Path { get; } = System.IO.Path.Combine(Config.LogsPath, "meta.json");
		public static string GetMD5Hash(string input)
		{
			using (var md5 = MD5.Create())
			{
				byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
				var sb = new StringBuilder(data.Length * 2);
				foreach (byte b in data)
					sb.Append(b.ToString("x2"));
				return sb.ToString();
			}
		}
		public static string GetLoadOrderHash()
		{
			string lo = string.Empty;
			List<Mod> tmp = ModManager.loadedMods.list;
			for (int i = 0; i < tmp.Count; i++)
			{
				lo += $"{tmp[i].Author}{tmp[i].Name}{tmp[i].Version}";
			}
			return GetMD5Hash(lo);
		}
	}
}
