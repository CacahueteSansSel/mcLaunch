using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using DynamicData;
using mcLaunch.Utilities;
using SharpNBT;

namespace mcLaunch.Views.Windows;

public partial class NbtEditorWindow : Window
{
    ObservableCollection<TagNode> nodes = [];
    string path;
    
    public CompoundTag? Root { get; private set; }
    
    public NbtEditorWindow()
    {
        InitializeComponent();
        
        if (Design.IsDesignMode) Load("level.dat");
    }

    public NbtEditorWindow(string filename) : this()
    {
        Load(filename);
    }

    public void Load(string filename)
    {
        path = filename;
        SetRoot(NbtFile.Read(filename, FormatOptions.Java), Path.GetFileName(filename));
    }

    public async void Save()
    {
        if (Root is null) return;

        await NbtFile.WriteAsync(path, Root, FormatOptions.Java);
    }

    public void SetRoot(CompoundTag root, string name = "Root")
    {
        nodes.Clear();
        Root = root;

        TagNode rootNode = GetNodeForTag(root);
        rootNode.Name = name;
        nodes.Add(rootNode);
        DataContext = nodes;
    }

    TagNode GetNodeForTag(Tag tag)
    {
        if (tag is CompoundTag compoundTag)
        {
            List<TagNode> childrenNodes = [];

            foreach (Tag childrenTag in compoundTag)
                childrenNodes.Add(GetNodeForTag(childrenTag));
            
            return new TagNode(tag, childrenNodes.ToArray());
        }
        
        if (tag is ListTag listTag)
        {
            List<TagNode> childrenNodes = [];

            foreach (Tag childrenTag in listTag)
                childrenNodes.Add(GetNodeForTag(childrenTag));
            
            return new TagNode(tag, childrenNodes.ToArray());
        }
        
        return new TagNode(tag);
    }

    public class TagNode
    {
        public Tag Tag { get; set; }
        public string? Name { get; set; }
        public TagType Type { get; set; }
        public ObservableCollection<TagNode> Children { get; set; }

        public string? Value
        {
            get
            {
                switch (Tag.Type)
                {
                    case TagType.Byte:
                        return ((ByteTag) Tag).Value.ToString();
                    case TagType.Short:
                        return ((ShortTag) Tag).Value.ToString();
                    case TagType.Int:
                        return ((IntTag) Tag).Value.ToString();
                    case TagType.Long:
                        return ((LongTag) Tag).Value.ToString();
                    case TagType.Float:
                        return ((FloatTag) Tag).Value.ToString(CultureInfo.InvariantCulture);
                    case TagType.Double:
                        return ((DoubleTag) Tag).Value.ToString(CultureInfo.InvariantCulture);
                    case TagType.String:
                        return ((StringTag) Tag).Value;
                }
                
                return null;
            }
            set
            {
                switch (Tag.Type)
                {
                    case TagType.Byte:
                        ((ByteTag) Tag).Value = string.IsNullOrWhiteSpace(value) ? (byte)0 : byte.Parse(value);
                        break;
                    case TagType.Short:
                        ((ShortTag) Tag).Value = string.IsNullOrWhiteSpace(value) ? (short)0 : short.Parse(value);
                        break;
                    case TagType.Int:
                        ((IntTag) Tag).Value = string.IsNullOrWhiteSpace(value) ? 0 : int.Parse(value);
                        break;
                    case TagType.Long:
                        ((LongTag) Tag).Value = string.IsNullOrWhiteSpace(value) ? 0 : long.Parse(value);
                        break;
                    case TagType.Float:
                        ((FloatTag) Tag).Value = string.IsNullOrWhiteSpace(value) ? 0 : float.Parse(value);
                        break;
                    case TagType.Double:
                        ((DoubleTag) Tag).Value = string.IsNullOrWhiteSpace(value) ? 0 : double.Parse(value);
                        break;
                    case TagType.String:
                        ((StringTag) Tag).Value = value;
                        break;
                }
            }
        }

        public bool HasValue => Value != null;
        
        // For XAML
        public bool IsNameEmpty => string.IsNullOrWhiteSpace(Name);
        public string TypeLine => $"{Type} tag";
        public bool IsByte => Type == TagType.Byte;
        public bool IsShort => Type == TagType.Short;
        public bool IsInt => Type == TagType.Int;
        public bool IsLong => Type == TagType.Long;
        public bool IsFloat => Type == TagType.Float;
        public bool IsDouble => Type == TagType.Double;
        public bool IsByteArray => Type == TagType.ByteArray;
        public bool IsString => Type == TagType.String;
        public bool IsList => Type == TagType.List;
        public bool IsCompound => Type == TagType.Compound;
        public bool IsIntArray => Type == TagType.IntArray;
        public bool IsLongArray => Type == TagType.LongArray;

        public TagNode(Tag tag, params TagNode[] children)
        {
            Tag = tag;
            Name = tag.Name;
            Type = tag.Type;
            Children = new ObservableCollection<TagNode>(children);
        }
    }

    void SaveButtonClicked(object? sender, RoutedEventArgs e)
    {
        Save();
    }

    async void OpenButtonClicked(object? sender, RoutedEventArgs e)
    {
        string[] files = await FilePickerUtilities.PickFiles(false, "Open NBT File", ["dat", "nbt"]);
        
        if (files.Length != 1) return;
        
        Load(files[0]);
    }

    async void SaveAsButtonClicked(object? sender, RoutedEventArgs e)
    {
        IStorageFile? file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            DefaultExtension = "dat",
            FileTypeChoices = [new FilePickerFileType("nbt") {Patterns = ["*.nbt"]}, new FilePickerFileType("dat") {Patterns = ["*.dat"]}],
            Title = "Save NBT File"
        });
        
        if (file == null) return;
        
        path = file.TryGetLocalPath()!;
        Save();
    }
}