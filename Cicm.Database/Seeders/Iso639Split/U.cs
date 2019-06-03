using Microsoft.EntityFrameworkCore;

namespace Cicm.Database.Seeders.Iso639Split
{
    public static class U
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Iso639>()
                        .HasData(new {Id = "uam", Scope = "I", Type = "E", ReferenceName = "Uamué"},
                                 new {Id = "uan", Scope = "I", Type = "L", ReferenceName = "Kuan"},
                                 new {Id = "uar", Scope = "I", Type = "L", ReferenceName = "Tairuma"},
                                 new {Id = "uba", Scope = "I", Type = "L", ReferenceName = "Ubang"},
                                 new {Id = "ubi", Scope = "I", Type = "L", ReferenceName = "Ubi"},
                                 new {Id = "ubl", Scope = "I", Type = "L", ReferenceName = "Buhi'non Bikol"},
                                 new {Id = "ubr", Scope = "I", Type = "L", ReferenceName = "Ubir"},
                                 new {Id = "ubu", Scope = "I", Type = "L", ReferenceName = "Umbu-Ungu"},
                                 new {Id = "uby", Scope = "I", Type = "E", ReferenceName = "Ubykh"},
                                 new {Id = "uda", Scope = "I", Type = "L", ReferenceName = "Uda"},
                                 new {Id = "ude", Scope = "I", Type = "L", ReferenceName = "Udihe"},
                                 new {Id = "udg", Scope = "I", Type = "L", ReferenceName = "Muduga"},
                                 new {Id = "udi", Scope = "I", Type = "L", ReferenceName = "Udi"},
                                 new {Id = "udj", Scope = "I", Type = "L", ReferenceName = "Ujir"},
                                 new {Id = "udl", Scope = "I", Type = "L", ReferenceName = "Wuzlam"},
                                 new
                                 {
                                     Id            = "udm",
                                     Part2B        = "udm",
                                     Part2T        = "udm",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Udmurt"
                                 }, new {Id = "udu", Scope = "I", Type = "L", ReferenceName = "Uduk"},
                                 new {Id    = "ues", Scope = "I", Type = "L", ReferenceName = "Kioko"},
                                 new {Id    = "ufi", Scope = "I", Type = "L", ReferenceName = "Ufim"},
                                 new
                                 {
                                     Id            = "uga",
                                     Part2B        = "uga",
                                     Part2T        = "uga",
                                     Scope         = "I",
                                     Type          = "A",
                                     ReferenceName = "Ugaritic"
                                 }, new {Id = "ugb", Scope = "I", Type = "E", ReferenceName = "Kuku-Ugbanh"},
                                 new {Id    = "uge", Scope = "I", Type = "L", ReferenceName = "Ughele"},
                                 new {Id    = "ugn", Scope = "I", Type = "L", ReferenceName = "Ugandan Sign Language"},
                                 new {Id    = "ugo", Scope = "I", Type = "L", ReferenceName = "Ugong"},
                                 new
                                     {
                                         Id            = "ugy",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Uruguayan Sign Language"
                                     },
                                 new {Id = "uha", Scope = "I", Type = "L", ReferenceName = "Uhami"},
                                 new {Id = "uhn", Scope = "I", Type = "L", ReferenceName = "Damal"},
                                 new
                                 {
                                     Id            = "uig",
                                     Part2B        = "uig",
                                     Part2T        = "uig",
                                     Part1         = "ug",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Uighur"
                                 }, new {Id = "uis", Scope = "I", Type = "L", ReferenceName = "Uisai"},
                                 new {Id    = "uiv", Scope = "I", Type = "L", ReferenceName = "Iyive"},
                                 new {Id    = "uji", Scope = "I", Type = "L", ReferenceName = "Tanjijili"},
                                 new {Id    = "uka", Scope = "I", Type = "L", ReferenceName = "Kaburi"},
                                 new {Id    = "ukg", Scope = "I", Type = "L", ReferenceName = "Ukuriguma"},
                                 new {Id    = "ukh", Scope = "I", Type = "L", ReferenceName = "Ukhwejo"},
                                 new {Id    = "ukk", Scope = "I", Type = "L", ReferenceName = "Muak Sa-aak"},
                                 new
                                     {
                                         Id            = "ukl",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Ukrainian Sign Language"
                                     },
                                 new {Id = "ukp", Scope = "I", Type = "L", ReferenceName = "Ukpe-Bayobiri"},
                                 new {Id = "ukq", Scope = "I", Type = "L", ReferenceName = "Ukwa"},
                                 new
                                 {
                                     Id            = "ukr",
                                     Part2B        = "ukr",
                                     Part2T        = "ukr",
                                     Part1         = "uk",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Ukrainian"
                                 },
                                 new
                                 {
                                     Id            = "uks",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Urubú-Kaapor Sign Language"
                                 }, new {Id = "uku", Scope = "I", Type = "L", ReferenceName = "Ukue"},
                                 new {Id    = "ukw", Scope = "I", Type = "L", ReferenceName = "Ukwuani-Aboh-Ndoni"},
                                 new {Id    = "uky", Scope = "I", Type = "E", ReferenceName = "Kuuk-Yak"},
                                 new {Id    = "ula", Scope = "I", Type = "L", ReferenceName = "Fungwa"},
                                 new {Id    = "ulb", Scope = "I", Type = "L", ReferenceName = "Ulukwumi"},
                                 new {Id    = "ulc", Scope = "I", Type = "L", ReferenceName = "Ulch"},
                                 new {Id    = "ule", Scope = "I", Type = "E", ReferenceName = "Lule"},
                                 new {Id    = "ulf", Scope = "I", Type = "L", ReferenceName = "Usku"},
                                 new {Id    = "uli", Scope = "I", Type = "L", ReferenceName = "Ulithian"},
                                 new {Id    = "ulk", Scope = "I", Type = "L", ReferenceName = "Meriam Mir"},
                                 new {Id    = "ull", Scope = "I", Type = "L", ReferenceName = "Ullatan"},
                                 new {Id    = "ulm", Scope = "I", Type = "L", ReferenceName = "Ulumanda'"},
                                 new {Id    = "uln", Scope = "I", Type = "L", ReferenceName = "Unserdeutsch"},
                                 new {Id    = "ulu", Scope = "I", Type = "L", ReferenceName = "Uma' Lung"},
                                 new {Id    = "ulw", Scope = "I", Type = "L", ReferenceName = "Ulwa"},
                                 new {Id    = "uma", Scope = "I", Type = "L", ReferenceName = "Umatilla"},
                                 new
                                 {
                                     Id            = "umb",
                                     Part2B        = "umb",
                                     Part2T        = "umb",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Umbundu"
                                 }, new {Id = "umc", Scope = "I", Type = "A", ReferenceName = "Marrucinian"},
                                 new {Id    = "umd", Scope = "I", Type = "E", ReferenceName = "Umbindhamu"},
                                 new {Id    = "umg", Scope = "I", Type = "E", ReferenceName = "Morrobalama"},
                                 new {Id    = "umi", Scope = "I", Type = "L", ReferenceName = "Ukit"},
                                 new {Id    = "umm", Scope = "I", Type = "L", ReferenceName = "Umon"},
                                 new {Id    = "umn", Scope = "I", Type = "L", ReferenceName = "Makyan Naga"},
                                 new {Id    = "umo", Scope = "I", Type = "E", ReferenceName = "Umotína"},
                                 new {Id    = "ump", Scope = "I", Type = "L", ReferenceName = "Umpila"},
                                 new {Id    = "umr", Scope = "I", Type = "E", ReferenceName = "Umbugarla"},
                                 new {Id    = "ums", Scope = "I", Type = "L", ReferenceName = "Pendau"},
                                 new {Id    = "umu", Scope = "I", Type = "L", ReferenceName = "Munsee"},
                                 new {Id    = "una", Scope = "I", Type = "L", ReferenceName = "North Watut"},
                                 new
                                 {
                                     Id            = "und",
                                     Part2B        = "und",
                                     Part2T        = "und",
                                     Scope         = "S",
                                     Type          = "S",
                                     ReferenceName = "Undetermined"
                                 }, new {Id = "une", Scope = "I", Type = "L", ReferenceName = "Uneme"},
                                 new {Id    = "ung", Scope = "I", Type = "L", ReferenceName = "Ngarinyin"},
                                 new {Id    = "unk", Scope = "I", Type = "L", ReferenceName = "Enawené-Nawé"},
                                 new {Id    = "unm", Scope = "I", Type = "E", ReferenceName = "Unami"},
                                 new {Id    = "unn", Scope = "I", Type = "L", ReferenceName = "Kurnai"},
                                 new {Id    = "unr", Scope = "I", Type = "L", ReferenceName = "Mundari"},
                                 new {Id    = "unu", Scope = "I", Type = "L", ReferenceName = "Unubahe"},
                                 new {Id    = "unx", Scope = "I", Type = "L", ReferenceName = "Munda"},
                                 new {Id    = "unz", Scope = "I", Type = "L", ReferenceName = "Unde Kaili"},
                                 new {Id    = "upi", Scope = "I", Type = "L", ReferenceName = "Umeda"},
                                 new
                                     {
                                         Id            = "upv",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Uripiv-Wala-Rano-Atchin"
                                     },
                                 new {Id = "ura", Scope = "I", Type = "L", ReferenceName = "Urarina"},
                                 new {Id = "urb", Scope = "I", Type = "L", ReferenceName = "Urubú-Kaapor"},
                                 new {Id = "urc", Scope = "I", Type = "E", ReferenceName = "Urningangg"},
                                 new
                                 {
                                     Id            = "urd",
                                     Part2B        = "urd",
                                     Part2T        = "urd",
                                     Part1         = "ur",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Urdu"
                                 }, new {Id = "ure", Scope = "I", Type = "L", ReferenceName = "Uru"},
                                 new {Id    = "urf", Scope = "I", Type = "E", ReferenceName = "Uradhi"},
                                 new {Id    = "urg", Scope = "I", Type = "L", ReferenceName = "Urigina"},
                                 new {Id    = "urh", Scope = "I", Type = "L", ReferenceName = "Urhobo"},
                                 new {Id    = "uri", Scope = "I", Type = "L", ReferenceName = "Urim"},
                                 new {Id    = "urk", Scope = "I", Type = "L", ReferenceName = "Urak Lawoi'"},
                                 new {Id    = "url", Scope = "I", Type = "L", ReferenceName = "Urali"},
                                 new {Id    = "urm", Scope = "I", Type = "L", ReferenceName = "Urapmin"},
                                 new {Id    = "urn", Scope = "I", Type = "L", ReferenceName = "Uruangnirin"},
                                 new {Id    = "uro", Scope = "I", Type = "L", ReferenceName = "Ura (Papua New Guinea)"},
                                 new {Id    = "urp", Scope = "I", Type = "L", ReferenceName = "Uru-Pa-In"},
                                 new {Id    = "urr", Scope = "I", Type = "L", ReferenceName = "Lehalurup"},
                                 new {Id    = "urt", Scope = "I", Type = "L", ReferenceName = "Urat"},
                                 new {Id    = "uru", Scope = "I", Type = "E", ReferenceName = "Urumi"},
                                 new {Id    = "urv", Scope = "I", Type = "E", ReferenceName = "Uruava"},
                                 new {Id    = "urw", Scope = "I", Type = "L", ReferenceName = "Sop"},
                                 new {Id    = "urx", Scope = "I", Type = "L", ReferenceName = "Urimo"},
                                 new {Id    = "ury", Scope = "I", Type = "L", ReferenceName = "Orya"},
                                 new {Id    = "urz", Scope = "I", Type = "L", ReferenceName = "Uru-Eu-Wau-Wau"},
                                 new {Id    = "usa", Scope = "I", Type = "L", ReferenceName = "Usarufa"},
                                 new {Id    = "ush", Scope = "I", Type = "L", ReferenceName = "Ushojo"},
                                 new {Id    = "usi", Scope = "I", Type = "L", ReferenceName = "Usui"},
                                 new {Id    = "usk", Scope = "I", Type = "L", ReferenceName = "Usaghade"},
                                 new {Id    = "usp", Scope = "I", Type = "L", ReferenceName = "Uspanteco"},
                                 new {Id    = "uss", Scope = "I", Type = "L", ReferenceName = "us-Saare"},
                                 new {Id    = "usu", Scope = "I", Type = "L", ReferenceName = "Uya"},
                                 new {Id    = "uta", Scope = "I", Type = "L", ReferenceName = "Otank"},
                                 new {Id    = "ute", Scope = "I", Type = "L", ReferenceName = "Ute-Southern Paiute"},
                                 new {Id    = "uth", Scope = "I", Type = "L", ReferenceName = "ut-Hun"},
                                 new {Id    = "utp", Scope = "I", Type = "L", ReferenceName = "Amba (Solomon Islands)"},
                                 new {Id    = "utr", Scope = "I", Type = "L", ReferenceName = "Etulo"},
                                 new {Id    = "utu", Scope = "I", Type = "L", ReferenceName = "Utu"},
                                 new {Id    = "uum", Scope = "I", Type = "L", ReferenceName = "Urum"},
                                 new {Id    = "uun", Scope = "I", Type = "L", ReferenceName = "Kulon-Pazeh"},
                                 new {Id    = "uur", Scope = "I", Type = "L", ReferenceName = "Ura (Vanuatu)"},
                                 new {Id    = "uuu", Scope = "I", Type = "L", ReferenceName = "U"},
                                 new {Id    = "uve", Scope = "I", Type = "L", ReferenceName = "West Uvean"},
                                 new {Id    = "uvh", Scope = "I", Type = "L", ReferenceName = "Uri"},
                                 new {Id    = "uvl", Scope = "I", Type = "L", ReferenceName = "Lote"},
                                 new {Id    = "uwa", Scope = "I", Type = "L", ReferenceName = "Kuku-Uwanh"},
                                 new {Id    = "uya", Scope = "I", Type = "L", ReferenceName = "Doko-Uyanga"},
                                 new
                                 {
                                     Id            = "uzb",
                                     Part2B        = "uzb",
                                     Part2T        = "uzb",
                                     Part1         = "uz",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Uzbek"
                                 }, new {Id = "uzn", Scope = "I", Type = "L", ReferenceName = "Northern Uzbek"},
                                 new {Id    = "uzs", Scope = "I", Type = "L", ReferenceName = "Southern Uzbek"});
        }
    }
}