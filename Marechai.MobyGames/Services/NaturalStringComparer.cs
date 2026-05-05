using System;
using System.Collections.Generic;

namespace Marechai.MobyGames.Services;

sealed class NaturalStringComparer : IComparer<string>
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
            bool xDigit = char.IsDigit(x[ix]);
            bool yDigit = char.IsDigit(y[iy]);

            if(xDigit && yDigit)
            {
                // Compare numeric spans
                int xStart = ix, yStart = iy;

                while(ix < x.Length && char.IsDigit(x[ix])) ix++;
                while(iy < y.Length && char.IsDigit(y[iy])) iy++;

                int xLen = ix - xStart;
                int yLen = iy - yStart;

                // Skip leading zeros for value comparison
                int xZ = xStart;
                while(xZ < ix - 1 && x[xZ] == '0') xZ++;

                int yZ = yStart;
                while(yZ < iy - 1 && y[yZ] == '0') yZ++;

                int xEffLen = ix - xZ;
                int yEffLen = iy - yZ;

                if(xEffLen != yEffLen)
                    return xEffLen.CompareTo(yEffLen);

                for(int i = 0; i < xEffLen; i++)
                {
                    int cmp = x[xZ + i].CompareTo(y[yZ + i]);

                    if(cmp != 0) return cmp;
                }

                // Same numeric value — shorter original (fewer leading zeros) first
                if(xLen != yLen)
                    return xLen.CompareTo(yLen);
            }
            else
            {
                int cmp = char.ToLowerInvariant(x[ix]).CompareTo(char.ToLowerInvariant(y[iy]));

                if(cmp != 0) return cmp;

                ix++;
                iy++;
            }
        }

        return x.Length.CompareTo(y.Length);
    }
}
