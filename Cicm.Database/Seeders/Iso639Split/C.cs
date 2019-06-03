using Microsoft.EntityFrameworkCore;

namespace Cicm.Database.Seeders.Iso639Split
{
    public static class C
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Iso639>()
                        .HasData(new {Id = "caa", Scope = "I", Type = "L", ReferenceName = "Chortí"},
                                 new {Id = "cab", Scope = "I", Type = "L", ReferenceName = "Garifuna"},
                                 new {Id = "cac", Scope = "I", Type = "L", ReferenceName = "Chuj"},
                                 new
                                 {
                                     Id            = "cad",
                                     Part2B        = "cad",
                                     Part2T        = "cad",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Caddo"
                                 }, new {Id = "cae", Scope = "I", Type = "L", ReferenceName = "Lehar"},
                                 new {Id    = "caf", Scope = "I", Type = "L", ReferenceName = "Southern Carrier"},
                                 new {Id    = "cag", Scope = "I", Type = "L", ReferenceName = "Nivaclé"},
                                 new {Id    = "cah", Scope = "I", Type = "L", ReferenceName = "Cahuarano"},
                                 new {Id    = "caj", Scope = "I", Type = "E", ReferenceName = "Chané"},
                                 new {Id    = "cak", Scope = "I", Type = "L", ReferenceName = "Kaqchikel"},
                                 new {Id    = "cal", Scope = "I", Type = "L", ReferenceName = "Carolinian"},
                                 new {Id    = "cam", Scope = "I", Type = "L", ReferenceName = "Cemuhî"},
                                 new {Id    = "can", Scope = "I", Type = "L", ReferenceName = "Chambri"},
                                 new {Id    = "cao", Scope = "I", Type = "L", ReferenceName = "Chácobo"},
                                 new {Id    = "cap", Scope = "I", Type = "L", ReferenceName = "Chipaya"},
                                 new {Id    = "caq", Scope = "I", Type = "L", ReferenceName = "Car Nicobarese"},
                                 new
                                 {
                                     Id            = "car",
                                     Part2B        = "car",
                                     Part2T        = "car",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Galibi Carib"
                                 }, new {Id = "cas", Scope = "I", Type = "L", ReferenceName = "Tsimané"},
                                 new
                                 {
                                     Id            = "cat",
                                     Part2B        = "cat",
                                     Part2T        = "cat",
                                     Part1         = "ca",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Catalan"
                                 }, new {Id = "cav", Scope = "I", Type = "L", ReferenceName = "Cavineña"},
                                 new {Id    = "caw", Scope = "I", Type = "L", ReferenceName = "Callawalla"},
                                 new {Id    = "cax", Scope = "I", Type = "L", ReferenceName = "Chiquitano"},
                                 new {Id    = "cay", Scope = "I", Type = "L", ReferenceName = "Cayuga"},
                                 new {Id    = "caz", Scope = "I", Type = "E", ReferenceName = "Canichana"},
                                 new {Id    = "cbb", Scope = "I", Type = "L", ReferenceName = "Cabiyarí"},
                                 new {Id    = "cbc", Scope = "I", Type = "L", ReferenceName = "Carapana"},
                                 new {Id    = "cbd", Scope = "I", Type = "L", ReferenceName = "Carijona"},
                                 new {Id    = "cbg", Scope = "I", Type = "L", ReferenceName = "Chimila"},
                                 new {Id    = "cbi", Scope = "I", Type = "L", ReferenceName = "Chachi"},
                                 new {Id    = "cbj", Scope = "I", Type = "L", ReferenceName = "Ede Cabe"},
                                 new {Id    = "cbk", Scope = "I", Type = "L", ReferenceName = "Chavacano"},
                                 new {Id    = "cbl", Scope = "I", Type = "L", ReferenceName = "Bualkhaw Chin"},
                                 new {Id    = "cbn", Scope = "I", Type = "L", ReferenceName = "Nyahkur"},
                                 new {Id    = "cbo", Scope = "I", Type = "L", ReferenceName = "Izora"},
                                 new {Id    = "cbq", Scope = "I", Type = "L", ReferenceName = "Tsucuba"},
                                 new {Id    = "cbr", Scope = "I", Type = "L", ReferenceName = "Cashibo-Cacataibo"},
                                 new {Id    = "cbs", Scope = "I", Type = "L", ReferenceName = "Cashinahua"},
                                 new {Id    = "cbt", Scope = "I", Type = "L", ReferenceName = "Chayahuita"},
                                 new {Id    = "cbu", Scope = "I", Type = "L", ReferenceName = "Candoshi-Shapra"},
                                 new {Id    = "cbv", Scope = "I", Type = "L", ReferenceName = "Cacua"},
                                 new {Id    = "cbw", Scope = "I", Type = "L", ReferenceName = "Kinabalian"},
                                 new {Id    = "cby", Scope = "I", Type = "L", ReferenceName = "Carabayo"},
                                 new {Id    = "cca", Scope = "I", Type = "E", ReferenceName = "Cauca"},
                                 new {Id    = "ccc", Scope = "I", Type = "L", ReferenceName = "Chamicuro"},
                                 new {Id    = "ccd", Scope = "I", Type = "L", ReferenceName = "Cafundo Creole"},
                                 new {Id    = "cce", Scope = "I", Type = "L", ReferenceName = "Chopi"},
                                 new {Id    = "ccg", Scope = "I", Type = "L", ReferenceName = "Samba Daka"},
                                 new {Id    = "cch", Scope = "I", Type = "L", ReferenceName = "Atsam"},
                                 new {Id    = "ccj", Scope = "I", Type = "L", ReferenceName = "Kasanga"},
                                 new {Id    = "ccl", Scope = "I", Type = "L", ReferenceName = "Cutchi-Swahili"},
                                 new {Id    = "ccm", Scope = "I", Type = "L", ReferenceName = "Malaccan Creole Malay"},
                                 new {Id    = "cco", Scope = "I", Type = "L", ReferenceName = "Comaltepec Chinantec"},
                                 new {Id    = "ccp", Scope = "I", Type = "L", ReferenceName = "Chakma"},
                                 new {Id    = "ccr", Scope = "I", Type = "E", ReferenceName = "Cacaopera"},
                                 new {Id    = "cda", Scope = "I", Type = "L", ReferenceName = "Choni"},
                                 new {Id    = "cde", Scope = "I", Type = "L", ReferenceName = "Chenchu"},
                                 new {Id    = "cdf", Scope = "I", Type = "L", ReferenceName = "Chiru"},
                                 new {Id    = "cdg", Scope = "I", Type = "L", ReferenceName = "Chamari"},
                                 new {Id    = "cdh", Scope = "I", Type = "L", ReferenceName = "Chambeali"},
                                 new {Id    = "cdi", Scope = "I", Type = "L", ReferenceName = "Chodri"},
                                 new {Id    = "cdj", Scope = "I", Type = "L", ReferenceName = "Churahi"},
                                 new {Id    = "cdm", Scope = "I", Type = "L", ReferenceName = "Chepang"},
                                 new {Id    = "cdn", Scope = "I", Type = "L", ReferenceName = "Chaudangsi"},
                                 new {Id    = "cdo", Scope = "I", Type = "L", ReferenceName = "Min Dong Chinese"},
                                 new {Id    = "cdr", Scope = "I", Type = "L", ReferenceName = "Cinda-Regi-Tiyal"},
                                 new {Id    = "cds", Scope = "I", Type = "L", ReferenceName = "Chadian Sign Language"},
                                 new {Id    = "cdy", Scope = "I", Type = "L", ReferenceName = "Chadong"},
                                 new {Id    = "cdz", Scope = "I", Type = "L", ReferenceName = "Koda"},
                                 new {Id    = "cea", Scope = "I", Type = "E", ReferenceName = "Lower Chehalis"},
                                 new
                                 {
                                     Id            = "ceb",
                                     Part2B        = "ceb",
                                     Part2T        = "ceb",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Cebuano"
                                 }, new {Id = "ceg", Scope = "I", Type = "L", ReferenceName = "Chamacoco"},
                                 new {Id    = "cek", Scope = "I", Type = "L", ReferenceName = "Eastern Khumi Chin"},
                                 new {Id    = "cen", Scope = "I", Type = "L", ReferenceName = "Cen"},
                                 new
                                 {
                                     Id            = "ces",
                                     Part2B        = "cze",
                                     Part2T        = "ces",
                                     Part1         = "cs",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Czech"
                                 }, new {Id = "cet", Scope = "I", Type = "L", ReferenceName = "Centúúm"},
                                 new {Id    = "cey", Scope = "I", Type = "L", ReferenceName = "Ekai Chin"},
                                 new {Id    = "cfa", Scope = "I", Type = "L", ReferenceName = "Dijim-Bwilim"},
                                 new {Id    = "cfd", Scope = "I", Type = "L", ReferenceName = "Cara"},
                                 new {Id    = "cfg", Scope = "I", Type = "L", ReferenceName = "Como Karim"},
                                 new {Id    = "cfm", Scope = "I", Type = "L", ReferenceName = "Falam Chin"},
                                 new {Id    = "cga", Scope = "I", Type = "L", ReferenceName = "Changriwa"},
                                 new {Id    = "cgc", Scope = "I", Type = "L", ReferenceName = "Kagayanen"},
                                 new {Id    = "cgg", Scope = "I", Type = "L", ReferenceName = "Chiga"},
                                 new {Id    = "cgk", Scope = "I", Type = "L", ReferenceName = "Chocangacakha"},
                                 new
                                 {
                                     Id            = "cha",
                                     Part2B        = "cha",
                                     Part2T        = "cha",
                                     Part1         = "ch",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Chamorro"
                                 }, new
                                 {
                                     Id            = "chb",
                                     Part2B        = "chb",
                                     Part2T        = "chb",
                                     Scope         = "I",
                                     Type          = "E",
                                     ReferenceName = "Chibcha"
                                 }, new {Id = "chc", Scope = "I", Type = "E", ReferenceName = "Catawba"},
                                 new
                                     {
                                         Id            = "chd",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Highland Oaxaca Chontal"
                                     },
                                 new
                                 {
                                     Id            = "che",
                                     Part2B        = "che",
                                     Part2T        = "che",
                                     Part1         = "ce",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Chechen"
                                 }, new {Id = "chf", Scope = "I", Type = "L", ReferenceName = "Tabasco Chontal"},
                                 new
                                 {
                                     Id            = "chg",
                                     Part2B        = "chg",
                                     Part2T        = "chg",
                                     Scope         = "I",
                                     Type          = "E",
                                     ReferenceName = "Chagatai"
                                 }, new {Id = "chh", Scope = "I", Type = "E", ReferenceName = "Chinook"},
                                 new {Id    = "chj", Scope = "I", Type = "L", ReferenceName = "Ojitlán Chinantec"},
                                 new
                                 {
                                     Id            = "chk",
                                     Part2B        = "chk",
                                     Part2T        = "chk",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Chuukese"
                                 }, new {Id = "chl", Scope = "I", Type = "L", ReferenceName = "Cahuilla"},
                                 new
                                 {
                                     Id            = "chm",
                                     Part2B        = "chm",
                                     Part2T        = "chm",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Mari (Russia)"
                                 }, new
                                 {
                                     Id            = "chn",
                                     Part2B        = "chn",
                                     Part2T        = "chn",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Chinook jargon"
                                 }, new
                                 {
                                     Id            = "cho",
                                     Part2B        = "cho",
                                     Part2T        = "cho",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Choctaw"
                                 }, new
                                 {
                                     Id            = "chp",
                                     Part2B        = "chp",
                                     Part2T        = "chp",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Chipewyan"
                                 }, new {Id = "chq", Scope = "I", Type = "L", ReferenceName = "Quiotepec Chinantec"},
                                 new
                                 {
                                     Id            = "chr",
                                     Part2B        = "chr",
                                     Part2T        = "chr",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Cherokee"
                                 }, new {Id = "cht", Scope = "I", Type = "E", ReferenceName = "Cholón"},
                                 new
                                 {
                                     Id            = "chu",
                                     Part2B        = "chu",
                                     Part2T        = "chu",
                                     Part1         = "cu",
                                     Scope         = "I",
                                     Type          = "A",
                                     ReferenceName = "Church Slavic"
                                 }, new
                                 {
                                     Id            = "chv",
                                     Part2B        = "chv",
                                     Part2T        = "chv",
                                     Part1         = "cv",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Chuvash"
                                 }, new {Id = "chw", Scope = "I", Type = "L", ReferenceName = "Chuwabu"},
                                 new {Id    = "chx", Scope = "I", Type = "L", ReferenceName = "Chantyal"},
                                 new
                                 {
                                     Id            = "chy",
                                     Part2B        = "chy",
                                     Part2T        = "chy",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Cheyenne"
                                 }, new {Id = "chz", Scope = "I", Type = "L", ReferenceName = "Ozumacín Chinantec"},
                                 new {Id    = "cia", Scope = "I", Type = "L", ReferenceName = "Cia-Cia"},
                                 new {Id    = "cib", Scope = "I", Type = "L", ReferenceName = "Ci Gbe"},
                                 new {Id    = "cic", Scope = "I", Type = "L", ReferenceName = "Chickasaw"},
                                 new {Id    = "cid", Scope = "I", Type = "E", ReferenceName = "Chimariko"},
                                 new {Id    = "cie", Scope = "I", Type = "L", ReferenceName = "Cineni"},
                                 new {Id    = "cih", Scope = "I", Type = "L", ReferenceName = "Chinali"},
                                 new {Id    = "cik", Scope = "I", Type = "L", ReferenceName = "Chitkuli Kinnauri"},
                                 new {Id    = "cim", Scope = "I", Type = "L", ReferenceName = "Cimbrian"},
                                 new {Id    = "cin", Scope = "I", Type = "L", ReferenceName = "Cinta Larga"},
                                 new {Id    = "cip", Scope = "I", Type = "L", ReferenceName = "Chiapanec"},
                                 new {Id    = "cir", Scope = "I", Type = "L", ReferenceName = "Tiri"},
                                 new {Id    = "ciw", Scope = "I", Type = "L", ReferenceName = "Chippewa"},
                                 new {Id    = "ciy", Scope = "I", Type = "L", ReferenceName = "Chaima"},
                                 new {Id    = "cja", Scope = "I", Type = "L", ReferenceName = "Western Cham"},
                                 new {Id    = "cje", Scope = "I", Type = "L", ReferenceName = "Chru"},
                                 new {Id    = "cjh", Scope = "I", Type = "E", ReferenceName = "Upper Chehalis"},
                                 new {Id    = "cji", Scope = "I", Type = "L", ReferenceName = "Chamalal"},
                                 new {Id    = "cjk", Scope = "I", Type = "L", ReferenceName = "Chokwe"},
                                 new {Id    = "cjm", Scope = "I", Type = "L", ReferenceName = "Eastern Cham"},
                                 new {Id    = "cjn", Scope = "I", Type = "L", ReferenceName = "Chenapian"},
                                 new {Id    = "cjo", Scope = "I", Type = "L", ReferenceName = "Ashéninka Pajonal"},
                                 new {Id    = "cjp", Scope = "I", Type = "L", ReferenceName = "Cabécar"},
                                 new {Id    = "cjs", Scope = "I", Type = "L", ReferenceName = "Shor"},
                                 new {Id    = "cjv", Scope = "I", Type = "L", ReferenceName = "Chuave"},
                                 new {Id    = "cjy", Scope = "I", Type = "L", ReferenceName = "Jinyu Chinese"},
                                 new {Id    = "ckb", Scope = "I", Type = "L", ReferenceName = "Central Kurdish"},
                                 new {Id    = "ckh", Scope = "I", Type = "L", ReferenceName = "Chak"},
                                 new {Id    = "ckl", Scope = "I", Type = "L", ReferenceName = "Cibak"},
                                 new {Id    = "ckn", Scope = "I", Type = "L", ReferenceName = "Kaang Chin"},
                                 new {Id    = "cko", Scope = "I", Type = "L", ReferenceName = "Anufo"},
                                 new {Id    = "ckq", Scope = "I", Type = "L", ReferenceName = "Kajakse"},
                                 new {Id    = "ckr", Scope = "I", Type = "L", ReferenceName = "Kairak"},
                                 new {Id    = "cks", Scope = "I", Type = "L", ReferenceName = "Tayo"},
                                 new {Id    = "ckt", Scope = "I", Type = "L", ReferenceName = "Chukot"},
                                 new {Id    = "cku", Scope = "I", Type = "L", ReferenceName = "Koasati"},
                                 new {Id    = "ckv", Scope = "I", Type = "L", ReferenceName = "Kavalan"},
                                 new {Id    = "ckx", Scope = "I", Type = "L", ReferenceName = "Caka"},
                                 new {Id    = "cky", Scope = "I", Type = "L", ReferenceName = "Cakfem-Mushere"},
                                 new
                                 {
                                     Id            = "ckz",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Cakchiquel-Quiché Mixed Language"
                                 }, new {Id = "cla", Scope = "I", Type = "L", ReferenceName = "Ron"},
                                 new {Id    = "clc", Scope = "I", Type = "L", ReferenceName = "Chilcotin"},
                                 new
                                     {
                                         Id            = "cld",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Chaldean Neo-Aramaic"
                                     },
                                 new {Id = "cle", Scope = "I", Type = "L", ReferenceName = "Lealao Chinantec"},
                                 new {Id = "clh", Scope = "I", Type = "L", ReferenceName = "Chilisso"},
                                 new {Id = "cli", Scope = "I", Type = "L", ReferenceName = "Chakali"},
                                 new {Id = "clj", Scope = "I", Type = "L", ReferenceName = "Laitu Chin"},
                                 new {Id = "clk", Scope = "I", Type = "L", ReferenceName = "Idu-Mishmi"},
                                 new {Id = "cll", Scope = "I", Type = "L", ReferenceName = "Chala"},
                                 new {Id = "clm", Scope = "I", Type = "L", ReferenceName = "Clallam"},
                                 new
                                     {
                                         Id            = "clo",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Lowland Oaxaca Chontal"
                                     },
                                 new {Id = "clt", Scope = "I", Type = "L", ReferenceName = "Lautu Chin"},
                                 new {Id = "clu", Scope = "I", Type = "L", ReferenceName = "Caluyanun"},
                                 new {Id = "clw", Scope = "I", Type = "L", ReferenceName = "Chulym"},
                                 new
                                     {
                                         Id            = "cly",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Eastern Highland Chatino"
                                     },
                                 new {Id = "cma", Scope = "I", Type = "L", ReferenceName = "Maa"},
                                 new {Id = "cme", Scope = "I", Type = "L", ReferenceName = "Cerma"},
                                 new
                                     {
                                         Id            = "cmg",
                                         Scope         = "I",
                                         Type          = "H",
                                         ReferenceName = "Classical Mongolian"
                                     },
                                 new {Id = "cmi", Scope = "I", Type = "L", ReferenceName = "Emberá-Chamí"},
                                 new {Id = "cml", Scope = "I", Type = "L", ReferenceName = "Campalagian"},
                                 new {Id = "cmm", Scope = "I", Type = "E", ReferenceName = "Michigamea"},
                                 new {Id = "cmn", Scope = "I", Type = "L", ReferenceName = "Mandarin Chinese"},
                                 new {Id = "cmo", Scope = "I", Type = "L", ReferenceName = "Central Mnong"},
                                 new {Id = "cmr", Scope = "I", Type = "L", ReferenceName = "Mro-Khimi Chin"},
                                 new {Id = "cms", Scope = "I", Type = "A", ReferenceName = "Messapic"},
                                 new {Id = "cmt", Scope = "I", Type = "L", ReferenceName = "Camtho"},
                                 new {Id = "cna", Scope = "I", Type = "L", ReferenceName = "Changthang"},
                                 new {Id = "cnb", Scope = "I", Type = "L", ReferenceName = "Chinbon Chin"},
                                 new {Id = "cnc", Scope = "I", Type = "L", ReferenceName = "Côông"},
                                 new {Id = "cng", Scope = "I", Type = "L", ReferenceName = "Northern Qiang"},
                                 new {Id = "cnh", Scope = "I", Type = "L", ReferenceName = "Hakha Chin"},
                                 new {Id = "cni", Scope = "I", Type = "L", ReferenceName = "Asháninka"},
                                 new {Id = "cnk", Scope = "I", Type = "L", ReferenceName = "Khumi Chin"},
                                 new {Id = "cnl", Scope = "I", Type = "L", ReferenceName = "Lalana Chinantec"},
                                 new {Id = "cno", Scope = "I", Type = "L", ReferenceName = "Con"},
                                 new
                                 {
                                     Id            = "cnr",
                                     Part2B        = "cnr",
                                     Part2T        = "cnr",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Montenegrin"
                                 }, new {Id = "cns", Scope = "I", Type = "L", ReferenceName = "Central Asmat"},
                                 new {Id    = "cnt", Scope = "I", Type = "L", ReferenceName = "Tepetotutla Chinantec"},
                                 new {Id    = "cnu", Scope = "I", Type = "L", ReferenceName = "Chenoua"},
                                 new {Id    = "cnw", Scope = "I", Type = "L", ReferenceName = "Ngawn Chin"},
                                 new {Id    = "cnx", Scope = "I", Type = "H", ReferenceName = "Middle Cornish"},
                                 new {Id    = "coa", Scope = "I", Type = "L", ReferenceName = "Cocos Islands Malay"},
                                 new {Id    = "cob", Scope = "I", Type = "E", ReferenceName = "Chicomuceltec"},
                                 new {Id    = "coc", Scope = "I", Type = "L", ReferenceName = "Cocopa"},
                                 new {Id    = "cod", Scope = "I", Type = "L", ReferenceName = "Cocama-Cocamilla"},
                                 new {Id    = "coe", Scope = "I", Type = "L", ReferenceName = "Koreguaje"},
                                 new {Id    = "cof", Scope = "I", Type = "L", ReferenceName = "Colorado"},
                                 new {Id    = "cog", Scope = "I", Type = "L", ReferenceName = "Chong"},
                                 new {Id    = "coh", Scope = "I", Type = "L", ReferenceName = "Chonyi-Dzihana-Kauma"},
                                 new {Id    = "coj", Scope = "I", Type = "E", ReferenceName = "Cochimi"},
                                 new {Id    = "cok", Scope = "I", Type = "L", ReferenceName = "Santa Teresa Cora"},
                                 new {Id    = "col", Scope = "I", Type = "L", ReferenceName = "Columbia-Wenatchi"},
                                 new {Id    = "com", Scope = "I", Type = "L", ReferenceName = "Comanche"},
                                 new {Id    = "con", Scope = "I", Type = "L", ReferenceName = "Cofán"},
                                 new {Id    = "coo", Scope = "I", Type = "L", ReferenceName = "Comox"},
                                 new
                                 {
                                     Id            = "cop",
                                     Part2B        = "cop",
                                     Part2T        = "cop",
                                     Scope         = "I",
                                     Type          = "E",
                                     ReferenceName = "Coptic"
                                 }, new {Id = "coq", Scope = "I", Type = "E", ReferenceName = "Coquille"},
                                 new
                                 {
                                     Id            = "cor",
                                     Part2B        = "cor",
                                     Part2T        = "cor",
                                     Part1         = "kw",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Cornish"
                                 }, new
                                 {
                                     Id            = "cos",
                                     Part2B        = "cos",
                                     Part2T        = "cos",
                                     Part1         = "co",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Corsican"
                                 }, new {Id = "cot", Scope = "I", Type = "L", ReferenceName = "Caquinte"},
                                 new {Id    = "cou", Scope = "I", Type = "L", ReferenceName = "Wamey"},
                                 new {Id    = "cov", Scope = "I", Type = "L", ReferenceName = "Cao Miao"},
                                 new {Id    = "cow", Scope = "I", Type = "E", ReferenceName = "Cowlitz"},
                                 new {Id    = "cox", Scope = "I", Type = "L", ReferenceName = "Nanti"},
                                 new {Id    = "coz", Scope = "I", Type = "L", ReferenceName = "Chochotec"},
                                 new {Id    = "cpa", Scope = "I", Type = "L", ReferenceName = "Palantla Chinantec"},
                                 new
                                     {
                                         Id            = "cpb",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Ucayali-Yurúa Ashéninka"
                                     },
                                 new {Id = "cpc", Scope = "I", Type = "L", ReferenceName = "Ajyíninka Apurucayali"},
                                 new {Id = "cpg", Scope = "I", Type = "E", ReferenceName = "Cappadocian Greek"},
                                 new {Id = "cpi", Scope = "I", Type = "L", ReferenceName = "Chinese Pidgin English"},
                                 new {Id = "cpn", Scope = "I", Type = "L", ReferenceName = "Cherepon"},
                                 new {Id = "cpo", Scope = "I", Type = "L", ReferenceName = "Kpeego"},
                                 new {Id = "cps", Scope = "I", Type = "L", ReferenceName = "Capiznon"},
                                 new {Id = "cpu", Scope = "I", Type = "L", ReferenceName = "Pichis Ashéninka"},
                                 new {Id = "cpx", Scope = "I", Type = "L", ReferenceName = "Pu-Xian Chinese"},
                                 new
                                     {
                                         Id            = "cpy",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "South Ucayali Ashéninka"
                                     },
                                 new
                                 {
                                     Id            = "cqd",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Chuanqiandian Cluster Miao"
                                 }, new {Id = "cra", Scope = "I", Type = "L", ReferenceName = "Chara"},
                                 new {Id    = "crb", Scope = "I", Type = "E", ReferenceName = "Island Carib"},
                                 new {Id    = "crc", Scope = "I", Type = "L", ReferenceName = "Lonwolwol"},
                                 new {Id    = "crd", Scope = "I", Type = "L", ReferenceName = "Coeur d'Alene"},
                                 new
                                 {
                                     Id            = "cre",
                                     Part2B        = "cre",
                                     Part2T        = "cre",
                                     Part1         = "cr",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Cree"
                                 }, new {Id = "crf", Scope = "I", Type = "E", ReferenceName = "Caramanta"},
                                 new {Id    = "crg", Scope = "I", Type = "L", ReferenceName = "Michif"},
                                 new
                                 {
                                     Id            = "crh",
                                     Part2B        = "crh",
                                     Part2T        = "crh",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Crimean Tatar"
                                 }, new {Id = "cri", Scope = "I", Type = "L", ReferenceName = "Sãotomense"},
                                 new {Id    = "crj", Scope = "I", Type = "L", ReferenceName = "Southern East Cree"},
                                 new {Id    = "crk", Scope = "I", Type = "L", ReferenceName = "Plains Cree"},
                                 new {Id    = "crl", Scope = "I", Type = "L", ReferenceName = "Northern East Cree"},
                                 new {Id    = "crm", Scope = "I", Type = "L", ReferenceName = "Moose Cree"},
                                 new {Id    = "crn", Scope = "I", Type = "L", ReferenceName = "El Nayar Cora"},
                                 new {Id    = "cro", Scope = "I", Type = "L", ReferenceName = "Crow"},
                                 new {Id    = "crq", Scope = "I", Type = "L", ReferenceName = "Iyo'wujwa Chorote"},
                                 new {Id    = "crr", Scope = "I", Type = "E", ReferenceName = "Carolina Algonquian"},
                                 new {Id    = "crs", Scope = "I", Type = "L", ReferenceName = "Seselwa Creole French"},
                                 new {Id    = "crt", Scope = "I", Type = "L", ReferenceName = "Iyojwa'ja Chorote"},
                                 new {Id    = "crv", Scope = "I", Type = "L", ReferenceName = "Chaura"},
                                 new {Id    = "crw", Scope = "I", Type = "L", ReferenceName = "Chrau"},
                                 new {Id    = "crx", Scope = "I", Type = "L", ReferenceName = "Carrier"},
                                 new {Id    = "cry", Scope = "I", Type = "L", ReferenceName = "Cori"},
                                 new {Id    = "crz", Scope = "I", Type = "E", ReferenceName = "Cruzeño"},
                                 new {Id    = "csa", Scope = "I", Type = "L", ReferenceName = "Chiltepec Chinantec"},
                                 new
                                 {
                                     Id            = "csb",
                                     Part2B        = "csb",
                                     Part2T        = "csb",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Kashubian"
                                 }, new {Id = "csc", Scope = "I", Type = "L", ReferenceName = "Catalan Sign Language"},
                                 new
                                     {
                                         Id            = "csd",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Chiangmai Sign Language"
                                     },
                                 new {Id = "cse", Scope = "I", Type = "L", ReferenceName = "Czech Sign Language"},
                                 new {Id = "csf", Scope = "I", Type = "L", ReferenceName = "Cuba Sign Language"},
                                 new {Id = "csg", Scope = "I", Type = "L", ReferenceName = "Chilean Sign Language"},
                                 new {Id = "csh", Scope = "I", Type = "L", ReferenceName = "Asho Chin"},
                                 new {Id = "csi", Scope = "I", Type = "E", ReferenceName = "Coast Miwok"},
                                 new {Id = "csj", Scope = "I", Type = "L", ReferenceName = "Songlai Chin"},
                                 new {Id = "csk", Scope = "I", Type = "L", ReferenceName = "Jola-Kasa"},
                                 new {Id = "csl", Scope = "I", Type = "L", ReferenceName = "Chinese Sign Language"},
                                 new {Id = "csm", Scope = "I", Type = "L", ReferenceName = "Central Sierra Miwok"},
                                 new
                                     {
                                         Id            = "csn",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Colombian Sign Language"
                                     },
                                 new {Id = "cso", Scope = "I", Type = "L", ReferenceName = "Sochiapam Chinantec"},
                                 new {Id = "csq", Scope = "I", Type = "L", ReferenceName = "Croatia Sign Language"},
                                 new
                                     {
                                         Id            = "csr",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Costa Rican Sign Language"
                                     },
                                 new {Id = "css", Scope = "I", Type = "E", ReferenceName = "Southern Ohlone"},
                                 new {Id = "cst", Scope = "I", Type = "L", ReferenceName = "Northern Ohlone"},
                                 new {Id = "csv", Scope = "I", Type = "L", ReferenceName = "Sumtu Chin"},
                                 new {Id = "csw", Scope = "I", Type = "L", ReferenceName = "Swampy Cree"},
                                 new {Id = "csy", Scope = "I", Type = "L", ReferenceName = "Siyin Chin"},
                                 new {Id = "csz", Scope = "I", Type = "L", ReferenceName = "Coos"},
                                 new {Id = "cta", Scope = "I", Type = "L", ReferenceName = "Tataltepec Chatino"},
                                 new {Id = "ctc", Scope = "I", Type = "E", ReferenceName = "Chetco"},
                                 new {Id = "ctd", Scope = "I", Type = "L", ReferenceName = "Tedim Chin"},
                                 new {Id = "cte", Scope = "I", Type = "L", ReferenceName = "Tepinapa Chinantec"},
                                 new {Id = "ctg", Scope = "I", Type = "L", ReferenceName = "Chittagonian"},
                                 new {Id = "cth", Scope = "I", Type = "L", ReferenceName = "Thaiphum Chin"},
                                 new
                                     {
                                         Id            = "ctl",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Tlacoatzintepec Chinantec"
                                     },
                                 new {Id = "ctm", Scope = "I", Type = "E", ReferenceName = "Chitimacha"},
                                 new {Id = "ctn", Scope = "I", Type = "L", ReferenceName = "Chhintange"},
                                 new {Id = "cto", Scope = "I", Type = "L", ReferenceName = "Emberá-Catío"},
                                 new
                                     {
                                         Id            = "ctp",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Western Highland Chatino"
                                     },
                                 new
                                 {
                                     Id            = "cts",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Northern Catanduanes Bikol"
                                 }, new {Id = "ctt", Scope = "I", Type = "L", ReferenceName = "Wayanad Chetti"},
                                 new {Id    = "ctu", Scope = "I", Type = "L", ReferenceName = "Chol"},
                                 new {Id    = "ctz", Scope = "I", Type = "L", ReferenceName = "Zacatepec Chatino"},
                                 new {Id    = "cua", Scope = "I", Type = "L", ReferenceName = "Cua"},
                                 new {Id    = "cub", Scope = "I", Type = "L", ReferenceName = "Cubeo"},
                                 new {Id    = "cuc", Scope = "I", Type = "L", ReferenceName = "Usila Chinantec"},
                                 new {Id    = "cug", Scope = "I", Type = "L", ReferenceName = "Chungmboko"},
                                 new {Id    = "cuh", Scope = "I", Type = "L", ReferenceName = "Chuka"},
                                 new {Id    = "cui", Scope = "I", Type = "L", ReferenceName = "Cuiba"},
                                 new {Id    = "cuj", Scope = "I", Type = "L", ReferenceName = "Mashco Piro"},
                                 new {Id    = "cuk", Scope = "I", Type = "L", ReferenceName = "San Blas Kuna"},
                                 new {Id    = "cul", Scope = "I", Type = "L", ReferenceName = "Culina"},
                                 new {Id    = "cuo", Scope = "I", Type = "E", ReferenceName = "Cumanagoto"},
                                 new {Id    = "cup", Scope = "I", Type = "E", ReferenceName = "Cupeño"},
                                 new {Id    = "cuq", Scope = "I", Type = "L", ReferenceName = "Cun"},
                                 new {Id    = "cur", Scope = "I", Type = "L", ReferenceName = "Chhulung"},
                                 new {Id    = "cut", Scope = "I", Type = "L", ReferenceName = "Teutila Cuicatec"},
                                 new {Id    = "cuu", Scope = "I", Type = "L", ReferenceName = "Tai Ya"},
                                 new {Id    = "cuv", Scope = "I", Type = "L", ReferenceName = "Cuvok"},
                                 new {Id    = "cuw", Scope = "I", Type = "L", ReferenceName = "Chukwa"},
                                 new {Id    = "cux", Scope = "I", Type = "L", ReferenceName = "Tepeuxila Cuicatec"},
                                 new {Id    = "cuy", Scope = "I", Type = "L", ReferenceName = "Cuitlatec"},
                                 new {Id    = "cvg", Scope = "I", Type = "L", ReferenceName = "Chug"},
                                 new
                                     {
                                         Id            = "cvn",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Valle Nacional Chinantec"
                                     },
                                 new {Id = "cwa", Scope = "I", Type = "L", ReferenceName = "Kabwa"},
                                 new {Id = "cwb", Scope = "I", Type = "L", ReferenceName = "Maindo"},
                                 new {Id = "cwd", Scope = "I", Type = "L", ReferenceName = "Woods Cree"},
                                 new {Id = "cwe", Scope = "I", Type = "L", ReferenceName = "Kwere"},
                                 new {Id = "cwg", Scope = "I", Type = "L", ReferenceName = "Chewong"},
                                 new {Id = "cwt", Scope = "I", Type = "L", ReferenceName = "Kuwaataay"},
                                 new {Id = "cya", Scope = "I", Type = "L", ReferenceName = "Nopala Chatino"},
                                 new {Id = "cyb", Scope = "I", Type = "E", ReferenceName = "Cayubaba"},
                                 new
                                 {
                                     Id            = "cym",
                                     Part2B        = "wel",
                                     Part2T        = "cym",
                                     Part1         = "cy",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Welsh"
                                 }, new {Id = "cyo", Scope = "I", Type = "L", ReferenceName = "Cuyonon"},
                                 new {Id    = "czh", Scope = "I", Type = "L", ReferenceName = "Huizhou Chinese"},
                                 new {Id    = "czk", Scope = "I", Type = "E", ReferenceName = "Knaanic"},
                                 new {Id    = "czn", Scope = "I", Type = "L", ReferenceName = "Zenzontepec Chatino"},
                                 new {Id    = "czo", Scope = "I", Type = "L", ReferenceName = "Min Zhong Chinese"},
                                 new {Id    = "czt", Scope = "I", Type = "L", ReferenceName = "Zotung Chin"});
        }
    }
}