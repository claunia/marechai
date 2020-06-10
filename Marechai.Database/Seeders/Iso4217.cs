using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Marechai.Database.Models;
using Marechai.Database.Schemas;

namespace Marechai.Database.Seeders
{
    public class Iso4217
    {
        // Lists from https://www.currency-iso.org/en/home/tables/table-a1.html
        public static void Seed(MarechaiContext context)
        {
            if(!Directory.Exists("iso4217"))
                return;

            var codes = new Iso4217Xml.Root();

            try
            {
                if(File.Exists(Path.Combine("iso4217", "list_one.xml")))
                {
                    using var sr      = new StreamReader(Path.Combine("iso4217", "list_one.xml"));
                    var       xs      = new XmlSerializer(typeof(Iso4217Xml.Root));
                    var       listOne = (Iso4217Xml.Root)xs.Deserialize(sr);

                    if(listOne.Published > codes.Published)
                        codes.Published = listOne.Published;

                    codes.Current = listOne.Current;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception trying to read list one.");
            }

            try
            {
                if(File.Exists(Path.Combine("iso4217", "list_three.xml")))
                {
                    using var sr        = new StreamReader(Path.Combine("iso4217", "list_three.xml"));
                    var       xs        = new XmlSerializer(typeof(Iso4217Xml.Root));
                    var       listThree = (Iso4217Xml.Root)xs.Deserialize(sr);

                    if(listThree.Published > codes.Published)
                        codes.Published = listThree.Published;

                    codes.Historical = listThree.Historical;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception trying to read list three.");
            }

            if(codes.Current is null &&
               codes.Historical is null)
            {
                Console.WriteLine("No ISO-4217 codes could be read.");

                return;
            }

            Dictionary<string, Models.Iso4217> existingCodes = context.Iso4217.ToDictionary(c => c.Code);
            Dictionary<string, Models.Iso4217> newCodes      = new Dictionary<string, Models.Iso4217>();

            long modified = 0;

            codes.Current    ??= new Iso4217Xml.Currency[0];
            codes.Historical ??= new Iso4217Xml.Currency[0];

            foreach(Iso4217Xml.Currency currency in codes.Historical)
            {
                bool     changed = false;
                byte     minorUnits;
                DateTime withdrawn;

                if(currency.Code is null)
                    continue;

                if(!existingCodes.TryGetValue(currency.Code, out Models.Iso4217 existing))
                {
                    var newCode = new Models.Iso4217
                    {
                        Code = currency.Code, Name = currency.Name, Numeric = currency.Number
                    };

                    if(byte.TryParse(currency.MinorUnits, out minorUnits))
                        newCode.MinorUnits = minorUnits;

                    if(DateTime.TryParseExact(currency.Withdrawn, "yyyy'-'MM", CultureInfo.InvariantCulture,
                                              DateTimeStyles.AssumeUniversal, out withdrawn))
                        newCode.Withdrawn = withdrawn;

                    newCodes[currency.Code]      = newCode;
                    existingCodes[currency.Code] = newCode;

                    continue;
                }

                if(existing.Code != currency.Code)
                {
                    existing.Code = currency.Code;
                    changed       = true;
                }

                if(existing.Name != currency.Name)
                {
                    existing.Name = currency.Name;
                    changed       = true;
                }

                if(byte.TryParse(currency.MinorUnits, out minorUnits) &&
                   existing.MinorUnits != minorUnits)
                {
                    existing.MinorUnits = minorUnits;
                    changed             = true;
                }

                if(DateTime.TryParseExact(currency.Withdrawn, "yyyy'-'MM", CultureInfo.InvariantCulture,
                                          DateTimeStyles.AssumeUniversal, out withdrawn) &&
                   existing.Withdrawn != withdrawn)
                {
                    existing.Withdrawn = withdrawn;
                    changed            = true;
                }

                if(changed)
                    modified++;
            }

            foreach(Iso4217Xml.Currency currency in codes.Current)
            {
                bool changed = false;
                byte minorUnits;

                if(currency.Code is null)
                    continue;

                if(!existingCodes.TryGetValue(currency.Code, out Models.Iso4217 existing))
                {
                    var newCode = new Models.Iso4217
                    {
                        Code = currency.Code, Name = currency.Name, Numeric = currency.Number
                    };

                    if(byte.TryParse(currency.MinorUnits, out minorUnits))
                        newCode.MinorUnits = minorUnits;

                    newCodes[currency.Code]      = newCode;
                    existingCodes[currency.Code] = newCode;

                    continue;
                }

                if(existing.Code != currency.Code)
                {
                    existing.Code = currency.Code;
                    changed       = true;
                }

                if(existing.Name != currency.Name)
                {
                    existing.Name = currency.Name;
                    changed       = true;
                }

                if(byte.TryParse(currency.MinorUnits, out minorUnits) &&
                   existing.MinorUnits != minorUnits)
                {
                    existing.MinorUnits = minorUnits;
                    changed             = true;
                }

                if(existing.Withdrawn != null)
                {
                    existing.Withdrawn = null;
                    changed            = true;
                }

                if(changed)
                    modified++;
            }

            context.Iso4217.AddRange(newCodes.Values);

            Console.WriteLine("{0} currency codes added", newCodes.Count);
            Console.WriteLine("{0} currency codes modified", modified);
        }
    }
}