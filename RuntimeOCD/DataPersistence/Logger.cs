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

using UnityEngine;

namespace RuntimeOCD
{
	public class Logger
	{
		public Logger(string name = OcdManager.Name, string componentName = "", bool hostOnly = false)
		{
			ModName = name;
			Component = componentName;
			DefaultPath = Path.Combine(Config.LogsPath, $"{Component}_log.txt");
			MsgPrefix = Component == "" ? $"[{ModName}] " : $"[{ModName}.{Component}] ";
			HostOnly = hostOnly;
		}
		public virtual string ModName { get; } = string.Empty;
		public virtual string Component { get; } = string.Empty;
		public virtual string MsgPrefix { get; } = string.Empty;
		public virtual string DefaultPath { get; } = Path.Combine(Config.LogsPath, "log.txt");
		public virtual bool HostOnly { get; set; }
		public virtual Dictionary<string, List<string>> LogFiles { get; } = new(); //key = path, value = contents (line-by-line)
		public virtual void AddLine(string message)
		{
			AddLine(message, DefaultPath);
		}
		public virtual void AddLine(string message, string path)
		{
			if (HostOnly && !OcdManager.IsHost) return;
			if (!Path.IsPathRooted(path))
				path = Path.Combine(Config.LogsPath, path);
			if (!LogFiles.ContainsKey(path)) LogFiles.Add(path, new List<string>());
			LogFiles[path].Add(message);
		}
		public virtual void AddLine(IEnumerable<string> multiMessage, string prefix = "")
		{
			AddLine(multiMessage, prefix, DefaultPath);
		}
		public virtual void AddLine(IEnumerable<string> multiMessage, string prefix, string path)
		{
			if (HostOnly && !OcdManager.IsHost) return;
			foreach (var line in multiMessage)
			{
				AddLine($"{prefix}{line}", path);
			}
		}
		public virtual void WriteLogFiles()
		{
			if (HostOnly && !OcdManager.IsHost) return;
			foreach (string path in LogFiles.Keys)
			{
				string dirName = Path.GetDirectoryName(path);
				if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
				StreamWriter writer = new(path);
				try
				{
					foreach (string line in LogFiles[path])
					{
						writer.WriteLine(line);
					}
				}
				finally
				{
					writer.Close();
				}
			}
			LogFiles.Clear();
		}
		public virtual void Info(string message)
		{
			if (!HostOnly || OcdManager.IsHost) Log.Out(MsgPrefix + message);
		}
		public virtual void Info(IEnumerable<string> multiMessage)
		{
			if (multiMessage == null || HostOnly && !OcdManager.IsHost) return;

			foreach (var msg in multiMessage)
			{
				Debug.Log(MsgPrefix + msg);
			}
		}
		public virtual void Info(IEnumerable<string> multiMessage, string prefix)
		{
			if (multiMessage == null || HostOnly && !OcdManager.IsHost) return;

			foreach (var msg in multiMessage)
			{
				Debug.Log(MsgPrefix + prefix + msg);
			}
		}
		public virtual void Warn(string message)
		{
			if (!HostOnly || OcdManager.IsHost) Log.Warning(MsgPrefix + message);
		}
		public virtual void Warn(IEnumerable<string> multiMessage)
		{
			if (multiMessage == null || HostOnly && !OcdManager.IsHost) return;

			foreach (var msg in multiMessage)
			{
				Debug.LogWarning(MsgPrefix + msg);
			}
		}
		public virtual void Warn(IEnumerable<string> multiMessage, string prefix)
		{
			if (multiMessage == null || HostOnly && !OcdManager.IsHost) return;

			foreach (var msg in multiMessage)
			{
				Debug.LogWarning(MsgPrefix + prefix + msg);
			}
		}
		public virtual void Error(string message)
		{
			if (!HostOnly || OcdManager.IsHost) Log.Error(MsgPrefix + message);
		}
		public virtual void Error(IEnumerable<string> multiMessage)
		{
			if (multiMessage == null || HostOnly && !OcdManager.IsHost) return;

			foreach (var msg in multiMessage)
			{
				Debug.LogError(MsgPrefix + msg);
			}
		}
		public virtual void Error(IEnumerable<string> multiMessage, string prefix)
		{
			if (multiMessage == null || HostOnly && !OcdManager.IsHost) return;

			foreach (var msg in multiMessage)
			{
				Debug.LogError(MsgPrefix + prefix + msg);
			}
		}
	}
}
