using Microsoft.EntityFrameworkCore;

namespace Cicm.Database.Seeders.Iso639Split
{
    public static class Z
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Iso639>()
                        .HasData(new {Id = "zaa", Scope = "I", Type = "L", ReferenceName = "Sierra de Juárez Zapotec"},
                                 new
                                 {
                                     Id            = "zab",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Western Tlacolula Valley Zapotec"
                                 }, new {Id = "zac", Scope = "I", Type = "L", ReferenceName = "Ocotlán Zapotec"},
                                 new {Id    = "zad", Scope = "I", Type = "L", ReferenceName = "Cajonos Zapotec"},
                                 new {Id    = "zae", Scope = "I", Type = "L", ReferenceName = "Yareni Zapotec"},
                                 new {Id    = "zaf", Scope = "I", Type = "L", ReferenceName = "Ayoquesco Zapotec"},
                                 new {Id    = "zag", Scope = "I", Type = "L", ReferenceName = "Zaghawa"},
                                 new {Id    = "zah", Scope = "I", Type = "L", ReferenceName = "Zangwal"},
                                 new {Id    = "zai", Scope = "I", Type = "L", ReferenceName = "Isthmus Zapotec"},
                                 new {Id    = "zaj", Scope = "I", Type = "L", ReferenceName = "Zaramo"},
                                 new {Id    = "zak", Scope = "I", Type = "L", ReferenceName = "Zanaki"},
                                 new {Id    = "zal", Scope = "I", Type = "L", ReferenceName = "Zauzou"},
                                 new {Id    = "zam", Scope = "I", Type = "L", ReferenceName = "Miahuatlán Zapotec"},
                                 new {Id    = "zao", Scope = "I", Type = "L", ReferenceName = "Ozolotepec Zapotec"},
                                 new
                                 {
                                     Id            = "zap",
                                     Part2B        = "zap",
                                     Part2T        = "zap",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Zapotec"
                                 }, new {Id = "zaq", Scope = "I", Type = "L", ReferenceName = "Aloápam Zapotec"},
                                 new {Id    = "zar", Scope = "I", Type = "L", ReferenceName = "Rincón Zapotec"},
                                 new
                                 {
                                     Id            = "zas",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Santo Domingo Albarradas Zapotec"
                                 }, new {Id = "zat", Scope = "I", Type = "L", ReferenceName = "Tabaa Zapotec"},
                                 new {Id    = "zau", Scope = "I", Type = "L", ReferenceName = "Zangskari"},
                                 new {Id    = "zav", Scope = "I", Type = "L", ReferenceName = "Yatzachi Zapotec"},
                                 new {Id    = "zaw", Scope = "I", Type = "L", ReferenceName = "Mitla Zapotec"},
                                 new {Id    = "zax", Scope = "I", Type = "L", ReferenceName = "Xadani Zapotec"},
                                 new {Id    = "zay", Scope = "I", Type = "L", ReferenceName = "Zayse-Zergulla"},
                                 new {Id    = "zaz", Scope = "I", Type = "L", ReferenceName = "Zari"},
                                 new {Id    = "zbc", Scope = "I", Type = "L", ReferenceName = "Central Berawan"},
                                 new {Id    = "zbe", Scope = "I", Type = "L", ReferenceName = "East Berawan"},
                                 new
                                 {
                                     Id            = "zbl",
                                     Part2B        = "zbl",
                                     Part2T        = "zbl",
                                     Scope         = "I",
                                     Type          = "C",
                                     ReferenceName = "Blissymbols"
                                 }, new {Id = "zbt", Scope = "I", Type = "L", ReferenceName = "Batui"},
                                 new {Id    = "zbw", Scope = "I", Type = "L", ReferenceName = "West Berawan"},
                                 new {Id    = "zca", Scope = "I", Type = "L", ReferenceName = "Coatecas Altas Zapotec"},
                                 new
                                     {
                                         Id            = "zch",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Central Hongshuihe Zhuang"
                                     },
                                 new {Id = "zdj", Scope = "I", Type = "L", ReferenceName = "Ngazidja Comorian"},
                                 new {Id = "zea", Scope = "I", Type = "L", ReferenceName = "Zeeuws"},
                                 new {Id = "zeg", Scope = "I", Type = "L", ReferenceName = "Zenag"},
                                 new
                                     {
                                         Id            = "zeh",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Eastern Hongshuihe Zhuang"
                                     },
                                 new
                                 {
                                     Id            = "zen",
                                     Part2B        = "zen",
                                     Part2T        = "zen",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Zenaga"
                                 }, new {Id = "zga", Scope = "I", Type = "L", ReferenceName = "Kinga"},
                                 new {Id    = "zgb", Scope = "I", Type = "L", ReferenceName = "Guibei Zhuang"},
                                 new
                                 {
                                     Id            = "zgh",
                                     Part2B        = "zgh",
                                     Part2T        = "zgh",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Standard Moroccan Tamazight"
                                 }, new {Id = "zgm", Scope = "I", Type = "L", ReferenceName = "Minz Zhuang"},
                                 new {Id    = "zgn", Scope = "I", Type = "L", ReferenceName = "Guibian Zhuang"},
                                 new {Id    = "zgr", Scope = "I", Type = "L", ReferenceName = "Magori"},
                                 new
                                 {
                                     Id            = "zha",
                                     Part2B        = "zha",
                                     Part2T        = "zha",
                                     Part1         = "za",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Zhuang"
                                 }, new {Id = "zhb", Scope = "I", Type = "L", ReferenceName = "Zhaba"},
                                 new {Id    = "zhd", Scope = "I", Type = "L", ReferenceName = "Dai Zhuang"},
                                 new {Id    = "zhi", Scope = "I", Type = "L", ReferenceName = "Zhire"},
                                 new {Id    = "zhn", Scope = "I", Type = "L", ReferenceName = "Nong Zhuang"},
                                 new
                                 {
                                     Id            = "zho",
                                     Part2B        = "chi",
                                     Part2T        = "zho",
                                     Part1         = "zh",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Chinese"
                                 }, new {Id = "zhw", Scope = "I", Type = "L", ReferenceName = "Zhoa"},
                                 new {Id    = "zia", Scope = "I", Type = "L", ReferenceName = "Zia"},
                                 new {Id    = "zib", Scope = "I", Type = "L", ReferenceName = "Zimbabwe Sign Language"},
                                 new {Id    = "zik", Scope = "I", Type = "L", ReferenceName = "Zimakani"},
                                 new {Id    = "zil", Scope = "I", Type = "L", ReferenceName = "Zialo"},
                                 new {Id    = "zim", Scope = "I", Type = "L", ReferenceName = "Mesme"},
                                 new {Id    = "zin", Scope = "I", Type = "L", ReferenceName = "Zinza"},
                                 new {Id    = "zir", Scope = "I", Type = "E", ReferenceName = "Ziriya"},
                                 new {Id    = "ziw", Scope = "I", Type = "L", ReferenceName = "Zigula"},
                                 new {Id    = "ziz", Scope = "I", Type = "L", ReferenceName = "Zizilivakan"},
                                 new {Id    = "zka", Scope = "I", Type = "L", ReferenceName = "Kaimbulawa"},
                                 new {Id    = "zkb", Scope = "I", Type = "E", ReferenceName = "Koibal"},
                                 new {Id    = "zkd", Scope = "I", Type = "L", ReferenceName = "Kadu"},
                                 new {Id    = "zkg", Scope = "I", Type = "A", ReferenceName = "Koguryo"},
                                 new {Id    = "zkh", Scope = "I", Type = "H", ReferenceName = "Khorezmian"},
                                 new {Id    = "zkk", Scope = "I", Type = "E", ReferenceName = "Karankawa"},
                                 new {Id    = "zkn", Scope = "I", Type = "L", ReferenceName = "Kanan"},
                                 new {Id    = "zko", Scope = "I", Type = "E", ReferenceName = "Kott"},
                                 new {Id    = "zkp", Scope = "I", Type = "E", ReferenceName = "São Paulo Kaingáng"},
                                 new {Id    = "zkr", Scope = "I", Type = "L", ReferenceName = "Zakhring"},
                                 new {Id    = "zkt", Scope = "I", Type = "H", ReferenceName = "Kitan"},
                                 new {Id    = "zku", Scope = "I", Type = "L", ReferenceName = "Kaurna"},
                                 new {Id    = "zkv", Scope = "I", Type = "E", ReferenceName = "Krevinian"},
                                 new {Id    = "zkz", Scope = "I", Type = "H", ReferenceName = "Khazar"},
                                 new {Id    = "zlj", Scope = "I", Type = "L", ReferenceName = "Liujiang Zhuang"},
                                 new
                                 {
                                     Id            = "zlm",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Malay (individual language)"
                                 }, new {Id = "zln", Scope = "I", Type = "L", ReferenceName = "Lianshan Zhuang"},
                                 new {Id    = "zlq", Scope = "I", Type = "L", ReferenceName = "Liuqian Zhuang"},
                                 new {Id    = "zma", Scope = "I", Type = "L", ReferenceName = "Manda (Australia)"},
                                 new {Id    = "zmb", Scope = "I", Type = "L", ReferenceName = "Zimba"},
                                 new {Id    = "zmc", Scope = "I", Type = "E", ReferenceName = "Margany"},
                                 new {Id    = "zmd", Scope = "I", Type = "L", ReferenceName = "Maridan"},
                                 new {Id    = "zme", Scope = "I", Type = "E", ReferenceName = "Mangerr"},
                                 new {Id    = "zmf", Scope = "I", Type = "L", ReferenceName = "Mfinu"},
                                 new {Id    = "zmg", Scope = "I", Type = "L", ReferenceName = "Marti Ke"},
                                 new {Id    = "zmh", Scope = "I", Type = "E", ReferenceName = "Makolkol"},
                                 new
                                     {
                                         Id            = "zmi",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Negeri Sembilan Malay"
                                     },
                                 new {Id = "zmj", Scope = "I", Type = "L", ReferenceName = "Maridjabin"},
                                 new {Id = "zmk", Scope = "I", Type = "E", ReferenceName = "Mandandanyi"},
                                 new {Id = "zml", Scope = "I", Type = "E", ReferenceName = "Matngala"},
                                 new {Id = "zmm", Scope = "I", Type = "L", ReferenceName = "Marimanindji"},
                                 new {Id = "zmn", Scope = "I", Type = "L", ReferenceName = "Mbangwe"},
                                 new {Id = "zmo", Scope = "I", Type = "L", ReferenceName = "Molo"},
                                 new {Id = "zmp", Scope = "I", Type = "L", ReferenceName = "Mpuono"},
                                 new {Id = "zmq", Scope = "I", Type = "L", ReferenceName = "Mituku"},
                                 new {Id = "zmr", Scope = "I", Type = "L", ReferenceName = "Maranunggu"},
                                 new {Id = "zms", Scope = "I", Type = "L", ReferenceName = "Mbesa"},
                                 new {Id = "zmt", Scope = "I", Type = "L", ReferenceName = "Maringarr"},
                                 new {Id = "zmu", Scope = "I", Type = "E", ReferenceName = "Muruwari"},
                                 new {Id = "zmv", Scope = "I", Type = "E", ReferenceName = "Mbariman-Gudhinma"},
                                 new
                                 {
                                     Id            = "zmw",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Mbo (Democratic Republic of Congo)"
                                 }, new {Id = "zmx", Scope = "I", Type = "L", ReferenceName = "Bomitaba"},
                                 new {Id    = "zmy", Scope = "I", Type = "L", ReferenceName = "Mariyedi"},
                                 new {Id    = "zmz", Scope = "I", Type = "L", ReferenceName = "Mbandja"},
                                 new {Id    = "zna", Scope = "I", Type = "L", ReferenceName = "Zan Gula"},
                                 new
                                 {
                                     Id            = "zne",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Zande (individual language)"
                                 }, new {Id = "zng", Scope = "I", Type = "L", ReferenceName = "Mang"},
                                 new {Id    = "znk", Scope = "I", Type = "E", ReferenceName = "Manangkari"},
                                 new {Id    = "zns", Scope = "I", Type = "L", ReferenceName = "Mangas"},
                                 new {Id    = "zoc", Scope = "I", Type = "L", ReferenceName = "Copainalá Zoque"},
                                 new {Id    = "zoh", Scope = "I", Type = "L", ReferenceName = "Chimalapa Zoque"},
                                 new {Id    = "zom", Scope = "I", Type = "L", ReferenceName = "Zou"},
                                 new
                                     {
                                         Id            = "zoo",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Asunción Mixtepec Zapotec"
                                     },
                                 new {Id = "zoq", Scope = "I", Type = "L", ReferenceName = "Tabasco Zoque"},
                                 new {Id = "zor", Scope = "I", Type = "L", ReferenceName = "Rayón Zoque"},
                                 new
                                     {
                                         Id            = "zos",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Francisco León Zoque"
                                     },
                                 new {Id = "zpa", Scope = "I", Type = "L", ReferenceName = "Lachiguiri Zapotec"},
                                 new {Id = "zpb", Scope = "I", Type = "L", ReferenceName = "Yautepec Zapotec"},
                                 new {Id = "zpc", Scope = "I", Type = "L", ReferenceName = "Choapan Zapotec"},
                                 new
                                 {
                                     Id            = "zpd",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Southeastern Ixtlán Zapotec"
                                 }, new {Id = "zpe", Scope = "I", Type = "L", ReferenceName = "Petapa Zapotec"},
                                 new
                                 {
                                     Id            = "zpf",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "San Pedro Quiatoni Zapotec"
                                 },
                                 new
                                 {
                                     Id            = "zpg",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Guevea De Humboldt Zapotec"
                                 },
                                 new {Id = "zph", Scope = "I", Type = "L", ReferenceName = "Totomachapan Zapotec"},
                                 new
                                 {
                                     Id            = "zpi",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Santa María Quiegolani Zapotec"
                                 },
                                 new {Id = "zpj", Scope = "I", Type = "L", ReferenceName = "Quiavicuzas Zapotec"},
                                 new {Id = "zpk", Scope = "I", Type = "L", ReferenceName = "Tlacolulita Zapotec"},
                                 new {Id = "zpl", Scope = "I", Type = "L", ReferenceName = "Lachixío Zapotec"},
                                 new {Id = "zpm", Scope = "I", Type = "L", ReferenceName = "Mixtepec Zapotec"},
                                 new
                                 {
                                     Id            = "zpn",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Santa Inés Yatzechi Zapotec"
                                 }, new {Id = "zpo", Scope = "I", Type = "L", ReferenceName = "Amatlán Zapotec"},
                                 new {Id    = "zpp", Scope = "I", Type = "L", ReferenceName = "El Alto Zapotec"},
                                 new {Id    = "zpq", Scope = "I", Type = "L", ReferenceName = "Zoogocho Zapotec"},
                                 new
                                     {
                                         Id            = "zpr",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Santiago Xanica Zapotec"
                                     },
                                 new {Id = "zps", Scope = "I", Type = "L", ReferenceName = "Coatlán Zapotec"},
                                 new
                                 {
                                     Id            = "zpt",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "San Vicente Coatlán Zapotec"
                                 }, new {Id = "zpu", Scope = "I", Type = "L", ReferenceName = "Yalálag Zapotec"},
                                 new
                                     {
                                         Id            = "zpv",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Chichicapan Zapotec"
                                     },
                                 new {Id = "zpw", Scope = "I", Type = "L", ReferenceName = "Zaniza Zapotec"},
                                 new
                                 {
                                     Id            = "zpx",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "San Baltazar Loxicha Zapotec"
                                 }, new {Id = "zpy", Scope = "I", Type = "L", ReferenceName = "Mazaltepec Zapotec"},
                                 new {Id    = "zpz", Scope = "I", Type = "L", ReferenceName = "Texmelucan Zapotec"},
                                 new {Id    = "zqe", Scope = "I", Type = "L", ReferenceName = "Qiubei Zhuang"},
                                 new {Id    = "zra", Scope = "I", Type = "A", ReferenceName = "Kara (Korea)"},
                                 new {Id    = "zrg", Scope = "I", Type = "L", ReferenceName = "Mirgan"},
                                 new {Id    = "zrn", Scope = "I", Type = "L", ReferenceName = "Zerenkel"},
                                 new {Id    = "zro", Scope = "I", Type = "L", ReferenceName = "Záparo"},
                                 new {Id    = "zrp", Scope = "I", Type = "E", ReferenceName = "Zarphatic"},
                                 new {Id    = "zrs", Scope = "I", Type = "L", ReferenceName = "Mairasi"},
                                 new {Id    = "zsa", Scope = "I", Type = "L", ReferenceName = "Sarasira"},
                                 new {Id    = "zsk", Scope = "I", Type = "A", ReferenceName = "Kaskean"},
                                 new
                                     {
                                         Id            = "zsl",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Zambian Sign Language"
                                     },
                                 new {Id = "zsm", Scope = "I", Type = "L", ReferenceName = "Standard Malay"},
                                 new
                                     {
                                         Id            = "zsr",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Southern Rincon Zapotec"
                                     },
                                 new {Id = "zsu", Scope = "I", Type = "L", ReferenceName = "Sukurum"},
                                 new {Id = "zte", Scope = "I", Type = "L", ReferenceName = "Elotepec Zapotec"},
                                 new {Id = "ztg", Scope = "I", Type = "L", ReferenceName = "Xanaguía Zapotec"},
                                 new
                                     {
                                         Id            = "ztl",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Lapaguía-Guivini Zapotec"
                                     },
                                 new
                                 {
                                     Id            = "ztm",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "San Agustín Mixtepec Zapotec"
                                 },
                                 new
                                 {
                                     Id            = "ztn",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Santa Catarina Albarradas Zapotec"
                                 }, new {Id = "ztp", Scope = "I", Type = "L", ReferenceName = "Loxicha Zapotec"},
                                 new
                                 {
                                     Id            = "ztq",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Quioquitani-Quierí Zapotec"
                                 }, new {Id = "zts", Scope = "I", Type = "L", ReferenceName = "Tilquiapan Zapotec"},
                                 new {Id    = "ztt", Scope = "I", Type = "L", ReferenceName = "Tejalapan Zapotec"},
                                 new {Id    = "ztu", Scope = "I", Type = "L", ReferenceName = "Güilá Zapotec"},
                                 new {Id    = "ztx", Scope = "I", Type = "L", ReferenceName = "Zaachila Zapotec"},
                                 new {Id    = "zty", Scope = "I", Type = "L", ReferenceName = "Yatee Zapotec"},
                                 new {Id    = "zua", Scope = "I", Type = "L", ReferenceName = "Zeem"},
                                 new {Id    = "zuh", Scope = "I", Type = "L", ReferenceName = "Tokano"},
                                 new
                                 {
                                     Id            = "zul",
                                     Part2B        = "zul",
                                     Part2T        = "zul",
                                     Part1         = "zu",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Zulu"
                                 }, new {Id = "zum", Scope = "I", Type = "L", ReferenceName = "Kumzari"},
                                 new
                                 {
                                     Id            = "zun",
                                     Part2B        = "zun",
                                     Part2T        = "zun",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Zuni"
                                 }, new {Id = "zuy", Scope = "I", Type = "L", ReferenceName = "Zumaya"},
                                 new {Id    = "zwa", Scope = "I", Type = "L", ReferenceName = "Zay"},
                                 new
                                 {
                                     Id            = "zxx",
                                     Part2B        = "zxx",
                                     Part2T        = "zxx",
                                     Scope         = "S",
                                     Type          = "S",
                                     ReferenceName = "No linguistic content"
                                 }, new {Id = "zyb", Scope = "I", Type = "L", ReferenceName = "Yongbei Zhuang"},
                                 new {Id    = "zyg", Scope = "I", Type = "L", ReferenceName = "Yang Zhuang"},
                                 new {Id    = "zyj", Scope = "I", Type = "L", ReferenceName = "Youjiang Zhuang"},
                                 new {Id    = "zyn", Scope = "I", Type = "L", ReferenceName = "Yongnan Zhuang"},
                                 new {Id    = "zyp", Scope = "I", Type = "L", ReferenceName = "Zyphe Chin"},
                                 new
                                 {
                                     Id            = "zza",
                                     Part2B        = "zza",
                                     Part2T        = "zza",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Zaza"
                                 }, new {Id = "zzj", Scope = "I", Type = "L", ReferenceName = "Zuojiang Zhuang"});
        }
    }
}