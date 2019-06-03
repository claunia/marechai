using Microsoft.EntityFrameworkCore;

namespace Cicm.Database.Seeders.Iso639Split
{
    public static class K
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Iso639>().HasData(new
                                                         {
                                                             Id            = "kaa",
                                                             Part2B        = "kaa",
                                                             Part2T        = "kaa",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kara-Kalpak"
                                                         }, new
                                                         {
                                                             Id            = "kab",
                                                             Part2B        = "kab",
                                                             Part2T        = "kab",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kabyle"
                                                         }, new
                                                         {
                                                             Id            = "kac",
                                                             Part2B        = "kac",
                                                             Part2T        = "kac",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kachin"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kad",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Adara"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kae",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Ketangalan"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kaf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Katso"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kag",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kajaman"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kah",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kara (Central African Republic)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kai",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karekare"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kaj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Jju"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kak",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kalanguya"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kal",
                                                             Part2B        = "kal",
                                                             Part2T        = "kal",
                                                             Part1         = "kl",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kalaallisut"
                                                         }, new
                                                         {
                                                             Id            = "kam",
                                                             Part2B        = "kam",
                                                             Part2T        = "kam",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kamba (Kenya)"
                                                         }, new
                                                         {
                                                             Id            = "kan",
                                                             Part2B        = "kan",
                                                             Part2T        = "kan",
                                                             Part1         = "kn",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kannada"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kao",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Xaasongaxango"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kap",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Bezhta"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kaq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Capanahua"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kas",
                                                             Part2B        = "kas",
                                                             Part2T        = "kas",
                                                             Part1         = "ks",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kashmiri"
                                                         }, new
                                                         {
                                                             Id            = "kat",
                                                             Part2B        = "geo",
                                                             Part2T        = "kat",
                                                             Part1         = "ka",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Georgian"
                                                         }, new
                                                         {
                                                             Id            = "kau",
                                                             Part2B        = "kau",
                                                             Part2T        = "kau",
                                                             Part1         = "kr",
                                                             Scope         = "M",
                                                             Type          = "L",
                                                             ReferenceName = "Kanuri"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kav",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Katukína"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kaw",
                                                             Part2B        = "kaw",
                                                             Part2T        = "kaw",
                                                             Scope         = "I",
                                                             Type          = "A",
                                                             ReferenceName = "Kawi"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kax",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kao"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kay",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kamayurá"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kaz",
                                                             Part2B        = "kaz",
                                                             Part2T        = "kaz",
                                                             Part1         = "kk",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kazakh"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kba",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Kalarko"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbb",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Kaxuiâna"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kadiwéu"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kbd",
                                                             Part2B        = "kbd",
                                                             Part2T        = "kbd",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kabardian"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kbe",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kanju"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khamba"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Camsá"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbi",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaptiau"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kari"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Grass Koiari"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbl",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kanembu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Iwal"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kare (Central African Republic)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbo",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Keliko"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kabiyè"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kamano"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kafa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbs",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kande"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbt",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Abadi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kabutra"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Dera (Indonesia)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaiep"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Ap Ma"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kby",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Manga Kanuri"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kbz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Duhwa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kca",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khanty"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kawacha"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Lubila"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Ngkâlmpw Kanum"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kce",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaivi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Ukaan"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Tyap"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kch",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Vono"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kci",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kamantan"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kobiana"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kck",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kalanga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcl",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kela (Papua New Guinea)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Gula (Central African Republic)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Nubi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kco",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kinalakna"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kanga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kamo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Katla"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcs",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koenoem"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kct",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaian"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kami (Tanzania)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kete"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kabwari"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kachama-Ganjule"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Korandje"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kcz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Konongo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kda",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Worimi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kutu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Yankunytjatjara"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kde",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Makonde"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Mamusi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Seba"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Tem"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdi",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kumam"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karamojong"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Numèè"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdl",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Tsikimba"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kagoma"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kunda"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaningdon-Nindem"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koch"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karaim"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdt",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kuy"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kadaru"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koneraw"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kam"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Keder"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kdz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwaja"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kea",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kabuverdianu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "keb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kélé"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kec",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Keiga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ked",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kerewe"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kee",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Eastern Keres"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kef",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kpessi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "keg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Tese"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "keh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Keak"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kei",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kei"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kej",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kadar"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kek",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kekchí"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kel",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kela (Democratic Republic of Congo)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kem",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kemak"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ken",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kenyang"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "keo",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kakwa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kep",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaikadi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "keq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kamar"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ker",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kera"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kes",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kugbo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ket",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Ket"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "keu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Akebu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kev",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kanikkaran"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kew",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "West Kewa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kex",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kukna"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "key",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kupia"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kez",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kukele"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfa",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kodava"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Northwestern Kolami"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Konda-Dora"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Korra Koraga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfe",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kota (India)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kff",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koya"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kudiya"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kurichiya"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfi",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kannada Kurumba"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kemiehua"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kinnauri"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfl",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kung"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khunsari"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kuk"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfo",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koro (Côte d'Ivoire)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Korwa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Korku"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kachhi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfs",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Bilaspuri"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kft",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kanjari"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Katkari"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kurmukar"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kharam Naga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kullu Pahari"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kumaoni"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kfz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koromfé"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kga",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koyaga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kawe"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kge",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Komering"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kube"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kusunda"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgi",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Selangor Sign Language"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Gamale Kham"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaiwá"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgl",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Kunggari"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgm",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Karipúna"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karingani"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgo",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Krongo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaingang"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kamoro"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Abun"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgs",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kumbainggar"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgt",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Somyev"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kobol"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karas"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karon Dori"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kamaru"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kgy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kyerung"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kha",
                                                             Part2B        = "kha",
                                                             Part2T        = "kha",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Khasi"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "khb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Lü"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Tukang Besi North"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Bädi Kanum"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khe",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Korowai"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khuen"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khams Tibetan"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kehu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kuturmi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Halh Mongolian"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khl",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Lusi"
                                                             },
                                                         new
                                                         {
                                                             Id            = "khm",
                                                             Part2B        = "khm",
                                                             Part2T        = "khm",
                                                             Part1         = "km",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Khmer"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "khn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khandesi"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kho",
                                                             Part2B        = "kho",
                                                             Part2T        = "kho",
                                                             Scope         = "I",
                                                             Type          = "A",
                                                             ReferenceName = "Khotanese"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "khp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kapori"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koyra Chiini Songhay"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kharia"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khs",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kasua"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kht",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khamti"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Nkhumbi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khvarshi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khowar"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kanu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kele (Democratic Republic of Congo)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "khz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Keapara"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kia",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kim"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kib",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koalib"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kic",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kickapoo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kid",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koshin"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kie",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kibet"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kif",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Eastern Parbate Kham"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kig",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kimaama"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kih",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kilmeri"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kii",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Kitsai"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kij",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kilivila"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kik",
                                                             Part2B        = "kik",
                                                             Part2T        = "kik",
                                                             Part1         = "ki",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kikuyu"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kil",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kariya"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kim",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karagas"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kin",
                                                             Part2B        = "kin",
                                                             Part2T        = "kin",
                                                             Part1         = "rw",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kinyarwanda"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kio",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kiowa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kip",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Sheshi Kham"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kiq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kosadle"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kir",
                                                             Part2B        = "kir",
                                                             Part2T        = "kir",
                                                             Part1         = "ky",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kirghiz"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kis",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kis"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kit",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Agob"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kiu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kirmanjki (individual language)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kiv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kimbu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kiw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Northeast Kiwai"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kix",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khiamniungan Naga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kiy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kirikiri"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kiz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kisi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kja",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Mlap"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Q'anjob'al"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Coastal Konjo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Southern Kiwai"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kje",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kisar"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khalaj"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khmu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khakas"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kji",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Zabana"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khinalugh"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Highland Konjo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjl",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Western Parbate Kham"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kháng"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kunjen"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjo",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Harijan Kinnauri"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Pwo Eastern Karen"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Western Keres"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kurudu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjs",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "East Kewa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjt",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Phrae Pwo Karen"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kju",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kashaya"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjv",
                                                                 Scope         = "I",
                                                                 Type          = "H",
                                                                 ReferenceName = "Kaikavian Literary Language"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Ramopa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Erave"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kjz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Bumthangkha"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kka",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kakanda"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwerisa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Odoodee"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kinuku"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kke",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kakabe"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kalaktang Monpa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Mabaka Valley Kalinga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khün"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kki",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kagulu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kako"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kokota"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkl",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kosarek Yale"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kiong"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kon Keu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kko",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karko"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Gugubera"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaiku"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kir-Balar"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kks",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Giiwo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkt",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kku",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Tumi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kangean"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Teke-Kukuya"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kohin"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kky",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Guugu Yimidhirr"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kkz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaska"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kla",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Klamath-Modoc"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kiliwa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kolbila"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kld",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Gamilaraay"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kle",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kulung (Nepal)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kendeje"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Tagakaulo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Weliki"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kli",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kalumpang"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Turkic Khalaj"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kono (Nigeria)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kll",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kagan Kalagan"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Migum"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kln",
                                                                 Scope         = "M",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kalenjin"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klo",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kapya"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kamasa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Rumu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khaling"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kls",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kalasha"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klt",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Nukna"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Klao"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Maskelynes"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Tado"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koluwawa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kly",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kalao"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "klz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kabola"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kma",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Konni"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kmb",
                                                             Part2B        = "kmb",
                                                             Part2T        = "kmb",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kimbundu"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kmc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Southern Dong"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Majukayang Kalinga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kme",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Bakole"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kare (Papua New Guinea)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kâte"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kalam"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmi",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kami (Nigeria)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kumarbhag Paharia"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Limos Kalinga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kml",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Tanudan Kalinga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kom (India)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Awtuw"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmo",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwoma"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Gimme"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwama"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Northern Kurdish"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kms",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kamasau"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmt",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kemtuik"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kanite"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karipúna Creole French"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Komo (Democratic Republic of Congo)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Waboda"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koma"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kmz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khorasani Turkish"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kna",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Dera (Nigeria)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Lubuagan Kalinga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Central Kanuri"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Konda"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kne",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kankanaey"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Mankanya"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kng",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koongo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kni",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kanufi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Western Kanjobal"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kuranko"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knl",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Keninjal"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kanamarí"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Konkani (individual language)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kno",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kono (Sierra Leone)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwanja"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kintaq"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaningra"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kns",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kensiu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knt",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Panoan Katukína"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kono (Guinea)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Tabo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kung-Ekoka"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kendayan"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kny",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kanyok"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "knz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kalamsé"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "koa",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Konomala"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "koc",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Kpati"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kod",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kodi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "koe",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kacipo-Balesi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kof",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Kubi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kog",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Cogui"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "koh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koyo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "koi",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Komi-Permyak"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kok",
                                                             Part2B        = "kok",
                                                             Part2T        = "kok",
                                                             Scope         = "M",
                                                             Type          = "L",
                                                             ReferenceName = "Konkani (macrolanguage)"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kol",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kol (Papua New Guinea)"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kom",
                                                             Part2B        = "kom",
                                                             Part2T        = "kom",
                                                             Part1         = "kv",
                                                             Scope         = "M",
                                                             Type          = "L",
                                                             ReferenceName = "Komi"
                                                         }, new
                                                         {
                                                             Id            = "kon",
                                                             Part2B        = "kon",
                                                             Part2T        = "kon",
                                                             Part1         = "kg",
                                                             Scope         = "M",
                                                             Type          = "L",
                                                             ReferenceName = "Kongo"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "koo",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Konzo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kop",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Waube"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "koq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kota (Gabon)"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kor",
                                                             Part2B        = "kor",
                                                             Part2T        = "kor",
                                                             Part1         = "ko",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Korean"
                                                         }, new
                                                         {
                                                             Id            = "kos",
                                                             Part2B        = "kos",
                                                             Part2T        = "kos",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kosraean"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kot",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Lagwan"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kou",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koke"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kov",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kudu-Camo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kow",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kugama"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "koy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koyukon"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "koz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Korak"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpa",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kutto"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Mullu Kurumba"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Curripaco"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koba"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kpe",
                                                             Part2B        = "kpe",
                                                             Part2T        = "kpe",
                                                             Scope         = "M",
                                                             Type          = "L",
                                                             ReferenceName = "Kpelle"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kpf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Komba"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kapingamarangi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kph",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kplang"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpi",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kofei"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karajá"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kpan"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpl",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kpala"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koho"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpn",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Kepkiriwát"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpo",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Ikposo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Korupun-Sela"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Korafe-Yegha"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kps",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Tehit"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpt",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karata"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kafoa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Komi-Zyrian"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kobon"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Mountain Koiali"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koryak"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kpz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kupsabiny"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqa",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Mum"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kovai"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Doromu-Koki"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koy Sanjaq Surat"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqe",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kalagan"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kakabai"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khe"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kisankasa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqi",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koitabu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koromira"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kotafon Gbe"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kql",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kyenele"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khisa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaonde"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqo",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Eastern Krahn"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kimré"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Krenak"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kimaragang"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqs",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Northern Kissi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqt",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Klias River Kadazan"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqu",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Seroa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Okolod"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kandas"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Mser"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koorete"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kqz",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Korana"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kra",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kumhali"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "krb",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Karkin"
                                                             },
                                                         new
                                                         {
                                                             Id            = "krc",
                                                             Part2B        = "krc",
                                                             Part2T        = "krc",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Karachay-Balkar"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "krd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kairui-Midiki"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kre",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Panará"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "krf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koro (Vanuatu)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "krh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kurama"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kri",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Krio"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "krj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kinaray-A"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "krk",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Kerek"
                                                             },
                                                         new
                                                         {
                                                             Id            = "krl",
                                                             Part2B        = "krl",
                                                             Part2T        = "krl",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Karelian"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "krn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Sapo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "krp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Korop"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "krr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Krung"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "krs",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Gbaya (Sudan)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "krt",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Tumari Kanuri"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kru",
                                                             Part2B        = "kru",
                                                             Part2T        = "kru",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kurukh"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "krv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kavet"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "krw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Western Krahn"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "krx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karon"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kry",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kryts"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "krz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Sota Kanum"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksa",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Shuwa-Zamani"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Shambala"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Southern Kalinga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kuanua"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kse",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kuni"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Bafia"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kusaghe"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kölsch"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksi",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Krisa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Uare"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kansa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksl",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kumalu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kumba"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kasiguranin"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kso",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kofa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaba"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwaami"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Borong"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kss",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Southern Kisi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kst",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Winyé"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khamyang"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kusu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "S'gaw Karen"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kedang"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kharia Thar"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ksz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kodaku"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kta",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Katua"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kambaata"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kholok"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kokata"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kte",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Nubri"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwami"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktg",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Kalkutung"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kth",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karanga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kti",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "North Muyu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Plapo Krumen"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktk",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Kaniet"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktl",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koroshi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kurti"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karitiâna"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kto",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kuot"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaduo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktq",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Katabaga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kts",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "South Muyu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktt",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Ketum"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kituba (Democratic Republic of Congo)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Eastern Katu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktw",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Kato"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaxararí"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kty",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kango (Bas-Uélé District)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "ktz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Juǀʼhoan"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kua",
                                                             Part2B        = "kua",
                                                             Part2T        = "kua",
                                                             Part1         = "kj",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kuanyama"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kub",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kutep"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kuc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwinsu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kud",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "'Auhelawa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kue",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kuman (Papua New Guinea)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kuf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Western Katu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kug",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kupa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kuh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kushi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kui",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kuikúro-Kalapálo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kuj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kuria"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kuk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kepo'"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kul",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kulere"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kum",
                                                             Part2B        = "kum",
                                                             Part2T        = "kum",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kumyk"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kun",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kunama"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kuo",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kumukio"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kup",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kunimaipa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kuq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karipuna"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kur",
                                                             Part2B        = "kur",
                                                             Part2T        = "kur",
                                                             Part1         = "ku",
                                                             Scope         = "M",
                                                             Type          = "L",
                                                             ReferenceName = "Kurdish"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kus",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kusaal"
                                                             },
                                                         new
                                                         {
                                                             Id            = "kut",
                                                             Part2B        = "kut",
                                                             Part2T        = "kut",
                                                             Scope         = "I",
                                                             Type          = "L",
                                                             ReferenceName = "Kutenai"
                                                         },
                                                         new
                                                             {
                                                                 Id            = "kuu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Upper Kuskokwim"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kuv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kur"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kuw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kpagua"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kux",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kukatja"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kuy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kuuku-Ya'u"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kuz",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Kunza"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kva",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Bagvalal"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kubu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kove"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kui (Indonesia)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kve",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kalabakan"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kabalai"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kuni-Boazi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Komodo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvi",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwang"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Psikye"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Korean Sign Language"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvl",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kayaw"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kendem"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Border Kuna"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvo",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Dobel"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kompane"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Geba Karen"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kerinci"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvt",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Lahta Karen"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Yinbaw Karen"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kola"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Wersing"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Parkari Koli"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Yintale Karen"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kvz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Tsakwambo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwa",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Dâw"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Likwala"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwaio"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwe",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwerba"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwara'ae"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Sara Kaba Deme"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kowiai"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwi",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Awa-Cuaiquer"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwanga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwakiutl"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwl",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kofyar"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwambi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwangali"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwo",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwomtari"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kodia"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwer"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kws",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwese"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwt",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwesten"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwakum"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Sara Kaba Náà"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kww",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwinti"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Khirwar"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "San Salvador Kongo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kwz",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Kwadi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxa",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kairiru"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Krobu"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Konso"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Brunei"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Manumanaw Karen"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karo (Ethiopia)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxi",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Keningau Murut"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kulfa"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Zayein Karen"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxl",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Nepali Kurux"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Northern Khmer"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kanowit-Tanjong Melanau"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxo",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Kanoé"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Wadiyara Koli"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Smärky Kanum"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koro (Papua New Guinea)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxs",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kangjia"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxt",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Koiwat"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kui (India)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kuvi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Konai"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Likuba"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kayong"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kxz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kerewo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kya",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kwaya"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Butbut Kalinga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kyaka"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karey"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kye",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Krache"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kouya"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Keyagana"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyh",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karok"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyi",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kiput"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyj",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karao"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyk",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kamayo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyl",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kalapuya"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kym",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kpatili"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Northern Binukidnon"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyo",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kelon"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kang"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kenga"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kuruáya"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kys",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Baram Kayan"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyt",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kayagar"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Western Kayah"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kayort"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyw",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kudmali"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyx",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Rapoisi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kambaira"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kyz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kayabí"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kza",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Western Karaboro"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzb",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaibobo"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzc",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Bondoukou Kulango"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzd",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kadai"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kze",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kosena"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzf",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Da'a Kaili"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzg",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kikai"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzi",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kelabit"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzk",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Kazukuru"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzl",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kayeli"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzm",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kais"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzn",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kokola"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzo",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaningi"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzp",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaidipang"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzq",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kaike"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzr",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Karang"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzs",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Sugut Dusun"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzu",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kayupulau"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzv",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Komyandaret"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzw",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Karirí-Xocó"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzx",
                                                                 Scope         = "I",
                                                                 Type          = "E",
                                                                 ReferenceName = "Kamarian"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzy",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kango (Tshopo District)"
                                                             },
                                                         new
                                                             {
                                                                 Id            = "kzz",
                                                                 Scope         = "I",
                                                                 Type          = "L",
                                                                 ReferenceName = "Kalabra"
                                                             });
        }
    }
}