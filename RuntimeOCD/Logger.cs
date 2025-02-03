/*
	This file is part of RuntimeOCD.

	RuntimeOCD is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

    RuntimeOCD is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along with RuntimeOCD. If not, see <https://www.gnu.org/licenses/>. 
*/

namespace RuntimeOCD
{
    public class Logger
    {
        public Logger(string name)
        {
            ModName = name;
            MsgPrefix = $"[{ModName}]: ";
        }
        public Logger(string name, string componentName)
        {
            ModName = name;
            Component = componentName;
            MsgPrefix = $"[{ModName}]::[{Component}]: ";
        }
        protected virtual string ModName { get; }
        protected virtual string? Component { get; }
        protected virtual string MsgPrefix { get; }
        public virtual void Info(string message, bool hostOnly = true)
        {
            if (!hostOnly || OcdManager.IsHost) Log.Out(MsgPrefix + message);
        }
        public virtual void Info(IEnumerable<string> multiMessage, bool hostOnly = true)
        {
            if (multiMessage == null || hostOnly && !OcdManager.IsHost) return;

            foreach (var msg in multiMessage)
            {
                Log.Out(MsgPrefix + msg);
            }
        }
        public virtual void Info(IEnumerable<string> multiMessage, string prefix, bool hostOnly = true)
        {
            if (multiMessage == null || hostOnly && !OcdManager.IsHost) return;

            foreach (var msg in multiMessage)
            {
                Log.Out(MsgPrefix + prefix + msg);
            }
        }
        public virtual void Warn(string message, bool hostOnly = true)
        {
            if (!hostOnly || OcdManager.IsHost) Log.Warning(MsgPrefix + message);
        }
        public virtual void Warn(IEnumerable<string> multiMessage, bool hostOnly = true)
        {
            if (multiMessage == null || hostOnly && !OcdManager.IsHost) return;

            foreach (var msg in multiMessage)
            {
                Log.Warning(MsgPrefix + msg);
            }
        }
        public virtual void Warn(IEnumerable<string> multiMessage, string prefix, bool hostOnly = true)
        {
            if (multiMessage == null || hostOnly && !OcdManager.IsHost) return;

            foreach (var msg in multiMessage)
            {
                Log.Warning(MsgPrefix + prefix + msg);
            }
        }
        public virtual void Error(string message, bool hostOnly = true)
        {
            if (!hostOnly || OcdManager.IsHost) Log.Error(MsgPrefix + message);
        }
        public virtual void Error(IEnumerable<string> multiMessage, bool hostOnly = true)
        {
            if (multiMessage == null || hostOnly && !OcdManager.IsHost) return;

            foreach (var msg in multiMessage)
            {
                Log.Error(MsgPrefix + msg);
            }
        }
        public virtual void Error(IEnumerable<string> multiMessage, string prefix, bool hostOnly = true)
        {
            if (multiMessage == null || hostOnly && !OcdManager.IsHost) return;

            foreach (var msg in multiMessage)
            {
                Log.Error(MsgPrefix + prefix + msg);
            }
        }
    }
}
