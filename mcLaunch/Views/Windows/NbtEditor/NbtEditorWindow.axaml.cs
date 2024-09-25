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
using mcLaunch.Models;
using mcLaunch.Utilities;
using SharpNBT;

namespace mcLaunch.Views.Windows.NbtEditor;

public partial class NbtEditorWindow : Window
{
    ObservableCollection<TagNode> nodes = [];
    string path;
    
    public CompoundTag? Root { get; private set; }
    
    public NbtEditorWindow()
    {
        InitializeComponent();
        
        if (Design.IsDesignMode) Load("level.dat");

        DataContext = new NbtEditorWindowDataContext(null);
        UpdateButtons();
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
                childrenNodes.Add(GetNodeForTag(childrenTag).WithParent(tag));
            
            return new TagNode(tag, childrenNodes.ToArray());
        }
        
        if (tag is ListTag listTag)
        {
            List<TagNode> childrenNodes = [];

            foreach (Tag childrenTag in listTag)
                childrenNodes.Add(GetNodeForTag(childrenTag).WithParent(tag));
            
            return new TagNode(tag, childrenNodes.ToArray());
        }
        
        return new TagNode(tag);
    }

    public class TagNode
    {
        public Tag Parent { get; set; }
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

        public TagNode WithParent(Tag parent)
        {
            Parent = parent;
            return this;
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

    void NewTagButtonClicked(object? sender, RoutedEventArgs e)
    {
        NewBoxButton.ContextMenu!.Open(NewBoxButton);
    }

    async void RenameButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (TagTree.SelectedItem == null) return;

        TagNode node = (TagNode) TagTree.SelectedItem;
        if (node.Parent is not CompoundTag compoundTag) return;
        
        string newName = await new NbtEditTagNameWindow(node.Type, node.Name ?? "").ShowDialog<string>(this);
        if (string.IsNullOrWhiteSpace(newName)) return;
        
        compoundTag.Remove(node.Tag);

        Tag? newTag = null;
        if (node.Tag is ByteTag byteTag) newTag = new ByteTag(newName, byteTag.Value);
        if (node.Tag is ByteArrayTag byteArrayTag) newTag = new ByteArrayTag(newName, byteArrayTag);
        if (node.Tag is CompoundTag compTag) newTag = new CompoundTag(newName, compTag.Values);
        if (node.Tag is DoubleTag doubleTag) newTag = new DoubleTag(newName, doubleTag.Value);
        if (node.Tag is FloatTag floatTag) newTag = new FloatTag(newName, floatTag.Value);
        if (node.Tag is IntTag intTag) newTag = new IntTag(newName, intTag.Value);
        if (node.Tag is IntArrayTag intArrayTag) newTag = new IntArrayTag(newName, intArrayTag);
        if (node.Tag is ListTag listTag)
        {
            newTag = new ListTag(newName, listTag.Type);
            ((ListTag)newTag).AddRange(listTag);
        }
        if (node.Tag is LongTag longTag) newTag = new LongTag(newName, longTag.Value);
        if (node.Tag is LongArrayTag longArrayTag) newTag = new LongArrayTag(newName, longArrayTag);
        if (node.Tag is ShortTag shortTag) newTag = new ShortTag(newName, shortTag.Value);
        if (node.Tag is StringTag stringTag) newTag = new StringTag(newName, stringTag.Value);

        compoundTag.Add(newName, newTag ?? node.Tag);

        // Refresh the tree
        SetRoot(Root!);
    }

    void UpdateButtons()
    {
        RenameButton.IsEnabled = TagTree.SelectedItem != null && ((TagNode)TagTree.SelectedItem).Parent != null;
    }

    void TagTreeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateButtons();
    }
}