using System;
using System.Collections.Generic;
using System.Globalization;

namespace Marechai.Data;

/// <summary>
///     Compares strings using a magnitude-aware natural sort order. Numeric tokens may include a decimal point
///     and may be followed by an ISO/IEC unit suffix (e.g. <c>KiB</c>, <c>MiB</c>, <c>GiB</c>, <c>kHz</c>,
///     <c>MHz</c>) which is converted to a normalized magnitude before comparison. Falls back to natural
///     character comparison (case-insensitive) for non-numeric segments.
/// </summary>
public sealed class NaturalUnitComparer : IComparer<string>
{
    public static readonly NaturalUnitComparer Instance = new();

    // Suffix → multiplier. Order matters for matching: longest first.
    static readonly (string Suffix, double Factor)[] Units =
    [
        // Binary (IEC) byte units
        ("EiB", 1024d * 1024 * 1024 * 1024 * 1024 * 1024), ("PiB", 1024d * 1024 * 1024 * 1024 * 1024),
        ("TiB", 1024d * 1024 * 1024 * 1024), ("GiB", 1024d * 1024 * 1024), ("MiB", 1024d * 1024), ("KiB", 1024d),
        // Decimal (SI) byte units
        ("EB", 1e18), ("PB", 1e15), ("TB", 1e12), ("GB", 1e9), ("MB", 1e6), ("KB", 1e3), ("kB", 1e3),
        // Frequency
        ("THz", 1e12), ("GHz", 1e9), ("MHz", 1e6), ("kHz", 1e3), ("KHz", 1e3), ("Hz", 1d),
        // Bits per second
        ("Gbps", 1e9), ("Mbps", 1e6), ("Kbps", 1e3), ("kbps", 1e3), ("bps", 1d),
        // Plain byte
        ("B", 1d)
    ];

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
                double xVal = ReadMagnitude(x, ref ix);
                double yVal = ReadMagnitude(y, ref iy);

                int cmp = xVal.CompareTo(yVal);

                if(cmp != 0) return cmp;
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

    static double ReadMagnitude(string s, ref int i)
    {
        int start = i;

        while(i < s.Length && char.IsDigit(s[i])) i++;

        // Optional decimal part: only consume if followed by digits (avoid consuming version separator dots).
        if(i + 1 < s.Length && s[i] == '.' && char.IsDigit(s[i + 1]))
        {
            i++;

            while(i < s.Length && char.IsDigit(s[i])) i++;
        }

        if(!double.TryParse(s.AsSpan(start, i - start), NumberStyles.Float, CultureInfo.InvariantCulture,
                            out double value))
            value = 0;

        // Skip optional whitespace (including non-breaking space U+00A0 and narrow no-break space U+202F)
        // before a unit suffix.
        int afterNumber = i;

        while(i < s.Length && (s[i] == ' ' || s[i] == '\u00A0' || s[i] == '\u202F' || s[i] == '\t')) i++;

        // Match longest unit suffix.
        foreach((string suffix, double factor) in Units)
        {
            if(i + suffix.Length > s.Length) continue;

            // Suffix must be followed by end-of-string or non-letter (to avoid matching "Mb" inside "Mbits").
            if(!string.CompareOrdinal(s, i, suffix, 0, suffix.Length).Equals(0)) continue;

            int endIndex = i + suffix.Length;

            if(endIndex < s.Length && char.IsLetter(s[endIndex])) continue;

            i =  endIndex;
            return value * factor;
        }

        // No unit matched: rewind to position right after the number (don't swallow whitespace).
        i = afterNumber;
        return value;
    }
}
