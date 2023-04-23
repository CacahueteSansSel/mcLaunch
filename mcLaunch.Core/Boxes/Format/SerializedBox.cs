using System.Reactive;
using K4os.Compression.LZ4;

namespace mcLaunch.Core.Boxes.Format;

public class SerializedBox
{
    public string Name { get; set; }
    public string Id { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }
    public string ModLoaderId { get; set; }
    public string ModLoaderVersion { get; set; }
    
    public uint CompressedIconSize { get; set; }
    public uint UncompressedIconSize { get; set; }
    public byte[] IconData { get; set; }
    public uint CompressedBackgroundSize { get; set; }
    public uint UncompressedBackgroundSize { get; set; }
    public byte[] BackgroundData { get; set; }
    
    public uint ModCount { get; set; }
    public Mod[] Mods { get; set; }
    
    public uint FileCount { get; set; }
    public uint CompressedFSSize { get; set; }
    public uint UncompressedFSSize { get; set; }
    public FSFile[] Files { get; set; }

    public SerializedBox()
    {
        
    }

    public SerializedBox(string filename)
    {
        BinaryReader rd = new BinaryReader(new FileStream(filename, FileMode.Open));
        Read(rd);
        rd.Close();
    }

    public SerializedBox(BinaryReader rd)
    {
        Read(rd);
    }

    public void Save(string filename)
    {
        FileStream fs = new FileStream(filename, FileMode.Create);
        Write(new BinaryWriter(fs));
        fs.Close();
    }

    void Read(BinaryReader rd)
    {
        if (new string(rd.ReadChars(5)) != "ddBox") 
            throw new BoxBinaryFormatException("Invalid magic");

        // Manifest Section
        Name = rd.ReadString();
        Id = rd.ReadString();
        Description = rd.ReadString();
        Author = rd.ReadString();
        Version = rd.ReadString();
        ModLoaderId = rd.ReadString();
        ModLoaderVersion = rd.ReadString();
        
        // Resources Section
        CompressedIconSize = rd.ReadUInt32();
        UncompressedIconSize = rd.ReadUInt32();
        byte[] compressedData = new byte[CompressedIconSize];
        for (ulong i = 0; i < CompressedIconSize; i++)
            compressedData[i] = rd.ReadByte();
        byte[] uncompressedData = new byte[UncompressedIconSize];
        int decoded = LZ4Codec.Decode(compressedData, uncompressedData);
        IconData = uncompressedData;
        
        CompressedBackgroundSize = rd.ReadUInt32();
        UncompressedBackgroundSize = rd.ReadUInt32();
        byte[] compressedBackgroundData = new byte[CompressedBackgroundSize];
        for (ulong i = 0; i < CompressedBackgroundSize; i++)
            compressedBackgroundData[i] = rd.ReadByte();
        byte[] uncompressedBackgroundData = new byte[UncompressedBackgroundSize];
        decoded = LZ4Codec.Decode(compressedBackgroundData, uncompressedBackgroundData);
        BackgroundData = uncompressedBackgroundData;
        
        // Mod Section
        ModCount = rd.ReadUInt32();
        Mods = new Mod[ModCount];
        for (uint i = 0; i < ModCount; i++)
        {
            Mod mod = new();
            mod.Read(rd);

            Mods[i] = mod;
        }
        
        // Filesystem Section
        FileCount = rd.ReadUInt32();
        Files = new FSFile[FileCount];
        CompressedFSSize = rd.ReadUInt32();
        UncompressedFSSize = rd.ReadUInt32();
        byte[] filesystemSection = new byte[CompressedFSSize];
        for (ulong i = 0; i < CompressedFSSize; i++)
            filesystemSection[i] = rd.ReadByte();
        byte[] uncompressedFS = new byte[UncompressedFSSize];
        decoded = LZ4Codec.Decode(filesystemSection, uncompressedFS);

        BinaryReader fs = new BinaryReader(new MemoryStream(uncompressedFS));

        for (uint i = 0; i < FileCount; i++)
        {
            FSFile file = new();
            file.Read(fs);

            Files[i] = file;
        }
        
        fs.Close();
    }

    void Write(BinaryWriter wr)
    {
        wr.Write("ddBox".ToCharArray());
        
        // Manifest Section
        wr.Write(Name);
        wr.Write(Id);
        wr.Write(Description ?? "no description");
        wr.Write(Author);
        wr.Write(Version);
        wr.Write(ModLoaderId);
        wr.Write(ModLoaderVersion);
        
        // Resources Section
        byte[] compressedIconData = new byte[LZ4Codec.MaximumOutputSize(IconData.Length)];
        int newSize = LZ4Codec.Encode(IconData, compressedIconData, LZ4Level.L12_MAX);
        Array.Resize(ref compressedIconData, newSize);
        
        wr.Write((uint)newSize);
        wr.Write((uint)IconData.LongLength);
        wr.Write(compressedIconData);
        
        byte[] compressedBackgroundData = new byte[LZ4Codec.MaximumOutputSize(BackgroundData.Length)];
        newSize = LZ4Codec.Encode(BackgroundData, compressedBackgroundData, LZ4Level.L12_MAX);
        Array.Resize(ref compressedBackgroundData, newSize);
        
        wr.Write((uint)newSize);
        wr.Write((uint)BackgroundData.LongLength);
        wr.Write(compressedBackgroundData);
        
        // Mod Section
        wr.Write((uint)Mods.LongLength);
        foreach (Mod mod in Mods) mod.Write(wr);
        
        // Filesystem Section
        wr.Write((uint)Files.LongLength);

        MemoryStream fsStream = new();
        BinaryWriter fs = new BinaryWriter(fsStream);

        foreach (FSFile file in Files) file.Write(fs);

        byte[] fsData = fsStream.ToArray();
        fs.Close();

        byte[] compressedFsData = new byte[LZ4Codec.MaximumOutputSize(fsData.Length)];
        newSize = LZ4Codec.Encode(fsData, compressedFsData);
        Array.Resize(ref compressedFsData, newSize);
        
        wr.Write((uint)compressedFsData.LongLength);
        wr.Write((uint)fsData.LongLength);
        wr.Write(compressedFsData);
    }
}

public class FSFile
{
    public string AbsFilename { get; set; }
    public ulong DataSize { get; set; }
    public byte[] Data { get; set; }

    public void Read(BinaryReader rd)
    {
        AbsFilename = rd.ReadString();
        DataSize = rd.ReadUInt64();
        Data = new byte[DataSize];

        for (ulong i = 0; i < DataSize; i++)
            Data[i] = rd.ReadByte();
    }

    public void Write(BinaryWriter wr)
    {
        wr.Write(AbsFilename);
        wr.Write((ulong)Data.LongLength);
        wr.Write(Data);
    }
}

public class Mod
{
    public string Id { get; set; }
    public string Version { get; set; }
    public string Platform { get; set; }
    public uint FileCount { get; set; }
    public string[] Filenames { get; set; }

    public void Read(BinaryReader rd)
    {
        Id = rd.ReadString();
        Version = rd.ReadString();
        Platform = rd.ReadString();
        FileCount = rd.ReadUInt32();
        Filenames = new string[FileCount];

        for (uint i = 0; i < FileCount; i++)
            Filenames[i] = rd.ReadString();
    }

    public void Write(BinaryWriter wr)
    {
        wr.Write(Id);
        wr.Write(Version);
        wr.Write(Platform);
        wr.Write((uint)Filenames.Length);
        
        foreach (string str in Filenames)
            wr.Write(str);
    }
}