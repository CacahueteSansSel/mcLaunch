using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using DynamicData;
using mcLaunch.Models;
using mcLaunch.Utilities;
using ReactiveUI;
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

        if (tag is IntArrayTag intArrayTag)
        {
            List<TagNode> childrenNodes = [];

            for (int i = 0; i < intArrayTag.Count; i++)
                childrenNodes.Add(new TagNode(intArrayTag, i));
            
            return new TagNode(tag, childrenNodes.ToArray());
        }

        if (tag is LongArrayTag longArrayTag)
        {
            List<TagNode> childrenNodes = [];

            for (int i = 0; i < longArrayTag.Count; i++)
                childrenNodes.Add(new TagNode(longArrayTag, i));
            
            return new TagNode(tag, childrenNodes.ToArray());
        }

        if (tag is ByteArrayTag byteArrayTag)
        {
            List<TagNode> childrenNodes = [];

            for (int i = 0; i < byteArrayTag.Count; i++)
                childrenNodes.Add(new TagNode(byteArrayTag, i));
            
            return new TagNode(tag, childrenNodes.ToArray());
        }
        
        return new TagNode(tag);
    }

    public class TagNode : ReactiveObject
    {
        Tag? parent;
        Tag? tag;
        string? name;
        TagType type;

        public Tag? Parent
        {
            get => parent;
            set => this.RaiseAndSetIfChanged(ref parent, value);
        }

        public Tag? Tag
        {
            get => tag;
            set => this.RaiseAndSetIfChanged(ref tag, value);
        }

        public string? Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        public TagType Type
        {
            get => type;
            set => this.RaiseAndSetIfChanged(ref type, value);
        }
        
        public ObservableCollection<TagNode> Children { get; set; }
        public bool IsArrayChildren { get; set; }
        public int ArrayChildrenIndex { get; set; }

        public string? Value
        {
            get
            {
                if (Parent is IntArrayTag intArrayTag && IsArrayChildren)
                    return intArrayTag[ArrayChildrenIndex].ToString();
                if (Parent is LongArrayTag longArrayTag && IsArrayChildren)
                    return longArrayTag[ArrayChildrenIndex].ToString();
                if (Parent is ByteArrayTag byteArrayTag && IsArrayChildren)
                    return byteArrayTag[ArrayChildrenIndex].ToString();
                
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
                if (Parent is IntArrayTag intArrayTag && IsArrayChildren)
                    intArrayTag[ArrayChildrenIndex] = int.TryParse(value, out int parsedValue) ? parsedValue : 0;
                if (Parent is LongArrayTag longArrayTag && IsArrayChildren)
                    longArrayTag[ArrayChildrenIndex] = long.TryParse(value, out long parsedValue) ? parsedValue : 0;
                if (Parent is ByteArrayTag byteArrayTag && IsArrayChildren)
                    byteArrayTag[ArrayChildrenIndex] = byte.TryParse(value, out byte parsedValue) ? parsedValue : (byte)0;
                
                if (IsArrayChildren) return;
                
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

        public TagNode(IntArrayTag parent, int index)
        {
            Type = TagType.Int;
            Parent = parent;
            IsArrayChildren = true;
            ArrayChildrenIndex = index;
        }

        public TagNode(LongArrayTag parent, int index)
        {
            Type = TagType.Long;
            Parent = parent;
            IsArrayChildren = true;
            ArrayChildrenIndex = index;
        }

        public TagNode(ByteArrayTag parent, int index)
        {
            Type = TagType.Byte;
            Parent = parent;
            IsArrayChildren = true;
            ArrayChildrenIndex = index;
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

    async void NewTagButtonClicked(object? sender, RoutedEventArgs e)
    {
        TagNode? selectedTag = (TagNode?)TagTree.SelectedItem;
        if (selectedTag == null) return;
        Tag parentTag = selectedTag.Tag.Type != TagType.Compound 
            ? selectedTag.Parent 
            : selectedTag.Tag;

        if (parentTag is ListTag listTag)
        {
            await CreateTagAsync(listTag.ChildType);
            return;
        }
        if (parentTag is IntArrayTag)
        {
            await CreateTagAsync(TagType.Int);
            return;
        }
        if (parentTag is LongArrayTag)
        {
            await CreateTagAsync(TagType.Long);
            return;
        }
        if (parentTag is ByteArrayTag)
        {
            await CreateTagAsync(TagType.Byte);
            return;
        }
        
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
        RenameButton.IsEnabled = TagTree.SelectedItem != null && ((TagNode)TagTree.SelectedItem).Parent != null && ((TagNode)TagTree.SelectedItem).Parent is CompoundTag;
        SnbtButton.IsEnabled = TagTree.SelectedItem != null;
    }

    void TagTreeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateButtons();
    }

    void SnbtButtonClicked(object? sender, RoutedEventArgs e)
    {
        TagNode? tag = (TagNode?)TagTree.SelectedItem;
        if (tag == null) return;

        new NbtViewTagSnbtWindow(tag.Tag).ShowDialog(this);
    }

    async Task CreateTagAsync(TagType type)
    {
        TagNode? selectedTag = (TagNode?)TagTree.SelectedItem;
        if (selectedTag == null) return;

        Tag parentTag = selectedTag.Tag.Type != TagType.Compound 
            ? selectedTag.Parent 
            : selectedTag.Tag;

        string name = null;
        if (parentTag is CompoundTag)
        {
            name = await new NbtEditTagNameWindow(type, "").ShowDialog<string>(this);
            if (string.IsNullOrWhiteSpace(name)) return;
        }

        Tag? toAddTag = null;

        switch (type)
        {
            case TagType.Byte:
                toAddTag = new ByteTag(name, 0);
                break;
            case TagType.Short:
                toAddTag = new ShortTag(name, 0);
                break;
            case TagType.Int:
                toAddTag = new IntTag(name, 0);
                break;
            case TagType.Long:
                toAddTag = new LongTag(name, 0);
                break;
            case TagType.Float:
                toAddTag = new FloatTag(name, 0);
                break;
            case TagType.Double:
                toAddTag = new DoubleTag(name, 0);
                break;
            case TagType.ByteArray:
                toAddTag = new ByteArrayTag(name, []);
                break;
            case TagType.String:
                toAddTag = new StringTag(name, string.Empty);
                break;
            case TagType.List:
                // todo
                break;
            case TagType.Compound:
                toAddTag = new CompoundTag(name);
                break;
            case TagType.IntArray:
                toAddTag = new IntArrayTag(name, []);
                break;
            case TagType.LongArray:
                toAddTag = new LongArrayTag(name, []);
                break;
        }

        if (toAddTag != null)
        {
            if (parentTag is CompoundTag compoundTag)
                compoundTag.Add(name, toAddTag);
            if (parentTag is ListTag listTag)
                listTag.Add(toAddTag);
        }
        
        SetRoot(Root);
    }

    async void TagMenuItemClicked(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not MenuItem menu) return;
        if (!Enum.TryParse(menu.Name!.Replace("ItemTag", ""), out TagType type)) return;

        await CreateTagAsync(type);
    }

    void DeleteButtonClicked(object? sender, RoutedEventArgs e)
    {
        TagNode? selectedTag = (TagNode?)TagTree.SelectedItem;
        if (selectedTag == null || selectedTag.Parent is not CompoundTag parent) return;

        parent.Remove(selectedTag.Tag);
        SetRoot(Root);
    }
}