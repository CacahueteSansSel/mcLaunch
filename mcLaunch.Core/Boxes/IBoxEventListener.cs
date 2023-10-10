using mcLaunch.Core.Mods;

namespace mcLaunch.Core.Boxes;

public interface IBoxEventListener
{
    void OnModAdded(Modification mod);
    void OnModRemoved(string modId);
}