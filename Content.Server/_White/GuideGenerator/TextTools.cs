namespace Content.Server.GuideGenerator.TextTools;

public sealed class TextTools
{
    /// <summary>
    /// Capitalizes first letter of given string.
    /// </summary>
    /// <param name="str">String to capitalize</param>
    /// <returns>String with capitalized first letter</returns>
    public static string CapitalizeString(string str)
    {
        if (str.Length > 1)
        {
            return OopsConcat(char.ToUpper(str[0]).ToString(), str.Remove(0, 1));
        }
        else if (str.Length == 1)
        {
            return char.ToUpper(str[0]).ToString();
        }
        else
        {
            return str;
        }
    }

    private static string OopsConcat(string a, string b)
    {
        // This exists to prevent Roslyn being clever and compiling something that fails sandbox checks.
        return a + b;
    }
}
