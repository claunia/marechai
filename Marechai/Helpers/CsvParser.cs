/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Marechai.Helpers;

public static class CsvParser
{
    public static string GenerateTemplate(string[] headers) => GenerateLine(headers);

    public static List<Dictionary<string, string>> Parse(string csvContent)
    {
        var results = new List<Dictionary<string, string>>();

        if(string.IsNullOrWhiteSpace(csvContent))
            return results;

        List<string[]> lines = ParseLines(csvContent);

        if(lines.Count < 2)
            return results;

        string[] headers = lines[0];

        for(int i = 1; i < lines.Count; i++)
        {
            string[] fields = lines[i];
            var      row    = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for(int j = 0; j < headers.Length; j++)
            {
                string value = j < fields.Length ? fields[j] : "";
                row[headers[j].Trim()] = value;
            }

            results.Add(row);
        }

        return results;
    }

    public static string Generate(string[] headers, List<Dictionary<string, string>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(GenerateLine(headers));

        foreach(Dictionary<string, string> row in rows)
        {
            var values = new string[headers.Length];

            for(int i = 0; i < headers.Length; i++)
                values[i] = row.TryGetValue(headers[i], out string? val) ? val ?? "" : "";

            sb.AppendLine(GenerateLine(values));
        }

        return sb.ToString();
    }

    static string GenerateLine(string[] fields)
    {
        var sb = new StringBuilder();

        for(int i = 0; i < fields.Length; i++)
        {
            if(i > 0)
                sb.Append(',');

            string field = fields[i] ?? "";

            if(field.Contains(',')  ||
               field.Contains('"')  ||
               field.Contains('\n') ||
               field.Contains('\r'))
            {
                sb.Append('"');
                sb.Append(field.Replace("\"", "\"\""));
                sb.Append('"');
            }
            else
            {
                sb.Append(field);
            }
        }

        return sb.ToString();
    }

    static List<string[]> ParseLines(string csvContent)
    {
        var    result  = new List<string[]>();
        var    fields  = new List<string>();
        var    current = new StringBuilder();
        bool   inQuote = false;

        using var reader = new StringReader(csvContent);
        int       ch;

        while((ch = reader.Read()) != -1)
        {
            var c = (char)ch;

            if(inQuote)
            {
                if(c == '"')
                {
                    int next = reader.Peek();

                    if(next == '"')
                    {
                        reader.Read();
                        current.Append('"');
                    }
                    else
                    {
                        inQuote = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                switch(c)
                {
                    case '"':
                        inQuote = true;

                        break;
                    case ',':
                        fields.Add(current.ToString());
                        current.Clear();

                        break;
                    case '\r':
                    {
                        int next = reader.Peek();

                        if(next == '\n')
                            reader.Read();

                        fields.Add(current.ToString());
                        current.Clear();

                        if(fields.Count > 0 && !(fields.Count == 1 && string.IsNullOrEmpty(fields[0])))
                            result.Add(fields.ToArray());

                        fields.Clear();

                        break;
                    }
                    case '\n':
                        fields.Add(current.ToString());
                        current.Clear();

                        if(fields.Count > 0 && !(fields.Count == 1 && string.IsNullOrEmpty(fields[0])))
                            result.Add(fields.ToArray());

                        fields.Clear();

                        break;
                    default:
                        current.Append(c);

                        break;
                }
            }
        }

        if(current.Length > 0 || fields.Count > 0)
        {
            fields.Add(current.ToString());

            if(!(fields.Count == 1 && string.IsNullOrEmpty(fields[0])))
                result.Add(fields.ToArray());
        }

        return result;
    }
}
