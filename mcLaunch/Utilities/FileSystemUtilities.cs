using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace mcLaunch.Utilities;

public class FileSystemUtilities
{
    public static async Task<string[]> PickFiles(bool multiple, string title, FilePickerFileType[] types)
    {
        IStorageProvider storage = MainWindow.Instance.StorageProvider;

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = multiple,
            FileTypeFilter = types
        });

        if (files == null || files.Count == 0) return Array.Empty<string>();

        return files.Select(file => file.TryGetLocalPath() ?? string.Empty)
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();
    }

    public static async Task<string[]> PickFiles(bool multiple, string title, string[] extensions)
    {
        return await PickFiles(multiple, title, extensions.Select(e => new FilePickerFileType(e)
        {
            Patterns = [$"*.{e}"]
        }).ToArray());
    }

    public static async Task<Bitmap[]> PickBitmaps(bool multiple, string title)
    {
        string[] files = await PickFiles(multiple, title, new[]
        {
            new FilePickerFileType("Image")
            {
                Patterns = new[] {"*.png", "*.jpg", "*.jpeg"},
                MimeTypes = new[] {"image/png", "image/jpeg"}
            }
        });

        return files.Select(file => new Bitmap(file)).ToArray();
    }
}