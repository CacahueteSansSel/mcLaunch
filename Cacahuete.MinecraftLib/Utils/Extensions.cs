namespace Cacahuete.MinecraftLib.Utils;

public static class Extensions
{
    public static List<T> Adds<T>(this List<T> list, IEnumerable<T> array)
    {
        list.AddRange(array);

        return list;
    }
}