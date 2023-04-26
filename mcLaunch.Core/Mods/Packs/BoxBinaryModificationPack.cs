using mcLaunch.Core.Boxes;
using mcLaunch.Core.Boxes.Format;
using mcLaunch.Core.Managers;

namespace mcLaunch.Core.Mods.Packs;

public class BoxBinaryModificationPack : ModificationPack
{
    private SerializedBox boxBinary;
    public override string Name => boxBinary.Name;
    public override string Author => boxBinary.Author;
    public override string Version => string.Empty;
    public override string? Id => boxBinary.Id;
    public override string? Description => boxBinary.Description;
    public override string MinecraftVersion => boxBinary.Version;
    public override string ModloaderId => boxBinary.ModLoaderId;
    public override string ModloaderVersion => boxBinary.ModLoaderVersion;
    public override SerializedModification[] Modifications { get; }
    public override AdditionalFile[] AdditionalFiles { get; }
    public byte[] IconData => boxBinary.IconData;
    public byte[] BackgroundData => boxBinary.BackgroundData;

    public BoxBinaryModificationPack()
    {
        
    }
    
    public BoxBinaryModificationPack(string filename) : base(filename)
    {
        boxBinary = new SerializedBox(filename);

        List<SerializedModification> mods = new();
        foreach (Mod mod in boxBinary.Mods)
        {
            mods.Add(new SerializedModification
            {
                IsRequired = true,
                ModId = mod.Id,
                PlatformId = mod.Platform,
                VersionId = mod.Version
            });
        }
        Modifications = mods.ToArray();

        List<AdditionalFile> files = new();
        foreach (FSFile file in boxBinary.Files)
        {
            files.Add(new AdditionalFile
            {
                Path = file.AbsFilename,
                Data = file.Data
            });
        }
        AdditionalFiles = files.ToArray();
    }
    
    public override async Task InstallModificationAsync(Box targetBox, SerializedModification mod)
    {
        await ModPlatformManager.Platform.InstallModificationAsync(targetBox, new Modification
        {
            Id = mod.ModId,
            ModPlatformId = mod.PlatformId
        }, mod.VersionId, false);
    }

    public override async Task ExportAsync(Box box, string filename)
    {
        SerializedBox bb = new SerializedBox
        {
            Id = box.Manifest.Id,
            Name = box.Manifest.Name,
            Author = box.Manifest.Author,
            Version = box.Manifest.Version,
            Description = box.Manifest.Description,
            ModLoaderId = box.Manifest.ModLoaderId,
            ModLoaderVersion = box.Manifest.ModLoaderVersion
        };

        MemoryStream iconStream = new();
        MemoryStream backgroundStream = new();
        box.Manifest.Icon?.Save(iconStream);
        box.Manifest.Background?.Save(backgroundStream);

        bb.IconData = iconStream.ToArray();
        bb.BackgroundData = backgroundStream.ToArray();

        List<Mod> mods = new();
        foreach (BoxStoredModification mod in box.Manifest.Modifications)
        {
            mods.Add(new Mod
            {
                Id = mod.Id,
                Filenames = mod.Filenames,
                Platform = mod.PlatformId,
                Version = mod.VersionId
            });
        }
        bb.Mods = mods.ToArray();

        List<FSFile> files = new();
        foreach (string file in box.GetAdditionalFiles())
        {
            string completePath = $"{box.Path}/minecraft/{file}";
            byte[] data = await File.ReadAllBytesAsync(completePath);
            
            files.Add(new FSFile
            {
                AbsFilename = file,
                Data = data
            });
        }
        bb.Files = files.ToArray();
        
        bb.Save(filename);
    }
}