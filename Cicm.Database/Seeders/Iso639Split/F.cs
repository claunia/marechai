using Microsoft.EntityFrameworkCore;

namespace Cicm.Database.Seeders.Iso639Split
{
    public static class F
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Iso639>()
                        .HasData(new {Id = "faa", Scope = "I", Type = "L", ReferenceName = "Fasu"},
                                 new {Id = "fab", Scope = "I", Type = "L", ReferenceName = "Fa d'Ambu"},
                                 new {Id = "fad", Scope = "I", Type = "L", ReferenceName = "Wagi"},
                                 new {Id = "faf", Scope = "I", Type = "L", ReferenceName = "Fagani"},
                                 new {Id = "fag", Scope = "I", Type = "L", ReferenceName = "Finongan"},
                                 new {Id = "fah", Scope = "I", Type = "L", ReferenceName = "Baissa Fali"},
                                 new {Id = "fai", Scope = "I", Type = "L", ReferenceName = "Faiwol"},
                                 new {Id = "faj", Scope = "I", Type = "L", ReferenceName = "Faita"},
                                 new {Id = "fak", Scope = "I", Type = "L", ReferenceName = "Fang (Cameroon)"},
                                 new {Id = "fal", Scope = "I", Type = "L", ReferenceName = "South Fali"},
                                 new {Id = "fam", Scope = "I", Type = "L", ReferenceName = "Fam"},
                                 new
                                 {
                                     Id            = "fan",
                                     Part2B        = "fan",
                                     Part2T        = "fan",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Fang (Equatorial Guinea)"
                                 }, new
                                 {
                                     Id            = "fao",
                                     Part2B        = "fao",
                                     Part2T        = "fao",
                                     Part1         = "fo",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Faroese"
                                 }, new {Id = "fap", Scope = "I", Type = "L", ReferenceName = "Paloor"},
                                 new {Id    = "far", Scope = "I", Type = "L", ReferenceName = "Fataleka"},
                                 new
                                 {
                                     Id            = "fas",
                                     Part2B        = "per",
                                     Part2T        = "fas",
                                     Part1         = "fa",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Persian"
                                 }, new
                                 {
                                     Id            = "fat",
                                     Part2B        = "fat",
                                     Part2T        = "fat",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Fanti"
                                 }, new {Id = "fau", Scope = "I", Type = "L", ReferenceName = "Fayu"},
                                 new {Id    = "fax", Scope = "I", Type = "L", ReferenceName = "Fala"},
                                 new {Id    = "fay", Scope = "I", Type = "L", ReferenceName = "Southwestern Fars"},
                                 new {Id    = "faz", Scope = "I", Type = "L", ReferenceName = "Northwestern Fars"},
                                 new {Id    = "fbl", Scope = "I", Type = "L", ReferenceName = "West Albay Bikol"},
                                 new {Id    = "fcs", Scope = "I", Type = "L", ReferenceName = "Quebec Sign Language"},
                                 new {Id    = "fer", Scope = "I", Type = "L", ReferenceName = "Feroge"},
                                 new {Id    = "ffi", Scope = "I", Type = "L", ReferenceName = "Foia Foia"},
                                 new {Id    = "ffm", Scope = "I", Type = "L", ReferenceName = "Maasina Fulfulde"},
                                 new {Id    = "fgr", Scope = "I", Type = "L", ReferenceName = "Fongoro"},
                                 new {Id    = "fia", Scope = "I", Type = "L", ReferenceName = "Nobiin"},
                                 new {Id    = "fie", Scope = "I", Type = "L", ReferenceName = "Fyer"},
                                 new
                                 {
                                     Id            = "fij",
                                     Part2B        = "fij",
                                     Part2T        = "fij",
                                     Part1         = "fj",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Fijian"
                                 }, new
                                 {
                                     Id            = "fil",
                                     Part2B        = "fil",
                                     Part2T        = "fil",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Filipino"
                                 }, new
                                 {
                                     Id            = "fin",
                                     Part2B        = "fin",
                                     Part2T        = "fin",
                                     Part1         = "fi",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Finnish"
                                 }, new {Id = "fip", Scope = "I", Type = "L", ReferenceName = "Fipa"},
                                 new {Id    = "fir", Scope = "I", Type = "L", ReferenceName = "Firan"},
                                 new {Id    = "fit", Scope = "I", Type = "L", ReferenceName = "Tornedalen Finnish"},
                                 new {Id    = "fiw", Scope = "I", Type = "L", ReferenceName = "Fiwaga"},
                                 new {Id    = "fkk", Scope = "I", Type = "L", ReferenceName = "Kirya-Konzəl"},
                                 new {Id    = "fkv", Scope = "I", Type = "L", ReferenceName = "Kven Finnish"},
                                 new
                                     {
                                         Id            = "fla",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Kalispel-Pend d'Oreille"
                                     },
                                 new {Id = "flh", Scope = "I", Type = "L", ReferenceName = "Foau"},
                                 new {Id = "fli", Scope = "I", Type = "L", ReferenceName = "Fali"},
                                 new {Id = "fll", Scope = "I", Type = "L", ReferenceName = "North Fali"},
                                 new {Id = "fln", Scope = "I", Type = "E", ReferenceName = "Flinders Island"},
                                 new {Id = "flr", Scope = "I", Type = "L", ReferenceName = "Fuliiru"},
                                 new {Id = "fly", Scope = "I", Type = "L", ReferenceName = "Flaaitaal"},
                                 new {Id = "fmp", Scope = "I", Type = "L", ReferenceName = "Fe'fe'"},
                                 new {Id = "fmu", Scope = "I", Type = "L", ReferenceName = "Far Western Muria"},
                                 new {Id = "fnb", Scope = "I", Type = "L", ReferenceName = "Fanbak"},
                                 new {Id = "fng", Scope = "I", Type = "L", ReferenceName = "Fanagalo"},
                                 new {Id = "fni", Scope = "I", Type = "L", ReferenceName = "Fania"},
                                 new {Id = "fod", Scope = "I", Type = "L", ReferenceName = "Foodo"},
                                 new {Id = "foi", Scope = "I", Type = "L", ReferenceName = "Foi"},
                                 new {Id = "fom", Scope = "I", Type = "L", ReferenceName = "Foma"},
                                 new
                                 {
                                     Id            = "fon",
                                     Part2B        = "fon",
                                     Part2T        = "fon",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Fon"
                                 }, new {Id = "for", Scope = "I", Type = "L", ReferenceName = "Fore"},
                                 new {Id    = "fos", Scope = "I", Type = "E", ReferenceName = "Siraya"},
                                 new
                                 {
                                     Id            = "fpe",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Fernando Po Creole English"
                                 }, new {Id = "fqs", Scope = "I", Type = "L", ReferenceName = "Fas"},
                                 new
                                 {
                                     Id            = "fra",
                                     Part2B        = "fre",
                                     Part2T        = "fra",
                                     Part1         = "fr",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "French"
                                 }, new {Id = "frc", Scope = "I", Type = "L", ReferenceName = "Cajun French"},
                                 new {Id    = "frd", Scope = "I", Type = "L", ReferenceName = "Fordata"},
                                 new {Id    = "frk", Scope = "I", Type = "H", ReferenceName = "Frankish"},
                                 new
                                 {
                                     Id            = "frm",
                                     Part2B        = "frm",
                                     Part2T        = "frm",
                                     Scope         = "I",
                                     Type          = "H",
                                     ReferenceName = "Middle French (ca. 1400-1600)"
                                 }, new
                                 {
                                     Id            = "fro",
                                     Part2B        = "fro",
                                     Part2T        = "fro",
                                     Scope         = "I",
                                     Type          = "H",
                                     ReferenceName = "Old French (842-ca. 1400)"
                                 }, new {Id = "frp", Scope = "I", Type = "L", ReferenceName = "Arpitan"},
                                 new {Id    = "frq", Scope = "I", Type = "L", ReferenceName = "Forak"},
                                 new
                                 {
                                     Id            = "frr",
                                     Part2B        = "frr",
                                     Part2T        = "frr",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Northern Frisian"
                                 }, new
                                 {
                                     Id            = "frs",
                                     Part2B        = "frs",
                                     Part2T        = "frs",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Eastern Frisian"
                                 }, new {Id = "frt", Scope = "I", Type = "L", ReferenceName = "Fortsenal"},
                                 new
                                 {
                                     Id            = "fry",
                                     Part2B        = "fry",
                                     Part2T        = "fry",
                                     Part1         = "fy",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Western Frisian"
                                 }, new {Id = "fse", Scope = "I", Type = "L", ReferenceName = "Finnish Sign Language"},
                                 new {Id    = "fsl", Scope = "I", Type = "L", ReferenceName = "French Sign Language"},
                                 new
                                 {
                                     Id            = "fss",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Finland-Swedish Sign Language"
                                 }, new {Id = "fub", Scope = "I", Type = "L", ReferenceName = "Adamawa Fulfulde"},
                                 new {Id    = "fuc", Scope = "I", Type = "L", ReferenceName = "Pulaar"},
                                 new {Id    = "fud", Scope = "I", Type = "L", ReferenceName = "East Futuna"},
                                 new {Id    = "fue", Scope = "I", Type = "L", ReferenceName = "Borgu Fulfulde"},
                                 new {Id    = "fuf", Scope = "I", Type = "L", ReferenceName = "Pular"},
                                 new
                                     {
                                         Id            = "fuh",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Western Niger Fulfulde"
                                     },
                                 new {Id = "fui", Scope = "I", Type = "L", ReferenceName = "Bagirmi Fulfulde"},
                                 new {Id = "fuj", Scope = "I", Type = "L", ReferenceName = "Ko"},
                                 new
                                 {
                                     Id            = "ful",
                                     Part2B        = "ful",
                                     Part2T        = "ful",
                                     Part1         = "ff",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Fulah"
                                 }, new {Id = "fum", Scope = "I", Type = "L", ReferenceName = "Fum"},
                                 new {Id    = "fun", Scope = "I", Type = "L", ReferenceName = "Fulniô"},
                                 new
                                 {
                                     Id            = "fuq",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Central-Eastern Niger Fulfulde"
                                 }, new
                                 {
                                     Id            = "fur",
                                     Part2B        = "fur",
                                     Part2T        = "fur",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Friulian"
                                 }, new {Id = "fut", Scope = "I", Type = "L", ReferenceName = "Futuna-Aniwa"},
                                 new {Id    = "fuu", Scope = "I", Type = "L", ReferenceName = "Furu"},
                                 new {Id    = "fuv", Scope = "I", Type = "L", ReferenceName = "Nigerian Fulfulde"},
                                 new {Id    = "fuy", Scope = "I", Type = "L", ReferenceName = "Fuyug"},
                                 new {Id    = "fvr", Scope = "I", Type = "L", ReferenceName = "Fur"},
                                 new {Id    = "fwa", Scope = "I", Type = "L", ReferenceName = "Fwâi"},
                                 new {Id    = "fwe", Scope = "I", Type = "L", ReferenceName = "Fwe"});
        }
    }
}