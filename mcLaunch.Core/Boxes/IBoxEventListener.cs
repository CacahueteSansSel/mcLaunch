using mcLaunch.Core.Contents;

namespace mcLaunch.Core.Boxes;

public interface IBoxEventListener
{
    void OnContentAdded(MinecraftContent content);
    void OnContentRemoved(string contentId);
}