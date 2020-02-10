using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Marechai.Database.Models;

namespace Marechai.Helpers
{
    public static class Iso639
    {
        // TODO: Changes file
        // Data files can be found in https://iso639-3.sil.org/code_tables/download_tables
        internal static void Import(cicmContext context)
        {
            if(!Directory.Exists("iso639")) return;

            IEnumerable<string> files    = Directory.EnumerateFiles("iso639", "iso-639-3_*.tab");
            long                added    = 0;
            long                modified = 0;

            foreach(string file in files)
            {
                Console.WriteLine("Importing ISO-639 codes from {0}", file);
                using(FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    using(StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        string   line;
                        string[] pieces;

                        line = sr.ReadLine();

                        if(line is null)
                        {
                            Console.WriteLine("Invalid data, not proceeding");
                            continue;
                        }

                        pieces = line.Split('\t');

                        if(pieces.Length != 8          || pieces[0] != "Id" || pieces[1] != "Part2B" ||
                           pieces[2]     != "Part2T"   ||
                           pieces[3]     != "Part1"    || pieces[4] != "Scope" || pieces[5] != "Language_Type" ||
                           pieces[6]     != "Ref_Name" || pieces[7] != "Comment")
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

                            Marechai.Database.Models.Iso639 lang = context.Iso639.FirstOrDefault(i => i.Id == pieces[0]);

                            if(lang is null)
                            {
                                context.Iso639.Add(new Marechai.Database.Models.Iso639
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
                                added++;
                                continue;
                            }

                            if(pieces[1] != lang.Part2B        || pieces[2] != lang.Part2T ||
                               pieces[3] != lang.Part1         ||
                               pieces[4] != lang.Scope         || pieces[5] != lang.Type ||
                               pieces[6] != lang.ReferenceName ||
                               pieces[7] != lang.Comment)
                            {
                                lang.Part2B        = pieces[1];
                                lang.Part2T        = pieces[2];
                                lang.Part1         = pieces[3];
                                lang.Scope         = pieces[4];
                                lang.Type          = pieces[5];
                                lang.ReferenceName = pieces[6];
                                lang.Comment       = pieces[7];
                                context.Iso639.Update(lang);
                                modified++;
                            }

                            line = sr.ReadLine();
                        }
                    }
                }
            }

            Console.WriteLine("{0} language codes added",    added);
            Console.WriteLine("{0} language codes modified", modified);
        }
    }
}