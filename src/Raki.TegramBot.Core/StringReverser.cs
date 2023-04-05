namespace Raki.TegramBot.Core;
using System;

public static class StringReverser
{
    public static string ReverseString(string input)
    {
        var charArray = input.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
}
