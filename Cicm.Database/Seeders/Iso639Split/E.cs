using Microsoft.EntityFrameworkCore;

namespace Cicm.Database.Seeders.Iso639Split
{
    public static class E
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Iso639>()
                        .HasData(new {Id = "eaa", Scope = "I", Type = "E", ReferenceName = "Karenggapa"},
                                 new {Id = "ebg", Scope = "I", Type = "L", ReferenceName = "Ebughu"},
                                 new {Id = "ebk", Scope = "I", Type = "L", ReferenceName = "Eastern Bontok"},
                                 new {Id = "ebo", Scope = "I", Type = "L", ReferenceName = "Teke-Ebo"},
                                 new {Id = "ebr", Scope = "I", Type = "L", ReferenceName = "Ebrié"},
                                 new {Id = "ebu", Scope = "I", Type = "L", ReferenceName = "Embu"},
                                 new {Id = "ecr", Scope = "I", Type = "A", ReferenceName = "Eteocretan"},
                                 new {Id = "ecs", Scope = "I", Type = "L", ReferenceName = "Ecuadorian Sign Language"},
                                 new {Id = "ecy", Scope = "I", Type = "A", ReferenceName = "Eteocypriot"},
                                 new {Id = "eee", Scope = "I", Type = "L", ReferenceName = "E"},
                                 new {Id = "efa", Scope = "I", Type = "L", ReferenceName = "Efai"},
                                 new {Id = "efe", Scope = "I", Type = "L", ReferenceName = "Efe"},
                                 new
                                 {
                                     Id            = "efi",
                                     Part2B        = "efi",
                                     Part2T        = "efi",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Efik"
                                 }, new {Id = "ega", Scope = "I", Type = "L", ReferenceName = "Ega"},
                                 new {Id    = "egl", Scope = "I", Type = "L", ReferenceName = "Emilian"},
                                 new {Id    = "ego", Scope = "I", Type = "L", ReferenceName = "Eggon"},
                                 new
                                 {
                                     Id            = "egy",
                                     Part2B        = "egy",
                                     Part2T        = "egy",
                                     Scope         = "I",
                                     Type          = "A",
                                     ReferenceName = "Egyptian (Ancient)"
                                 }, new {Id = "ehu", Scope = "I", Type = "L", ReferenceName = "Ehueun"},
                                 new {Id    = "eip", Scope = "I", Type = "L", ReferenceName = "Eipomek"},
                                 new {Id    = "eit", Scope = "I", Type = "L", ReferenceName = "Eitiep"},
                                 new {Id    = "eiv", Scope = "I", Type = "L", ReferenceName = "Askopan"},
                                 new {Id    = "eja", Scope = "I", Type = "L", ReferenceName = "Ejamat"},
                                 new
                                 {
                                     Id            = "eka",
                                     Part2B        = "eka",
                                     Part2T        = "eka",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Ekajuk"
                                 }, new {Id = "ekc", Scope = "I", Type = "E", ReferenceName = "Eastern Karnic"},
                                 new {Id    = "eke", Scope = "I", Type = "L", ReferenceName = "Ekit"},
                                 new {Id    = "ekg", Scope = "I", Type = "L", ReferenceName = "Ekari"},
                                 new {Id    = "eki", Scope = "I", Type = "L", ReferenceName = "Eki"},
                                 new {Id    = "ekk", Scope = "I", Type = "L", ReferenceName = "Standard Estonian"},
                                 new {Id    = "ekl", Scope = "I", Type = "L", ReferenceName = "Kol (Bangladesh)"},
                                 new {Id    = "ekm", Scope = "I", Type = "L", ReferenceName = "Elip"},
                                 new {Id    = "eko", Scope = "I", Type = "L", ReferenceName = "Koti"},
                                 new {Id    = "ekp", Scope = "I", Type = "L", ReferenceName = "Ekpeye"},
                                 new {Id    = "ekr", Scope = "I", Type = "L", ReferenceName = "Yace"},
                                 new {Id    = "eky", Scope = "I", Type = "L", ReferenceName = "Eastern Kayah"},
                                 new {Id    = "ele", Scope = "I", Type = "L", ReferenceName = "Elepi"},
                                 new {Id    = "elh", Scope = "I", Type = "L", ReferenceName = "El Hugeirat"},
                                 new {Id    = "eli", Scope = "I", Type = "E", ReferenceName = "Nding"},
                                 new {Id    = "elk", Scope = "I", Type = "L", ReferenceName = "Elkei"},
                                 new
                                 {
                                     Id            = "ell",
                                     Part2B        = "gre",
                                     Part2T        = "ell",
                                     Part1         = "el",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Modern Greek (1453-)"
                                 }, new {Id = "elm", Scope = "I", Type = "L", ReferenceName = "Eleme"},
                                 new {Id    = "elo", Scope = "I", Type = "L", ReferenceName = "El Molo"},
                                 new {Id    = "elu", Scope = "I", Type = "L", ReferenceName = "Elu"},
                                 new
                                 {
                                     Id            = "elx",
                                     Part2B        = "elx",
                                     Part2T        = "elx",
                                     Scope         = "I",
                                     Type          = "A",
                                     ReferenceName = "Elamite"
                                 }, new {Id = "ema", Scope = "I", Type = "L", ReferenceName = "Emai-Iuleha-Ora"},
                                 new {Id    = "emb", Scope = "I", Type = "L", ReferenceName = "Embaloh"},
                                 new {Id    = "eme", Scope = "I", Type = "L", ReferenceName = "Emerillon"},
                                 new {Id    = "emg", Scope = "I", Type = "L", ReferenceName = "Eastern Meohang"},
                                 new {Id    = "emi", Scope = "I", Type = "L", ReferenceName = "Mussau-Emira"},
                                 new {Id    = "emk", Scope = "I", Type = "L", ReferenceName = "Eastern Maninkakan"},
                                 new {Id    = "emm", Scope = "I", Type = "E", ReferenceName = "Mamulique"},
                                 new {Id    = "emn", Scope = "I", Type = "L", ReferenceName = "Eman"},
                                 new {Id    = "emp", Scope = "I", Type = "L", ReferenceName = "Northern Emberá"},
                                 new {Id    = "ems", Scope = "I", Type = "L", ReferenceName = "Pacific Gulf Yupik"},
                                 new {Id    = "emu", Scope = "I", Type = "L", ReferenceName = "Eastern Muria"},
                                 new {Id    = "emw", Scope = "I", Type = "L", ReferenceName = "Emplawas"},
                                 new {Id    = "emx", Scope = "I", Type = "L", ReferenceName = "Erromintxela"},
                                 new {Id    = "emy", Scope = "I", Type = "A", ReferenceName = "Epigraphic Mayan"},
                                 new {Id    = "ena", Scope = "I", Type = "L", ReferenceName = "Apali"},
                                 new {Id    = "enb", Scope = "I", Type = "L", ReferenceName = "Markweeta"},
                                 new {Id    = "enc", Scope = "I", Type = "L", ReferenceName = "En"},
                                 new {Id    = "end", Scope = "I", Type = "L", ReferenceName = "Ende"},
                                 new {Id    = "enf", Scope = "I", Type = "L", ReferenceName = "Forest Enets"},
                                 new
                                 {
                                     Id            = "eng",
                                     Part2B        = "eng",
                                     Part2T        = "eng",
                                     Part1         = "en",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "English"
                                 }, new {Id = "enh", Scope = "I", Type = "L", ReferenceName = "Tundra Enets"},
                                 new {Id    = "enl", Scope = "I", Type = "L", ReferenceName = "Enlhet"},
                                 new
                                 {
                                     Id            = "enm",
                                     Part2B        = "enm",
                                     Part2T        = "enm",
                                     Scope         = "I",
                                     Type          = "H",
                                     ReferenceName = "Middle English (1100-1500)"
                                 }, new {Id = "enn", Scope = "I", Type = "L", ReferenceName = "Engenni"},
                                 new {Id    = "eno", Scope = "I", Type = "L", ReferenceName = "Enggano"},
                                 new {Id    = "enq", Scope = "I", Type = "L", ReferenceName = "Enga"},
                                 new {Id    = "enr", Scope = "I", Type = "L", ReferenceName = "Emumu"},
                                 new {Id    = "enu", Scope = "I", Type = "L", ReferenceName = "Enu"},
                                 new {Id    = "env", Scope = "I", Type = "L", ReferenceName = "Enwan (Edu State)"},
                                 new
                                     {
                                         Id            = "enw",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Enwan (Akwa Ibom State)"
                                     },
                                 new {Id = "enx", Scope = "I", Type = "L", ReferenceName = "Enxet"},
                                 new {Id = "eot", Scope = "I", Type = "L", ReferenceName = "Beti (Côte d'Ivoire)"},
                                 new {Id = "epi", Scope = "I", Type = "L", ReferenceName = "Epie"},
                                 new
                                 {
                                     Id            = "epo",
                                     Part2B        = "epo",
                                     Part2T        = "epo",
                                     Part1         = "eo",
                                     Scope         = "I",
                                     Type          = "C",
                                     ReferenceName = "Esperanto"
                                 }, new {Id = "era", Scope = "I", Type = "L", ReferenceName = "Eravallan"},
                                 new {Id    = "erg", Scope = "I", Type = "L", ReferenceName = "Sie"},
                                 new {Id    = "erh", Scope = "I", Type = "L", ReferenceName = "Eruwa"},
                                 new {Id    = "eri", Scope = "I", Type = "L", ReferenceName = "Ogea"},
                                 new {Id    = "erk", Scope = "I", Type = "L", ReferenceName = "South Efate"},
                                 new {Id    = "ero", Scope = "I", Type = "L", ReferenceName = "Horpa"},
                                 new {Id    = "err", Scope = "I", Type = "E", ReferenceName = "Erre"},
                                 new {Id    = "ers", Scope = "I", Type = "L", ReferenceName = "Ersu"},
                                 new {Id    = "ert", Scope = "I", Type = "L", ReferenceName = "Eritai"},
                                 new {Id    = "erw", Scope = "I", Type = "L", ReferenceName = "Erokwanas"},
                                 new {Id    = "ese", Scope = "I", Type = "L", ReferenceName = "Ese Ejja"},
                                 new {Id    = "esg", Scope = "I", Type = "L", ReferenceName = "Aheri Gondi"},
                                 new {Id    = "esh", Scope = "I", Type = "L", ReferenceName = "Eshtehardi"},
                                 new
                                     {
                                         Id            = "esi",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "North Alaskan Inupiatun"
                                     },
                                 new
                                 {
                                     Id            = "esk",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Northwest Alaska Inupiatun"
                                 },
                                 new {Id = "esl", Scope = "I", Type = "L", ReferenceName = "Egypt Sign Language"},
                                 new {Id = "esm", Scope = "I", Type = "E", ReferenceName = "Esuma"},
                                 new {Id = "esn", Scope = "I", Type = "L", ReferenceName = "Salvadoran Sign Language"},
                                 new {Id = "eso", Scope = "I", Type = "L", ReferenceName = "Estonian Sign Language"},
                                 new {Id = "esq", Scope = "I", Type = "E", ReferenceName = "Esselen"},
                                 new {Id = "ess", Scope = "I", Type = "L", ReferenceName = "Central Siberian Yupik"},
                                 new
                                 {
                                     Id            = "est",
                                     Part2B        = "est",
                                     Part2T        = "est",
                                     Part1         = "et",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Estonian"
                                 }, new {Id = "esu", Scope = "I", Type = "L", ReferenceName = "Central Yupik"},
                                 new {Id    = "esy", Scope = "I", Type = "L", ReferenceName = "Eskayan"},
                                 new {Id    = "etb", Scope = "I", Type = "L", ReferenceName = "Etebi"},
                                 new {Id    = "etc", Scope = "I", Type = "E", ReferenceName = "Etchemin"},
                                 new
                                     {
                                         Id            = "eth",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Ethiopian Sign Language"
                                     },
                                 new {Id = "etn", Scope = "I", Type = "L", ReferenceName = "Eton (Vanuatu)"},
                                 new {Id = "eto", Scope = "I", Type = "L", ReferenceName = "Eton (Cameroon)"},
                                 new {Id = "etr", Scope = "I", Type = "L", ReferenceName = "Edolo"},
                                 new {Id = "ets", Scope = "I", Type = "L", ReferenceName = "Yekhee"},
                                 new {Id = "ett", Scope = "I", Type = "A", ReferenceName = "Etruscan"},
                                 new {Id = "etu", Scope = "I", Type = "L", ReferenceName = "Ejagham"},
                                 new {Id = "etx", Scope = "I", Type = "L", ReferenceName = "Eten"},
                                 new {Id = "etz", Scope = "I", Type = "L", ReferenceName = "Semimi"},
                                 new
                                 {
                                     Id            = "eus",
                                     Part2B        = "baq",
                                     Part2T        = "eus",
                                     Part1         = "eu",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Basque"
                                 }, new {Id = "eve", Scope = "I", Type = "L", ReferenceName = "Even"},
                                 new {Id    = "evh", Scope = "I", Type = "L", ReferenceName = "Uvbie"},
                                 new {Id    = "evn", Scope = "I", Type = "L", ReferenceName = "Evenki"},
                                 new
                                 {
                                     Id            = "ewe",
                                     Part2B        = "ewe",
                                     Part2T        = "ewe",
                                     Part1         = "ee",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Ewe"
                                 }, new
                                 {
                                     Id            = "ewo",
                                     Part2B        = "ewo",
                                     Part2T        = "ewo",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Ewondo"
                                 }, new {Id = "ext", Scope = "I", Type = "L", ReferenceName = "Extremaduran"},
                                 new {Id    = "eya", Scope = "I", Type = "E", ReferenceName = "Eyak"},
                                 new {Id    = "eyo", Scope = "I", Type = "L", ReferenceName = "Keiyo"},
                                 new {Id    = "eza", Scope = "I", Type = "L", ReferenceName = "Ezaa"},
                                 new {Id    = "eze", Scope = "I", Type = "L", ReferenceName = "Uzekwe"});
        }
    }
}