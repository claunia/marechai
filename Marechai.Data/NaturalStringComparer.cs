using System;
using System.Collections.Generic;

namespace Marechai.Data;

/// <summary>
///     Compares strings using natural sort order, where numeric segments are compared by value rather than
///     lexicographically. For example, "4.00.96" sorts before "4.00.111".
/// </summary>
public sealed class NaturalStringComparer : IComparer<string>
{
    public static readonly NaturalStringComparer Instance = new();

    public int Compare(string x, string y)
    {
        if(ReferenceEquals(x, y)) return 0;
        if(x is null) return -1;
        if(y is null) return 1;

        int ix = 0, iy = 0;

        while(ix < x.Length && iy < y.Length)
        {
            bool xIsDigit = char.IsDigit(x[ix]);
            bool yIsDigit = char.IsDigit(y[iy]);

            if(xIsDigit && yIsDigit)
            {
                // Skip leading zeros and compare numeric segments by value
                int xStart = ix;
                int yStart = iy;

                while(ix < x.Length && x[ix] == '0') ix++;
                while(iy < y.Length && y[iy] == '0') iy++;

                int xNumStart = ix;
                int yNumStart = iy;

                while(ix < x.Length && char.IsDigit(x[ix])) ix++;
                while(iy < y.Length && char.IsDigit(y[iy])) iy++;

                int xLen = ix - xNumStart;
                int yLen = iy - yNumStart;

                // Longer numeric segment is larger
                if(xLen != yLen) return xLen.CompareTo(yLen);

                // Same length: compare digit by digit
                for(int i = 0; i < xLen; i++)
                {
                    int cmp = x[xNumStart + i].CompareTo(y[yNumStart + i]);

                    if(cmp != 0) return cmp;
                }

                // Values equal — more leading zeros means smaller (e.g., "007" < "7" is debatable, keep stable)
                int xLeadingZeros = xNumStart - xStart;
                int yLeadingZeros = yNumStart - yStart;

                if(xLeadingZeros != yLeadingZeros) return xLeadingZeros.CompareTo(yLeadingZeros);
            }
            else
            {
                int cmp = char.ToUpperInvariant(x[ix]).CompareTo(char.ToUpperInvariant(y[iy]));

                if(cmp != 0) return cmp;

                ix++;
                iy++;
            }
        }

        return x.Length.CompareTo(y.Length);
    }
}
