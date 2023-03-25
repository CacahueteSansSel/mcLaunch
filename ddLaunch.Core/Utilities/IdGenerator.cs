namespace ddLaunch.Core.Utilities;

public static class IdGenerator
{
    const string IdCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string Generate(int length = 8)
    {
        string final = "";

        for (int i = 0; i < length; i++)
            final += IdCharacters[Random.Shared.Next(0, IdCharacters.Length)];

        return final;
    }
}