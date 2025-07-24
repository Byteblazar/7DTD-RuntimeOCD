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

namespace RuntimeOCD
{
	public class Config
	{
		public bool DetectConflicts { get; set; } = true;
		public bool DetectConflictsOnlyWhenModsChanged { get; set; } = true;
		public bool MergeBuffsWhenWalkedOn { get; set; } = true;
		public bool PreventChallengeCategoryCollisions { get; set; } = true;
		public bool ScreenEffectsCompatibility { get; set; } = true;

		public static Config Load()
		{
			if (!Directory.Exists(LogsPath))
				Directory.CreateDirectory(LogsPath);

			var cfg = new Config();
			if (!File.Exists(Path)) return cfg;

			string json = File.ReadAllText(Path);
			JsonConvert.PopulateObject(json, cfg);
			return cfg;
		}

		public void Save()
		{
			// ensure directory exists
			var dir = System.IO.Path.GetDirectoryName(Path)!;
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			using var sw = new StreamWriter(Path);
			using var writer = new JsonTextWriter(sw)
			{
				Formatting = Formatting.Indented
			};

			// serialize the object
			var serializer = new JsonSerializer();
			serializer.Serialize(writer, this);
		}

		public static string DataPath { get; } =
			System.IO.Path.Combine(GameIO.GetUserGameDataDir(), "RuntimeOCD");
		public static string LogsPath { get; } =
			System.IO.Path.Combine(DataPath, "logs");
		public static string Path { get; } =
			System.IO.Path.Combine(DataPath, "settings.json");
	}
}
