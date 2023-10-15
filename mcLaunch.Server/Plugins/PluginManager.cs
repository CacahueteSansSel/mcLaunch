using System.Reflection;
using mcLaunch.Server.Core;
using mcLaunch.Server.Logging;
using Spectre.Console;

namespace mcLaunch.Server.Plugins;

public static class PluginManager
{
    public static List<ServerPlugin> Plugins { get; } = new();

    public static void Load()
    {
        if (!Directory.Exists("Plugins"))
        {
            Directory.CreateDirectory("Plugins");
            ManagerLogger.Log("Loaded 0 plugin(s)");
            
            return;
        }
        
        // Load the plugins

        foreach (string dll in Directory.GetFiles("Plugins", "*.dll"))
        {
            try
            {
                Assembly asm = Assembly.LoadFrom(dll);
                Type[] plugins = asm.GetTypes().Where(t => t.IsSubclassOf(typeof(ServerPlugin))).ToArray();
                ServerPlugin[] instPlugins = plugins
                    .Select(t => (ServerPlugin) Activator.CreateInstance(t)).ToArray();
                
                Plugins.AddRange(instPlugins);
                
                ManagerLogger.Log($"Loaded plugin [blue bold]{instPlugins[0].Name}[/] ({Path.GetFileName(dll)})");
            }
            catch (Exception e)
            {
                ManagerLogger.Log($"Failed to load plugin [yellow bold]{Path.GetFileName(dll)}[/] : {e.Message}", LogSeverity.Error);
            }
        }
        
        ManagerLogger.Log($"Loaded {Plugins.Count} plugin(s)");
    }

    public static void RegisterHandlersToServer(MinecraftServer server)
    {
        
    }
}