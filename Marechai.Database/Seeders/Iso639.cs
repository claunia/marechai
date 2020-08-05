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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Marechai.Database.Models;

namespace Marechai.Database.Seeders
{
    public static class Iso639
    {
        // Data files can be found in https://iso639-3.sil.org/code_tables/download_tables
        public static void Seed(MarechaiContext context)
        {
            if(!Directory.Exists("iso639"))
                return;

            IEnumerable<string> files    = Directory.EnumerateFiles("iso639", "iso-639-3_*.tab");
            long                modified = 0;

            List<Models.Iso639> codes = new List<Models.Iso639>();

            foreach(string file in files)
            {
                Console.WriteLine("Importing ISO-639 codes from {0}", file);

                using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None);

                using var sr = new StreamReader(fs, Encoding.UTF8);

                string line = sr.ReadLine();

                if(line is null)
                {
                    Console.WriteLine("Invalid data, not proceeding");

                    continue;
                }

                string[] pieces = line.Split('\t');

                if(pieces.Length != 8               ||
                   pieces[0]     != "Id"            ||
                   pieces[1]     != "Part2B"        ||
                   pieces[2]     != "Part2T"        ||
                   pieces[3]     != "Part1"         ||
                   pieces[4]     != "Scope"         ||
                   pieces[5]     != "Language_Type" ||
                   pieces[6]     != "Ref_Name"      ||
                   pieces[7]     != "Comment")
                {
                    Console.WriteLine("Invalid data, not proceeding");

                    continue;
                }

                line = sr.ReadLine();

                while(line != null)
                {
                    pieces = line.Split('\t');

                    if(pieces.Length != 8)
                    {
                        Console.WriteLine("Invalid data, continuing with next line");
                        line = sr.ReadLine();

                        continue;
                    }

                    for(int p = 1; p < 8; p++)
                        if(pieces[p] == "")
                            pieces[p] = null;

                    codes.Add(new Models.Iso639
                    {
                        Id            = pieces[0],
                        Part2B        = pieces[1],
                        Part2T        = pieces[2],
                        Part1         = pieces[3],
                        Scope         = pieces[4],
                        Type          = pieces[5],
                        ReferenceName = pieces[6],
                        Comment       = pieces[7]
                    });

                    line = sr.ReadLine();
                }
            }

            if(codes.Count == 0)
            {
                Console.WriteLine("No codes found");

                return;
            }

            List<Models.Iso639> existingCodes = context.Iso639.ToList();
            List<Models.Iso639> newCodes      = new List<Models.Iso639>();

            foreach(Models.Iso639 code in codes)
            {
                Models.Iso639 existingCode = existingCodes.FirstOrDefault(c => c.Id == code.Id);

                if(existingCode is null)
                    newCodes.Add(code);
                else
                {
                    bool changed = false;

                    if(code.Part2B != existingCode.Part2B)
                    {
                        existingCode.Part2B = code.Part2B;
                        changed             = true;
                    }

                    if(code.Part2T != existingCode.Part2T)
                    {
                        existingCode.Part2T = code.Part2T;
                        changed             = true;
                    }

                    if(code.Part1 != existingCode.Part1)
                    {
                        existingCode.Part1 = code.Part1;
                        changed            = true;
                    }

                    if(code.Scope != existingCode.Scope)
                    {
                        existingCode.Scope = code.Scope;
                        changed            = true;
                    }

                    if(code.Type != existingCode.Type)
                    {
                        existingCode.Type = code.Type;
                        changed           = true;
                    }

                    if(code.ReferenceName != existingCode.ReferenceName)
                    {
                        existingCode.ReferenceName = code.ReferenceName;
                        changed                    = true;
                    }

                    if(code.Comment != existingCode.Comment)
                    {
                        existingCode.Comment = code.Comment;
                        changed              = true;
                    }

                    if(changed)
                        modified++;
                }
            }

            context.Iso639.AddRange(newCodes);

            Console.WriteLine("{0} language codes added", newCodes.Count);
            Console.WriteLine("{0} language codes modified", modified);
        }
    }
}