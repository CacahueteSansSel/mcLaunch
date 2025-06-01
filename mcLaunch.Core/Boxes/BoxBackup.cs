using System.Text.Json.Serialization;

namespace mcLaunch.Core.Boxes;

public class BoxBackup
{
    public BoxBackup()
    {
    }

    public BoxBackup(string name, BoxBackupType type, DateTime creationTime, string filename)
    {
        Name = name;
        Type = type;
        CreationTime = creationTime;
        Filename = filename;
    }

    public string Name { get; set; }
    public BoxBackupType Type { get; set; }
    public DateTime CreationTime { get; set; }
    public string Filename { get; set; }

    [JsonIgnore] public bool IsCompleteBackup => Type == BoxBackupType.Complete;
    [JsonIgnore] public bool IsPartialBackup => Type == BoxBackupType.Partial;
}

public enum BoxBackupType
{
    Complete,
    Partial
}