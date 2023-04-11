namespace Raki.TegramBot.Core.Services;
public static class Helper
{
    public static string[] GetEnglishAlphabet()
    {
        var alphabet = new string[26];

        for (var i = 0; i < 26; i++)
        {
            var letter = (char)('A' + i);
            alphabet[i] = letter.ToString();
        }

        return alphabet;
    }
}
