using mcLaunch.Core.Contents;

namespace mcLaunch.Core.Boxes;

public interface IBoxEventListener
{
    void OnModAdded(MinecraftContent mod);
    void OnModRemoved(string modId);
}