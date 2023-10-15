using System.Reflection;
using mcLaunch.Server.Core;

namespace mcLaunch.Server.Plugins;

public abstract class ServerPlugin
{
    public abstract string Name { get; }
    
    public abstract void OnLoad();
    public abstract void OnUnload();

    public abstract ServerEventHandler[] ProvideEventHandlers();

    protected ServerEventHandler[] LoadEventHandlersFromAssembly(Assembly assembly)
        => assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ServerEventHandler)))
            .Select(t => (ServerEventHandler)Activator.CreateInstance(t))
            .ToArray();
}