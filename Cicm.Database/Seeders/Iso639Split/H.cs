using Microsoft.EntityFrameworkCore;

namespace Cicm.Database.Seeders.Iso639Split
{
    public static class H
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Iso639>()
                        .HasData(new {Id = "haa", Scope = "I", Type = "L", ReferenceName = "Han"},
                                 new {Id = "hab", Scope = "I", Type = "L", ReferenceName = "Hanoi Sign Language"},
                                 new {Id = "hac", Scope = "I", Type = "L", ReferenceName = "Gurani"},
                                 new {Id = "had", Scope = "I", Type = "L", ReferenceName = "Hatam"},
                                 new {Id = "hae", Scope = "I", Type = "L", ReferenceName = "Eastern Oromo"},
                                 new {Id = "haf", Scope = "I", Type = "L", ReferenceName = "Haiphong Sign Language"},
                                 new {Id = "hag", Scope = "I", Type = "L", ReferenceName = "Hanga"},
                                 new {Id = "hah", Scope = "I", Type = "L", ReferenceName = "Hahon"},
                                 new
                                 {
                                     Id            = "hai",
                                     Part2B        = "hai",
                                     Part2T        = "hai",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Haida"
                                 }, new {Id = "haj", Scope = "I", Type = "L", ReferenceName = "Hajong"},
                                 new {Id    = "hak", Scope = "I", Type = "L", ReferenceName = "Hakka Chinese"},
                                 new {Id    = "hal", Scope = "I", Type = "L", ReferenceName = "Halang"},
                                 new {Id    = "ham", Scope = "I", Type = "L", ReferenceName = "Hewa"},
                                 new {Id    = "han", Scope = "I", Type = "L", ReferenceName = "Hangaza"},
                                 new {Id    = "hao", Scope = "I", Type = "L", ReferenceName = "Hakö"},
                                 new {Id    = "hap", Scope = "I", Type = "L", ReferenceName = "Hupla"},
                                 new {Id    = "haq", Scope = "I", Type = "L", ReferenceName = "Ha"},
                                 new {Id    = "har", Scope = "I", Type = "L", ReferenceName = "Harari"},
                                 new {Id    = "has", Scope = "I", Type = "L", ReferenceName = "Haisla"},
                                 new
                                 {
                                     Id            = "hat",
                                     Part2B        = "hat",
                                     Part2T        = "hat",
                                     Part1         = "ht",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Haitian"
                                 }, new
                                 {
                                     Id            = "hau",
                                     Part2B        = "hau",
                                     Part2T        = "hau",
                                     Part1         = "ha",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Hausa"
                                 }, new {Id = "hav", Scope = "I", Type = "L", ReferenceName = "Havu"},
                                 new
                                 {
                                     Id            = "haw",
                                     Part2B        = "haw",
                                     Part2T        = "haw",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Hawaiian"
                                 }, new {Id = "hax", Scope = "I", Type = "L", ReferenceName = "Southern Haida"},
                                 new {Id    = "hay", Scope = "I", Type = "L", ReferenceName = "Haya"},
                                 new {Id    = "haz", Scope = "I", Type = "L", ReferenceName = "Hazaragi"},
                                 new {Id    = "hba", Scope = "I", Type = "L", ReferenceName = "Hamba"},
                                 new {Id    = "hbb", Scope = "I", Type = "L", ReferenceName = "Huba"},
                                 new {Id    = "hbn", Scope = "I", Type = "L", ReferenceName = "Heiban"},
                                 new {Id    = "hbo", Scope = "I", Type = "H", ReferenceName = "Ancient Hebrew"},
                                 new
                                 {
                                     Id            = "hbs",
                                     Part1         = "sh",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Serbo-Croatian"
                                 }, new {Id = "hbu", Scope = "I", Type = "L", ReferenceName = "Habu"},
                                 new {Id    = "hca", Scope = "I", Type = "L", ReferenceName = "Andaman Creole Hindi"},
                                 new {Id    = "hch", Scope = "I", Type = "L", ReferenceName = "Huichol"},
                                 new {Id    = "hdn", Scope = "I", Type = "L", ReferenceName = "Northern Haida"},
                                 new {Id    = "hds", Scope = "I", Type = "L", ReferenceName = "Honduras Sign Language"},
                                 new {Id    = "hdy", Scope = "I", Type = "L", ReferenceName = "Hadiyya"},
                                 new {Id    = "hea", Scope = "I", Type = "L", ReferenceName = "Northern Qiandong Miao"},
                                 new
                                 {
                                     Id            = "heb",
                                     Part2B        = "heb",
                                     Part2T        = "heb",
                                     Part1         = "he",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Hebrew"
                                 }, new {Id = "hed", Scope = "I", Type = "L", ReferenceName = "Herdé"},
                                 new {Id    = "heg", Scope = "I", Type = "L", ReferenceName = "Helong"},
                                 new {Id    = "heh", Scope = "I", Type = "L", ReferenceName = "Hehe"},
                                 new {Id    = "hei", Scope = "I", Type = "L", ReferenceName = "Heiltsuk"},
                                 new {Id    = "hem", Scope = "I", Type = "L", ReferenceName = "Hemba"},
                                 new
                                 {
                                     Id            = "her",
                                     Part2B        = "her",
                                     Part2T        = "her",
                                     Part1         = "hz",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Herero"
                                 }, new {Id = "hgm", Scope = "I", Type = "L", ReferenceName = "Haiǁom"},
                                 new {Id    = "hgw", Scope = "I", Type = "L", ReferenceName = "Haigwai"},
                                 new {Id    = "hhi", Scope = "I", Type = "L", ReferenceName = "Hoia Hoia"},
                                 new {Id    = "hhr", Scope = "I", Type = "L", ReferenceName = "Kerak"},
                                 new {Id    = "hhy", Scope = "I", Type = "L", ReferenceName = "Hoyahoya"},
                                 new {Id    = "hia", Scope = "I", Type = "L", ReferenceName = "Lamang"},
                                 new {Id    = "hib", Scope = "I", Type = "E", ReferenceName = "Hibito"},
                                 new {Id    = "hid", Scope = "I", Type = "L", ReferenceName = "Hidatsa"},
                                 new {Id    = "hif", Scope = "I", Type = "L", ReferenceName = "Fiji Hindi"},
                                 new {Id    = "hig", Scope = "I", Type = "L", ReferenceName = "Kamwe"},
                                 new {Id    = "hih", Scope = "I", Type = "L", ReferenceName = "Pamosu"},
                                 new {Id    = "hii", Scope = "I", Type = "L", ReferenceName = "Hinduri"},
                                 new {Id    = "hij", Scope = "I", Type = "L", ReferenceName = "Hijuk"},
                                 new {Id    = "hik", Scope = "I", Type = "L", ReferenceName = "Seit-Kaitetu"},
                                 new
                                 {
                                     Id            = "hil",
                                     Part2B        = "hil",
                                     Part2T        = "hil",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Hiligaynon"
                                 }, new
                                 {
                                     Id            = "hin",
                                     Part2B        = "hin",
                                     Part2T        = "hin",
                                     Part1         = "hi",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Hindi"
                                 }, new {Id = "hio", Scope = "I", Type = "L", ReferenceName = "Tsoa"},
                                 new {Id    = "hir", Scope = "I", Type = "L", ReferenceName = "Himarimã"},
                                 new
                                 {
                                     Id            = "hit",
                                     Part2B        = "hit",
                                     Part2T        = "hit",
                                     Scope         = "I",
                                     Type          = "A",
                                     ReferenceName = "Hittite"
                                 }, new {Id = "hiw", Scope = "I", Type = "L", ReferenceName = "Hiw"},
                                 new {Id    = "hix", Scope = "I", Type = "L", ReferenceName = "Hixkaryána"},
                                 new {Id    = "hji", Scope = "I", Type = "L", ReferenceName = "Haji"},
                                 new {Id    = "hka", Scope = "I", Type = "L", ReferenceName = "Kahe"},
                                 new {Id    = "hke", Scope = "I", Type = "L", ReferenceName = "Hunde"},
                                 new {Id    = "hkk", Scope = "I", Type = "L", ReferenceName = "Hunjara-Kaina Ke"},
                                 new {Id    = "hkn", Scope = "I", Type = "L", ReferenceName = "Mel-Khaonh"},
                                 new
                                     {
                                         Id            = "hks",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Hong Kong Sign Language"
                                     },
                                 new {Id = "hla", Scope = "I", Type = "L", ReferenceName = "Halia"},
                                 new {Id = "hlb", Scope = "I", Type = "L", ReferenceName = "Halbi"},
                                 new {Id = "hld", Scope = "I", Type = "L", ReferenceName = "Halang Doan"},
                                 new {Id = "hle", Scope = "I", Type = "L", ReferenceName = "Hlersu"},
                                 new {Id = "hlt", Scope = "I", Type = "L", ReferenceName = "Matu Chin"},
                                 new {Id = "hlu", Scope = "I", Type = "A", ReferenceName = "Hieroglyphic Luwian"},
                                 new {Id = "hma", Scope = "I", Type = "L", ReferenceName = "Southern Mashan Hmong"},
                                 new {Id = "hmb", Scope = "I", Type = "L", ReferenceName = "Humburi Senni Songhay"},
                                 new {Id = "hmc", Scope = "I", Type = "L", ReferenceName = "Central Huishui Hmong"},
                                 new {Id = "hmd", Scope = "I", Type = "L", ReferenceName = "Large Flowery Miao"},
                                 new {Id = "hme", Scope = "I", Type = "L", ReferenceName = "Eastern Huishui Hmong"},
                                 new {Id = "hmf", Scope = "I", Type = "L", ReferenceName = "Hmong Don"},
                                 new
                                 {
                                     Id            = "hmg",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Southwestern Guiyang Hmong"
                                 },
                                 new
                                 {
                                     Id            = "hmh",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Southwestern Huishui Hmong"
                                 },
                                 new {Id = "hmi", Scope = "I", Type = "L", ReferenceName = "Northern Huishui Hmong"},
                                 new {Id = "hmj", Scope = "I", Type = "L", ReferenceName = "Ge"},
                                 new {Id = "hmk", Scope = "I", Type = "A", ReferenceName = "Maek"},
                                 new {Id = "hml", Scope = "I", Type = "L", ReferenceName = "Luopohe Hmong"},
                                 new {Id = "hmm", Scope = "I", Type = "L", ReferenceName = "Central Mashan Hmong"},
                                 new
                                 {
                                     Id            = "hmn",
                                     Part2B        = "hmn",
                                     Part2T        = "hmn",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Hmong"
                                 }, new
                                 {
                                     Id            = "hmo",
                                     Part2B        = "hmo",
                                     Part2T        = "hmo",
                                     Part1         = "ho",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Hiri Motu"
                                 }, new {Id = "hmp", Scope = "I", Type = "L", ReferenceName = "Northern Mashan Hmong"},
                                 new {Id    = "hmq", Scope = "I", Type = "L", ReferenceName = "Eastern Qiandong Miao"},
                                 new {Id    = "hmr", Scope = "I", Type = "L", ReferenceName = "Hmar"},
                                 new {Id    = "hms", Scope = "I", Type = "L", ReferenceName = "Southern Qiandong Miao"},
                                 new {Id    = "hmt", Scope = "I", Type = "L", ReferenceName = "Hamtai"},
                                 new {Id    = "hmu", Scope = "I", Type = "L", ReferenceName = "Hamap"},
                                 new {Id    = "hmv", Scope = "I", Type = "L", ReferenceName = "Hmong Dô"},
                                 new {Id    = "hmw", Scope = "I", Type = "L", ReferenceName = "Western Mashan Hmong"},
                                 new {Id    = "hmy", Scope = "I", Type = "L", ReferenceName = "Southern Guiyang Hmong"},
                                 new {Id    = "hmz", Scope = "I", Type = "L", ReferenceName = "Hmong Shua"},
                                 new {Id    = "hna", Scope = "I", Type = "L", ReferenceName = "Mina (Cameroon)"},
                                 new {Id    = "hnd", Scope = "I", Type = "L", ReferenceName = "Southern Hindko"},
                                 new {Id    = "hne", Scope = "I", Type = "L", ReferenceName = "Chhattisgarhi"},
                                 new {Id    = "hnh", Scope = "I", Type = "L", ReferenceName = "ǁAni"},
                                 new {Id    = "hni", Scope = "I", Type = "L", ReferenceName = "Hani"},
                                 new {Id    = "hnj", Scope = "I", Type = "L", ReferenceName = "Hmong Njua"},
                                 new {Id    = "hnn", Scope = "I", Type = "L", ReferenceName = "Hanunoo"},
                                 new {Id    = "hno", Scope = "I", Type = "L", ReferenceName = "Northern Hindko"},
                                 new {Id    = "hns", Scope = "I", Type = "L", ReferenceName = "Caribbean Hindustani"},
                                 new {Id    = "hnu", Scope = "I", Type = "L", ReferenceName = "Hung"},
                                 new {Id    = "hoa", Scope = "I", Type = "L", ReferenceName = "Hoava"},
                                 new {Id    = "hob", Scope = "I", Type = "L", ReferenceName = "Mari (Madang Province)"},
                                 new {Id    = "hoc", Scope = "I", Type = "L", ReferenceName = "Ho"},
                                 new {Id    = "hod", Scope = "I", Type = "E", ReferenceName = "Holma"},
                                 new {Id    = "hoe", Scope = "I", Type = "L", ReferenceName = "Horom"},
                                 new {Id    = "hoh", Scope = "I", Type = "L", ReferenceName = "Hobyót"},
                                 new {Id    = "hoi", Scope = "I", Type = "L", ReferenceName = "Holikachuk"},
                                 new {Id    = "hoj", Scope = "I", Type = "L", ReferenceName = "Hadothi"},
                                 new {Id    = "hol", Scope = "I", Type = "L", ReferenceName = "Holu"},
                                 new {Id    = "hom", Scope = "I", Type = "E", ReferenceName = "Homa"},
                                 new {Id    = "hoo", Scope = "I", Type = "L", ReferenceName = "Holoholo"},
                                 new {Id    = "hop", Scope = "I", Type = "L", ReferenceName = "Hopi"},
                                 new {Id    = "hor", Scope = "I", Type = "E", ReferenceName = "Horo"},
                                 new
                                 {
                                     Id            = "hos",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Ho Chi Minh City Sign Language"
                                 }, new {Id = "hot", Scope = "I", Type = "L", ReferenceName = "Hote"},
                                 new {Id    = "hov", Scope = "I", Type = "L", ReferenceName = "Hovongan"},
                                 new {Id    = "how", Scope = "I", Type = "L", ReferenceName = "Honi"},
                                 new {Id    = "hoy", Scope = "I", Type = "L", ReferenceName = "Holiya"},
                                 new {Id    = "hoz", Scope = "I", Type = "L", ReferenceName = "Hozo"},
                                 new {Id    = "hpo", Scope = "I", Type = "E", ReferenceName = "Hpon"},
                                 new
                                 {
                                     Id            = "hps",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Hawai'i Sign Language (HSL)"
                                 }, new {Id = "hra", Scope = "I", Type = "L", ReferenceName = "Hrangkhol"},
                                 new {Id    = "hrc", Scope = "I", Type = "L", ReferenceName = "Niwer Mil"},
                                 new {Id    = "hre", Scope = "I", Type = "L", ReferenceName = "Hre"},
                                 new {Id    = "hrk", Scope = "I", Type = "L", ReferenceName = "Haruku"},
                                 new {Id    = "hrm", Scope = "I", Type = "L", ReferenceName = "Horned Miao"},
                                 new {Id    = "hro", Scope = "I", Type = "L", ReferenceName = "Haroi"},
                                 new {Id    = "hrp", Scope = "I", Type = "E", ReferenceName = "Nhirrpi"},
                                 new {Id    = "hrt", Scope = "I", Type = "L", ReferenceName = "Hértevin"},
                                 new {Id    = "hru", Scope = "I", Type = "L", ReferenceName = "Hruso"},
                                 new
                                 {
                                     Id            = "hrv",
                                     Part2B        = "hrv",
                                     Part2T        = "hrv",
                                     Part1         = "hr",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Croatian"
                                 }, new {Id = "hrw", Scope = "I", Type = "L", ReferenceName = "Warwar Feni"},
                                 new {Id    = "hrx", Scope = "I", Type = "L", ReferenceName = "Hunsrik"},
                                 new {Id    = "hrz", Scope = "I", Type = "L", ReferenceName = "Harzani"},
                                 new
                                 {
                                     Id            = "hsb",
                                     Part2B        = "hsb",
                                     Part2T        = "hsb",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Upper Sorbian"
                                 },
                                 new {Id = "hsh", Scope = "I", Type = "L", ReferenceName = "Hungarian Sign Language"},
                                 new {Id = "hsl", Scope = "I", Type = "L", ReferenceName = "Hausa Sign Language"},
                                 new {Id = "hsn", Scope = "I", Type = "L", ReferenceName = "Xiang Chinese"},
                                 new {Id = "hss", Scope = "I", Type = "L", ReferenceName = "Harsusi"},
                                 new {Id = "hti", Scope = "I", Type = "E", ReferenceName = "Hoti"},
                                 new {Id = "hto", Scope = "I", Type = "L", ReferenceName = "Minica Huitoto"},
                                 new {Id = "hts", Scope = "I", Type = "L", ReferenceName = "Hadza"},
                                 new {Id = "htu", Scope = "I", Type = "L", ReferenceName = "Hitu"},
                                 new {Id = "htx", Scope = "I", Type = "A", ReferenceName = "Middle Hittite"},
                                 new {Id = "hub", Scope = "I", Type = "L", ReferenceName = "Huambisa"},
                                 new {Id = "huc", Scope = "I", Type = "L", ReferenceName = "ǂHua"},
                                 new {Id = "hud", Scope = "I", Type = "L", ReferenceName = "Huaulu"},
                                 new
                                 {
                                     Id            = "hue",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "San Francisco Del Mar Huave"
                                 }, new {Id = "huf", Scope = "I", Type = "L", ReferenceName = "Humene"},
                                 new {Id    = "hug", Scope = "I", Type = "L", ReferenceName = "Huachipaeri"},
                                 new {Id    = "huh", Scope = "I", Type = "L", ReferenceName = "Huilliche"},
                                 new {Id    = "hui", Scope = "I", Type = "L", ReferenceName = "Huli"},
                                 new
                                     {
                                         Id            = "huj",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Northern Guiyang Hmong"
                                     },
                                 new {Id = "huk", Scope = "I", Type = "E", ReferenceName = "Hulung"},
                                 new {Id = "hul", Scope = "I", Type = "L", ReferenceName = "Hula"},
                                 new {Id = "hum", Scope = "I", Type = "L", ReferenceName = "Hungana"},
                                 new
                                 {
                                     Id            = "hun",
                                     Part2B        = "hun",
                                     Part2T        = "hun",
                                     Part1         = "hu",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Hungarian"
                                 }, new {Id = "huo", Scope = "I", Type = "L", ReferenceName = "Hu"},
                                 new
                                 {
                                     Id            = "hup",
                                     Part2B        = "hup",
                                     Part2T        = "hup",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Hupa"
                                 }, new {Id = "huq", Scope = "I", Type = "L", ReferenceName = "Tsat"},
                                 new {Id    = "hur", Scope = "I", Type = "L", ReferenceName = "Halkomelem"},
                                 new {Id    = "hus", Scope = "I", Type = "L", ReferenceName = "Huastec"},
                                 new {Id    = "hut", Scope = "I", Type = "L", ReferenceName = "Humla"},
                                 new {Id    = "huu", Scope = "I", Type = "L", ReferenceName = "Murui Huitoto"},
                                 new
                                     {
                                         Id            = "huv",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "San Mateo Del Mar Huave"
                                     },
                                 new {Id = "huw", Scope = "I", Type = "E", ReferenceName = "Hukumina"},
                                 new {Id = "hux", Scope = "I", Type = "L", ReferenceName = "Nüpode Huitoto"},
                                 new {Id = "huy", Scope = "I", Type = "L", ReferenceName = "Hulaulá"},
                                 new {Id = "huz", Scope = "I", Type = "L", ReferenceName = "Hunzib"},
                                 new
                                 {
                                     Id            = "hvc",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Haitian Vodoun Culture Language"
                                 },
                                 new
                                 {
                                     Id            = "hve",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "San Dionisio Del Mar Huave"
                                 }, new {Id = "hvk", Scope = "I", Type = "L", ReferenceName = "Haveke"},
                                 new {Id    = "hvn", Scope = "I", Type = "L", ReferenceName = "Sabu"},
                                 new
                                     {
                                         Id            = "hvv",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Santa María Del Mar Huave"
                                     },
                                 new {Id = "hwa", Scope = "I", Type = "L", ReferenceName = "Wané"},
                                 new
                                     {
                                         Id            = "hwc",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Hawai'i Creole English"
                                     },
                                 new {Id = "hwo", Scope = "I", Type = "L", ReferenceName = "Hwana"},
                                 new {Id = "hya", Scope = "I", Type = "L", ReferenceName = "Hya"},
                                 new
                                 {
                                     Id            = "hye",
                                     Part2B        = "arm",
                                     Part2T        = "hye",
                                     Part1         = "hy",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Armenian"
                                 }, new {Id = "hyw", Scope = "I", Type = "L", ReferenceName = "Western Armenian"});
        }
    }
}