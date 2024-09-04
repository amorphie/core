namespace amorphie.core.Extension;
public static class StringExtension
{

    public static string DeleteUnAllowedCharecters(this string transitionName)
    {
        return System.Text.RegularExpressions.Regex.Replace(transitionName, "[^A-Za-z0-9]", "", System.Text.RegularExpressions.RegexOptions.Compiled);
    }
    public static string FirstCharToUpper(this string input)
    {
        return input switch
        {
            null => "",
            "" => "",
            _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
        };
    }

}
