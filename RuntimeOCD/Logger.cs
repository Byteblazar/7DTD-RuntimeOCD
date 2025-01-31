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
