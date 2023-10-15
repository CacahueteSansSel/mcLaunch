using System.Diagnostics;

namespace mcLaunch.Server.Core;

public abstract class ServerEventHandler
{
    public virtual async Task<HandlerResultType> OnChatMessage(ChatMessage message) 
        => HandlerResultType.Continue;
    public virtual async Task<HandlerResultType> OnServerStarted(string ip) 
        => HandlerResultType.Continue;
    public virtual async Task<HandlerResultType> OnServerExited(Process process) 
        => HandlerResultType.Continue;
    public virtual async Task<HandlerResultType> OnPlayerConnect(string username) 
        => HandlerResultType.Continue;
    public virtual async Task<HandlerResultType> OnPlayerUuid(string username, string uuid) 
        => HandlerResultType.Continue;
    public virtual async Task<HandlerResultType> OnPlayerDisconnect(string username, string uuid) 
        => HandlerResultType.Continue;
}

public enum HandlerResultType
{
    Continue,
    Break
}