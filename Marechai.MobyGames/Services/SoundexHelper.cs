using System;
using System.Collections.Generic;
using System.Linq;

namespace Marechai.MobyGames.Services;

public static class SoundexHelper
{
    static readonly char[] Map =
    [
        '0', '1', '2', '3', '0', '1', '2', '0', '0', '2', '2', '4', '5',
        '5', '0', '1', '2', '6', '2', '3', '0', '1', '0', '2', '0', '2'
    ];

    public static string Generate(string input)
    {
        if(string.IsNullOrWhiteSpace(input))
            return "0000";

        Span<char> result = stackalloc char[4];
        result[0] = char.ToUpperInvariant(input[0]);
        int  resultIndex = 1;
        char lastCode    = GetCode(result[0]);

        for(int i = 1; i < input.Length && resultIndex < 4; i++)
        {
            char c = input[i];

            if(!char.IsLetter(c)) continue;

            char code = GetCode(char.ToUpperInvariant(c));

            if(code == '0' || code == lastCode)
            {
                lastCode = code;
                continue;
            }

            result[resultIndex++] = code;
            lastCode              = code;
        }

        while(resultIndex < 4)
            result[resultIndex++] = '0';

        return new string(result);
    }

    static char GetCode(char c)
    {
        int index = char.ToUpperInvariant(c) - 'A';

        return index is >= 0 and < 26 ? Map[index] : '0';
    }

    public static Dictionary<string, List<T>> BuildSoundexIndex<T>(
        IEnumerable<T> items, Func<T, string> nameSelector)
    {
        var dict = new Dictionary<string, List<T>>();

        foreach(var item in items)
        {
            string name = nameSelector(item);

            if(string.IsNullOrWhiteSpace(name)) continue;

            string code = Generate(name);

            if(!dict.TryGetValue(code, out var list))
            {
                list       = [];
                dict[code] = list;
            }

            list.Add(item);
        }

        return dict;
    }
}
