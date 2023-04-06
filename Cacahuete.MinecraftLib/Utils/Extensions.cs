namespace Cacahuete.MinecraftLib.Utils;

public static class Extensions
{
    public static List<T> Adds<T>(this List<T> list, IEnumerable<T>? array)
    {
        if (array == null) return list;
        
        list.AddRange(array);
        return list;
    }
    public static List<T> AddsOnce<T>(this List<T> list, IEnumerable<T>? array)
    {
        if (array == null) return list;
        
        list.AddRange(array.Where(e => !list.Contains(e)));
        return list;
    }
}