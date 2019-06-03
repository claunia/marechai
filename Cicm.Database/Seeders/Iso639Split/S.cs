using Microsoft.EntityFrameworkCore;

namespace Cicm.Database.Seeders.Iso639Split
{
    public static class S
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Iso639>()
                        .HasData(new {Id = "saa", Scope = "I", Type = "L", ReferenceName = "Saba"},
                                 new {Id = "sab", Scope = "I", Type = "L", ReferenceName = "Buglere"},
                                 new {Id = "sac", Scope = "I", Type = "L", ReferenceName = "Meskwaki"},
                                 new
                                 {
                                     Id            = "sad",
                                     Part2B        = "sad",
                                     Part2T        = "sad",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Sandawe"
                                 }, new {Id = "sae", Scope = "I", Type = "L", ReferenceName = "Sabanê"},
                                 new {Id    = "saf", Scope = "I", Type = "L", ReferenceName = "Safaliba"},
                                 new
                                 {
                                     Id            = "sag",
                                     Part2B        = "sag",
                                     Part2T        = "sag",
                                     Part1         = "sg",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Sango"
                                 }, new
                                 {
                                     Id            = "sah",
                                     Part2B        = "sah",
                                     Part2T        = "sah",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Yakut"
                                 }, new {Id = "saj", Scope = "I", Type = "L", ReferenceName = "Sahu"},
                                 new {Id    = "sak", Scope = "I", Type = "L", ReferenceName = "Sake"},
                                 new
                                 {
                                     Id            = "sam",
                                     Part2B        = "sam",
                                     Part2T        = "sam",
                                     Scope         = "I",
                                     Type          = "E",
                                     ReferenceName = "Samaritan Aramaic"
                                 }, new
                                 {
                                     Id            = "san",
                                     Part2B        = "san",
                                     Part2T        = "san",
                                     Part1         = "sa",
                                     Scope         = "I",
                                     Type          = "A",
                                     ReferenceName = "Sanskrit"
                                 }, new {Id = "sao", Scope = "I", Type = "L", ReferenceName = "Sause"},
                                 new {Id    = "saq", Scope = "I", Type = "L", ReferenceName = "Samburu"},
                                 new {Id    = "sar", Scope = "I", Type = "E", ReferenceName = "Saraveca"},
                                 new
                                 {
                                     Id            = "sas",
                                     Part2B        = "sas",
                                     Part2T        = "sas",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Sasak"
                                 }, new
                                 {
                                     Id            = "sat",
                                     Part2B        = "sat",
                                     Part2T        = "sat",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Santali"
                                 }, new {Id = "sau", Scope = "I", Type = "L", ReferenceName = "Saleman"},
                                 new {Id    = "sav", Scope = "I", Type = "L", ReferenceName = "Saafi-Saafi"},
                                 new {Id    = "saw", Scope = "I", Type = "L", ReferenceName = "Sawi"},
                                 new {Id    = "sax", Scope = "I", Type = "L", ReferenceName = "Sa"},
                                 new {Id    = "say", Scope = "I", Type = "L", ReferenceName = "Saya"},
                                 new {Id    = "saz", Scope = "I", Type = "L", ReferenceName = "Saurashtra"},
                                 new {Id    = "sba", Scope = "I", Type = "L", ReferenceName = "Ngambay"},
                                 new {Id    = "sbb", Scope = "I", Type = "L", ReferenceName = "Simbo"},
                                 new
                                     {
                                         Id            = "sbc",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Kele (Papua New Guinea)"
                                     },
                                 new {Id = "sbd", Scope = "I", Type = "L", ReferenceName = "Southern Samo"},
                                 new {Id = "sbe", Scope = "I", Type = "L", ReferenceName = "Saliba"},
                                 new {Id = "sbf", Scope = "I", Type = "L", ReferenceName = "Chabu"},
                                 new {Id = "sbg", Scope = "I", Type = "L", ReferenceName = "Seget"},
                                 new {Id = "sbh", Scope = "I", Type = "L", ReferenceName = "Sori-Harengan"},
                                 new {Id = "sbi", Scope = "I", Type = "L", ReferenceName = "Seti"},
                                 new {Id = "sbj", Scope = "I", Type = "L", ReferenceName = "Surbakhal"},
                                 new {Id = "sbk", Scope = "I", Type = "L", ReferenceName = "Safwa"},
                                 new {Id = "sbl", Scope = "I", Type = "L", ReferenceName = "Botolan Sambal"},
                                 new {Id = "sbm", Scope = "I", Type = "L", ReferenceName = "Sagala"},
                                 new {Id = "sbn", Scope = "I", Type = "L", ReferenceName = "Sindhi Bhil"},
                                 new {Id = "sbo", Scope = "I", Type = "L", ReferenceName = "Sabüm"},
                                 new {Id = "sbp", Scope = "I", Type = "L", ReferenceName = "Sangu (Tanzania)"},
                                 new {Id = "sbq", Scope = "I", Type = "L", ReferenceName = "Sileibi"},
                                 new {Id = "sbr", Scope = "I", Type = "L", ReferenceName = "Sembakung Murut"},
                                 new {Id = "sbs", Scope = "I", Type = "L", ReferenceName = "Subiya"},
                                 new {Id = "sbt", Scope = "I", Type = "L", ReferenceName = "Kimki"},
                                 new {Id = "sbu", Scope = "I", Type = "L", ReferenceName = "Stod Bhoti"},
                                 new {Id = "sbv", Scope = "I", Type = "A", ReferenceName = "Sabine"},
                                 new {Id = "sbw", Scope = "I", Type = "L", ReferenceName = "Simba"},
                                 new {Id = "sbx", Scope = "I", Type = "L", ReferenceName = "Seberuang"},
                                 new {Id = "sby", Scope = "I", Type = "L", ReferenceName = "Soli"},
                                 new {Id = "sbz", Scope = "I", Type = "L", ReferenceName = "Sara Kaba"},
                                 new {Id = "scb", Scope = "I", Type = "L", ReferenceName = "Chut"},
                                 new {Id = "sce", Scope = "I", Type = "L", ReferenceName = "Dongxiang"},
                                 new
                                     {
                                         Id            = "scf",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "San Miguel Creole French"
                                     },
                                 new {Id = "scg", Scope = "I", Type = "L", ReferenceName = "Sanggau"},
                                 new {Id = "sch", Scope = "I", Type = "L", ReferenceName = "Sakachep"},
                                 new
                                     {
                                         Id            = "sci",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Sri Lankan Creole Malay"
                                     },
                                 new {Id = "sck", Scope = "I", Type = "L", ReferenceName = "Sadri"},
                                 new {Id = "scl", Scope = "I", Type = "L", ReferenceName = "Shina"},
                                 new
                                 {
                                     Id            = "scn",
                                     Part2B        = "scn",
                                     Part2T        = "scn",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Sicilian"
                                 }, new
                                 {
                                     Id            = "sco",
                                     Part2B        = "sco",
                                     Part2T        = "sco",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Scots"
                                 }, new {Id = "scp", Scope = "I", Type = "L", ReferenceName = "Hyolmo"},
                                 new {Id    = "scq", Scope = "I", Type = "L", ReferenceName = "Sa'och"},
                                 new {Id    = "scs", Scope = "I", Type = "L", ReferenceName = "North Slavey"},
                                 new {Id    = "sct", Scope = "I", Type = "L", ReferenceName = "Southern Katang"},
                                 new {Id    = "scu", Scope = "I", Type = "L", ReferenceName = "Shumcho"},
                                 new {Id    = "scv", Scope = "I", Type = "L", ReferenceName = "Sheni"},
                                 new {Id    = "scw", Scope = "I", Type = "L", ReferenceName = "Sha"},
                                 new {Id    = "scx", Scope = "I", Type = "A", ReferenceName = "Sicel"},
                                 new {Id    = "sda", Scope = "I", Type = "L", ReferenceName = "Toraja-Sa'dan"},
                                 new {Id    = "sdb", Scope = "I", Type = "L", ReferenceName = "Shabak"},
                                 new {Id    = "sdc", Scope = "I", Type = "L", ReferenceName = "Sassarese Sardinian"},
                                 new {Id    = "sde", Scope = "I", Type = "L", ReferenceName = "Surubu"},
                                 new {Id    = "sdf", Scope = "I", Type = "L", ReferenceName = "Sarli"},
                                 new {Id    = "sdg", Scope = "I", Type = "L", ReferenceName = "Savi"},
                                 new {Id    = "sdh", Scope = "I", Type = "L", ReferenceName = "Southern Kurdish"},
                                 new {Id    = "sdj", Scope = "I", Type = "L", ReferenceName = "Suundi"},
                                 new {Id    = "sdk", Scope = "I", Type = "L", ReferenceName = "Sos Kundi"},
                                 new
                                 {
                                     Id            = "sdl",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Saudi Arabian Sign Language"
                                 }, new {Id = "sdm", Scope = "I", Type = "L", ReferenceName = "Semandang"},
                                 new
                                     {
                                         Id            = "sdn",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Gallurese Sardinian"
                                     },
                                 new
                                     {
                                         Id            = "sdo",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Bukar-Sadung Bidayuh"
                                     },
                                 new {Id = "sdp", Scope = "I", Type = "L", ReferenceName = "Sherdukpen"},
                                 new {Id = "sdr", Scope = "I", Type = "L", ReferenceName = "Oraon Sadri"},
                                 new {Id = "sds", Scope = "I", Type = "E", ReferenceName = "Sened"},
                                 new {Id = "sdt", Scope = "I", Type = "E", ReferenceName = "Shuadit"},
                                 new {Id = "sdu", Scope = "I", Type = "L", ReferenceName = "Sarudu"},
                                 new {Id = "sdx", Scope = "I", Type = "L", ReferenceName = "Sibu Melanau"},
                                 new {Id = "sdz", Scope = "I", Type = "L", ReferenceName = "Sallands"},
                                 new {Id = "sea", Scope = "I", Type = "L", ReferenceName = "Semai"},
                                 new {Id = "seb", Scope = "I", Type = "L", ReferenceName = "Shempire Senoufo"},
                                 new {Id = "sec", Scope = "I", Type = "L", ReferenceName = "Sechelt"},
                                 new {Id = "sed", Scope = "I", Type = "L", ReferenceName = "Sedang"},
                                 new {Id = "see", Scope = "I", Type = "L", ReferenceName = "Seneca"},
                                 new {Id = "sef", Scope = "I", Type = "L", ReferenceName = "Cebaara Senoufo"},
                                 new {Id = "seg", Scope = "I", Type = "L", ReferenceName = "Segeju"},
                                 new {Id = "seh", Scope = "I", Type = "L", ReferenceName = "Sena"},
                                 new {Id = "sei", Scope = "I", Type = "L", ReferenceName = "Seri"},
                                 new {Id = "sej", Scope = "I", Type = "L", ReferenceName = "Sene"},
                                 new {Id = "sek", Scope = "I", Type = "L", ReferenceName = "Sekani"},
                                 new
                                 {
                                     Id            = "sel",
                                     Part2B        = "sel",
                                     Part2T        = "sel",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Selkup"
                                 }, new {Id = "sen", Scope = "I", Type = "L", ReferenceName = "Nanerigé Sénoufo"},
                                 new {Id    = "seo", Scope = "I", Type = "L", ReferenceName = "Suarmin"},
                                 new {Id    = "sep", Scope = "I", Type = "L", ReferenceName = "Sìcìté Sénoufo"},
                                 new {Id    = "seq", Scope = "I", Type = "L", ReferenceName = "Senara Sénoufo"},
                                 new {Id    = "ser", Scope = "I", Type = "L", ReferenceName = "Serrano"},
                                 new
                                     {
                                         Id            = "ses",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Koyraboro Senni Songhai"
                                     },
                                 new {Id = "set", Scope = "I", Type = "L", ReferenceName = "Sentani"},
                                 new {Id = "seu", Scope = "I", Type = "L", ReferenceName = "Serui-Laut"},
                                 new {Id = "sev", Scope = "I", Type = "L", ReferenceName = "Nyarafolo Senoufo"},
                                 new {Id = "sew", Scope = "I", Type = "L", ReferenceName = "Sewa Bay"},
                                 new {Id = "sey", Scope = "I", Type = "L", ReferenceName = "Secoya"},
                                 new {Id = "sez", Scope = "I", Type = "L", ReferenceName = "Senthang Chin"},
                                 new
                                 {
                                     Id            = "sfb",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Langue des signes de Belgique Francophone"
                                 }, new {Id = "sfe", Scope = "I", Type = "L", ReferenceName = "Eastern Subanen"},
                                 new {Id    = "sfm", Scope = "I", Type = "L", ReferenceName = "Small Flowery Miao"},
                                 new
                                 {
                                     Id            = "sfs",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "South African Sign Language"
                                 }, new {Id = "sfw", Scope = "I", Type = "L", ReferenceName = "Sehwi"},
                                 new
                                 {
                                     Id            = "sga",
                                     Part2B        = "sga",
                                     Part2T        = "sga",
                                     Scope         = "I",
                                     Type          = "H",
                                     ReferenceName = "Old Irish (to 900)"
                                 }, new {Id = "sgb", Scope = "I", Type = "L", ReferenceName = "Mag-antsi Ayta"},
                                 new {Id    = "sgc", Scope = "I", Type = "L", ReferenceName = "Kipsigis"},
                                 new {Id    = "sgd", Scope = "I", Type = "L", ReferenceName = "Surigaonon"},
                                 new {Id    = "sge", Scope = "I", Type = "L", ReferenceName = "Segai"},
                                 new
                                 {
                                     Id            = "sgg",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Swiss-German Sign Language"
                                 }, new {Id = "sgh", Scope = "I", Type = "L", ReferenceName = "Shughni"},
                                 new {Id    = "sgi", Scope = "I", Type = "L", ReferenceName = "Suga"},
                                 new {Id    = "sgj", Scope = "I", Type = "L", ReferenceName = "Surgujia"},
                                 new {Id    = "sgk", Scope = "I", Type = "L", ReferenceName = "Sangkong"},
                                 new {Id    = "sgm", Scope = "I", Type = "E", ReferenceName = "Singa"},
                                 new {Id    = "sgp", Scope = "I", Type = "L", ReferenceName = "Singpho"},
                                 new {Id    = "sgr", Scope = "I", Type = "L", ReferenceName = "Sangisari"},
                                 new {Id    = "sgs", Scope = "I", Type = "L", ReferenceName = "Samogitian"},
                                 new {Id    = "sgt", Scope = "I", Type = "L", ReferenceName = "Brokpake"},
                                 new {Id    = "sgu", Scope = "I", Type = "L", ReferenceName = "Salas"},
                                 new {Id    = "sgw", Scope = "I", Type = "L", ReferenceName = "Sebat Bet Gurage"},
                                 new
                                 {
                                     Id            = "sgx",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Sierra Leone Sign Language"
                                 }, new {Id = "sgy", Scope = "I", Type = "L", ReferenceName = "Sanglechi"},
                                 new {Id    = "sgz", Scope = "I", Type = "L", ReferenceName = "Sursurunga"},
                                 new {Id    = "sha", Scope = "I", Type = "L", ReferenceName = "Shall-Zwall"},
                                 new {Id    = "shb", Scope = "I", Type = "L", ReferenceName = "Ninam"},
                                 new {Id    = "shc", Scope = "I", Type = "L", ReferenceName = "Sonde"},
                                 new {Id    = "shd", Scope = "I", Type = "L", ReferenceName = "Kundal Shahi"},
                                 new {Id    = "she", Scope = "I", Type = "L", ReferenceName = "Sheko"},
                                 new {Id    = "shg", Scope = "I", Type = "L", ReferenceName = "Shua"},
                                 new {Id    = "shh", Scope = "I", Type = "L", ReferenceName = "Shoshoni"},
                                 new {Id    = "shi", Scope = "I", Type = "L", ReferenceName = "Tachelhit"},
                                 new {Id    = "shj", Scope = "I", Type = "L", ReferenceName = "Shatt"},
                                 new {Id    = "shk", Scope = "I", Type = "L", ReferenceName = "Shilluk"},
                                 new {Id    = "shl", Scope = "I", Type = "L", ReferenceName = "Shendu"},
                                 new {Id    = "shm", Scope = "I", Type = "L", ReferenceName = "Shahrudi"},
                                 new
                                 {
                                     Id            = "shn",
                                     Part2B        = "shn",
                                     Part2T        = "shn",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Shan"
                                 }, new {Id = "sho", Scope = "I", Type = "L", ReferenceName = "Shanga"},
                                 new {Id    = "shp", Scope = "I", Type = "L", ReferenceName = "Shipibo-Conibo"},
                                 new {Id    = "shq", Scope = "I", Type = "L", ReferenceName = "Sala"},
                                 new {Id    = "shr", Scope = "I", Type = "L", ReferenceName = "Shi"},
                                 new {Id    = "shs", Scope = "I", Type = "L", ReferenceName = "Shuswap"},
                                 new {Id    = "sht", Scope = "I", Type = "E", ReferenceName = "Shasta"},
                                 new {Id    = "shu", Scope = "I", Type = "L", ReferenceName = "Chadian Arabic"},
                                 new {Id    = "shv", Scope = "I", Type = "L", ReferenceName = "Shehri"},
                                 new {Id    = "shw", Scope = "I", Type = "L", ReferenceName = "Shwai"},
                                 new {Id    = "shx", Scope = "I", Type = "L", ReferenceName = "She"},
                                 new {Id    = "shy", Scope = "I", Type = "L", ReferenceName = "Tachawit"},
                                 new {Id    = "shz", Scope = "I", Type = "L", ReferenceName = "Syenara Senoufo"},
                                 new {Id    = "sia", Scope = "I", Type = "E", ReferenceName = "Akkala Sami"},
                                 new {Id    = "sib", Scope = "I", Type = "L", ReferenceName = "Sebop"},
                                 new
                                 {
                                     Id            = "sid",
                                     Part2B        = "sid",
                                     Part2T        = "sid",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Sidamo"
                                 }, new {Id = "sie", Scope = "I", Type = "L", ReferenceName = "Simaa"},
                                 new {Id    = "sif", Scope = "I", Type = "L", ReferenceName = "Siamou"},
                                 new {Id    = "sig", Scope = "I", Type = "L", ReferenceName = "Paasaal"},
                                 new {Id    = "sih", Scope = "I", Type = "L", ReferenceName = "Zire"},
                                 new {Id    = "sii", Scope = "I", Type = "L", ReferenceName = "Shom Peng"},
                                 new {Id    = "sij", Scope = "I", Type = "L", ReferenceName = "Numbami"},
                                 new {Id    = "sik", Scope = "I", Type = "L", ReferenceName = "Sikiana"},
                                 new {Id    = "sil", Scope = "I", Type = "L", ReferenceName = "Tumulung Sisaala"},
                                 new
                                     {
                                         Id            = "sim",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Mende (Papua New Guinea)"
                                     },
                                 new
                                 {
                                     Id            = "sin",
                                     Part2B        = "sin",
                                     Part2T        = "sin",
                                     Part1         = "si",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Sinhala"
                                 }, new {Id = "sip", Scope = "I", Type = "L", ReferenceName = "Sikkimese"},
                                 new {Id    = "siq", Scope = "I", Type = "L", ReferenceName = "Sonia"},
                                 new {Id    = "sir", Scope = "I", Type = "L", ReferenceName = "Siri"},
                                 new {Id    = "sis", Scope = "I", Type = "E", ReferenceName = "Siuslaw"},
                                 new {Id    = "siu", Scope = "I", Type = "L", ReferenceName = "Sinagen"},
                                 new {Id    = "siv", Scope = "I", Type = "L", ReferenceName = "Sumariup"},
                                 new {Id    = "siw", Scope = "I", Type = "L", ReferenceName = "Siwai"},
                                 new {Id    = "six", Scope = "I", Type = "L", ReferenceName = "Sumau"},
                                 new {Id    = "siy", Scope = "I", Type = "L", ReferenceName = "Sivandi"},
                                 new {Id    = "siz", Scope = "I", Type = "L", ReferenceName = "Siwi"},
                                 new {Id    = "sja", Scope = "I", Type = "L", ReferenceName = "Epena"},
                                 new {Id    = "sjb", Scope = "I", Type = "L", ReferenceName = "Sajau Basap"},
                                 new {Id    = "sjd", Scope = "I", Type = "L", ReferenceName = "Kildin Sami"},
                                 new {Id    = "sje", Scope = "I", Type = "L", ReferenceName = "Pite Sami"},
                                 new {Id    = "sjg", Scope = "I", Type = "L", ReferenceName = "Assangori"},
                                 new {Id    = "sjk", Scope = "I", Type = "E", ReferenceName = "Kemi Sami"},
                                 new {Id    = "sjl", Scope = "I", Type = "L", ReferenceName = "Sajalong"},
                                 new {Id    = "sjm", Scope = "I", Type = "L", ReferenceName = "Mapun"},
                                 new {Id    = "sjn", Scope = "I", Type = "C", ReferenceName = "Sindarin"},
                                 new {Id    = "sjo", Scope = "I", Type = "L", ReferenceName = "Xibe"},
                                 new {Id    = "sjp", Scope = "I", Type = "L", ReferenceName = "Surjapuri"},
                                 new {Id    = "sjr", Scope = "I", Type = "L", ReferenceName = "Siar-Lak"},
                                 new {Id    = "sjs", Scope = "I", Type = "E", ReferenceName = "Senhaja De Srair"},
                                 new {Id    = "sjt", Scope = "I", Type = "L", ReferenceName = "Ter Sami"},
                                 new {Id    = "sju", Scope = "I", Type = "L", ReferenceName = "Ume Sami"},
                                 new {Id    = "sjw", Scope = "I", Type = "L", ReferenceName = "Shawnee"},
                                 new {Id    = "ska", Scope = "I", Type = "L", ReferenceName = "Skagit"},
                                 new {Id    = "skb", Scope = "I", Type = "L", ReferenceName = "Saek"},
                                 new {Id    = "skc", Scope = "I", Type = "L", ReferenceName = "Ma Manda"},
                                 new {Id    = "skd", Scope = "I", Type = "L", ReferenceName = "Southern Sierra Miwok"},
                                 new {Id    = "ske", Scope = "I", Type = "L", ReferenceName = "Seke (Vanuatu)"},
                                 new {Id    = "skf", Scope = "I", Type = "L", ReferenceName = "Sakirabiá"},
                                 new {Id    = "skg", Scope = "I", Type = "L", ReferenceName = "Sakalava Malagasy"},
                                 new {Id    = "skh", Scope = "I", Type = "L", ReferenceName = "Sikule"},
                                 new {Id    = "ski", Scope = "I", Type = "L", ReferenceName = "Sika"},
                                 new {Id    = "skj", Scope = "I", Type = "L", ReferenceName = "Seke (Nepal)"},
                                 new {Id    = "skm", Scope = "I", Type = "L", ReferenceName = "Kutong"},
                                 new {Id    = "skn", Scope = "I", Type = "L", ReferenceName = "Kolibugan Subanon"},
                                 new {Id    = "sko", Scope = "I", Type = "L", ReferenceName = "Seko Tengah"},
                                 new {Id    = "skp", Scope = "I", Type = "L", ReferenceName = "Sekapan"},
                                 new {Id    = "skq", Scope = "I", Type = "L", ReferenceName = "Sininkere"},
                                 new {Id    = "skr", Scope = "I", Type = "L", ReferenceName = "Saraiki"},
                                 new {Id    = "sks", Scope = "I", Type = "L", ReferenceName = "Maia"},
                                 new {Id    = "skt", Scope = "I", Type = "L", ReferenceName = "Sakata"},
                                 new {Id    = "sku", Scope = "I", Type = "L", ReferenceName = "Sakao"},
                                 new {Id    = "skv", Scope = "I", Type = "L", ReferenceName = "Skou"},
                                 new {Id    = "skw", Scope = "I", Type = "E", ReferenceName = "Skepi Creole Dutch"},
                                 new {Id    = "skx", Scope = "I", Type = "L", ReferenceName = "Seko Padang"},
                                 new {Id    = "sky", Scope = "I", Type = "L", ReferenceName = "Sikaiana"},
                                 new {Id    = "skz", Scope = "I", Type = "L", ReferenceName = "Sekar"},
                                 new {Id    = "slc", Scope = "I", Type = "L", ReferenceName = "Sáliba"},
                                 new {Id    = "sld", Scope = "I", Type = "L", ReferenceName = "Sissala"},
                                 new {Id    = "sle", Scope = "I", Type = "L", ReferenceName = "Sholaga"},
                                 new
                                 {
                                     Id            = "slf",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Swiss-Italian Sign Language"
                                 }, new {Id = "slg", Scope = "I", Type = "L", ReferenceName = "Selungai Murut"},
                                 new
                                 {
                                     Id            = "slh",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Southern Puget Sound Salish"
                                 }, new {Id = "sli", Scope = "I", Type = "L", ReferenceName = "Lower Silesian"},
                                 new {Id    = "slj", Scope = "I", Type = "L", ReferenceName = "Salumá"},
                                 new
                                 {
                                     Id            = "slk",
                                     Part2B        = "slo",
                                     Part2T        = "slk",
                                     Part1         = "sk",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Slovak"
                                 }, new {Id = "sll", Scope = "I", Type = "L", ReferenceName = "Salt-Yui"},
                                 new {Id    = "slm", Scope = "I", Type = "L", ReferenceName = "Pangutaran Sama"},
                                 new {Id    = "sln", Scope = "I", Type = "E", ReferenceName = "Salinan"},
                                 new {Id    = "slp", Scope = "I", Type = "L", ReferenceName = "Lamaholot"},
                                 new {Id    = "slq", Scope = "I", Type = "E", ReferenceName = "Salchuq"},
                                 new {Id    = "slr", Scope = "I", Type = "L", ReferenceName = "Salar"},
                                 new
                                     {
                                         Id            = "sls",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Singapore Sign Language"
                                     },
                                 new {Id = "slt", Scope = "I", Type = "L", ReferenceName = "Sila"},
                                 new {Id = "slu", Scope = "I", Type = "L", ReferenceName = "Selaru"},
                                 new
                                 {
                                     Id            = "slv",
                                     Part2B        = "slv",
                                     Part2T        = "slv",
                                     Part1         = "sl",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Slovenian"
                                 }, new {Id = "slw", Scope = "I", Type = "L", ReferenceName = "Sialum"},
                                 new {Id    = "slx", Scope = "I", Type = "L", ReferenceName = "Salampasu"},
                                 new {Id    = "sly", Scope = "I", Type = "L", ReferenceName = "Selayar"},
                                 new {Id    = "slz", Scope = "I", Type = "L", ReferenceName = "Ma'ya"},
                                 new
                                 {
                                     Id            = "sma",
                                     Part2B        = "sma",
                                     Part2T        = "sma",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Southern Sami"
                                 }, new {Id = "smb", Scope = "I", Type = "L", ReferenceName = "Simbari"},
                                 new {Id    = "smc", Scope = "I", Type = "E", ReferenceName = "Som"},
                                 new {Id    = "smd", Scope = "I", Type = "L", ReferenceName = "Sama"},
                                 new
                                 {
                                     Id            = "sme",
                                     Part2B        = "sme",
                                     Part2T        = "sme",
                                     Part1         = "se",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Northern Sami"
                                 }, new {Id = "smf", Scope = "I", Type = "L", ReferenceName = "Auwe"},
                                 new {Id    = "smg", Scope = "I", Type = "L", ReferenceName = "Simbali"},
                                 new {Id    = "smh", Scope = "I", Type = "L", ReferenceName = "Samei"},
                                 new
                                 {
                                     Id            = "smj",
                                     Part2B        = "smj",
                                     Part2T        = "smj",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Lule Sami"
                                 }, new {Id = "smk", Scope = "I", Type = "L", ReferenceName = "Bolinao"},
                                 new {Id    = "sml", Scope = "I", Type = "L", ReferenceName = "Central Sama"},
                                 new {Id    = "smm", Scope = "I", Type = "L", ReferenceName = "Musasa"},
                                 new
                                 {
                                     Id            = "smn",
                                     Part2B        = "smn",
                                     Part2T        = "smn",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Inari Sami"
                                 }, new
                                 {
                                     Id            = "smo",
                                     Part2B        = "smo",
                                     Part2T        = "smo",
                                     Part1         = "sm",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Samoan"
                                 }, new {Id = "smp", Scope = "I", Type = "E", ReferenceName = "Samaritan"},
                                 new {Id    = "smq", Scope = "I", Type = "L", ReferenceName = "Samo"},
                                 new {Id    = "smr", Scope = "I", Type = "L", ReferenceName = "Simeulue"},
                                 new
                                 {
                                     Id            = "sms",
                                     Part2B        = "sms",
                                     Part2T        = "sms",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Skolt Sami"
                                 }, new {Id = "smt", Scope = "I", Type = "L", ReferenceName = "Simte"},
                                 new {Id    = "smu", Scope = "I", Type = "E", ReferenceName = "Somray"},
                                 new {Id    = "smv", Scope = "I", Type = "L", ReferenceName = "Samvedi"},
                                 new {Id    = "smw", Scope = "I", Type = "L", ReferenceName = "Sumbawa"},
                                 new {Id    = "smx", Scope = "I", Type = "L", ReferenceName = "Samba"},
                                 new {Id    = "smy", Scope = "I", Type = "L", ReferenceName = "Semnani"},
                                 new {Id    = "smz", Scope = "I", Type = "L", ReferenceName = "Simeku"},
                                 new
                                 {
                                     Id            = "sna",
                                     Part2B        = "sna",
                                     Part2T        = "sna",
                                     Part1         = "sn",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Shona"
                                 }, new {Id = "snb", Scope = "I", Type = "L", ReferenceName = "Sebuyau"},
                                 new {Id    = "snc", Scope = "I", Type = "L", ReferenceName = "Sinaugoro"},
                                 new
                                 {
                                     Id            = "snd",
                                     Part2B        = "snd",
                                     Part2T        = "snd",
                                     Part1         = "sd",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Sindhi"
                                 }, new {Id = "sne", Scope = "I", Type = "L", ReferenceName = "Bau Bidayuh"},
                                 new {Id    = "snf", Scope = "I", Type = "L", ReferenceName = "Noon"},
                                 new
                                 {
                                     Id            = "sng",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Sanga (Democratic Republic of Congo)"
                                 }, new {Id = "sni", Scope = "I", Type = "E", ReferenceName = "Sensi"},
                                 new {Id    = "snj", Scope = "I", Type = "L", ReferenceName = "Riverain Sango"},
                                 new
                                 {
                                     Id            = "snk",
                                     Part2B        = "snk",
                                     Part2T        = "snk",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Soninke"
                                 }, new {Id = "snl", Scope = "I", Type = "L", ReferenceName = "Sangil"},
                                 new {Id    = "snm", Scope = "I", Type = "L", ReferenceName = "Southern Ma'di"},
                                 new {Id    = "snn", Scope = "I", Type = "L", ReferenceName = "Siona"},
                                 new {Id    = "sno", Scope = "I", Type = "L", ReferenceName = "Snohomish"},
                                 new {Id    = "snp", Scope = "I", Type = "L", ReferenceName = "Siane"},
                                 new {Id    = "snq", Scope = "I", Type = "L", ReferenceName = "Sangu (Gabon)"},
                                 new {Id    = "snr", Scope = "I", Type = "L", ReferenceName = "Sihan"},
                                 new {Id    = "sns", Scope = "I", Type = "L", ReferenceName = "South West Bay"},
                                 new {Id    = "snu", Scope = "I", Type = "L", ReferenceName = "Senggi"},
                                 new {Id    = "snv", Scope = "I", Type = "L", ReferenceName = "Sa'ban"},
                                 new {Id    = "snw", Scope = "I", Type = "L", ReferenceName = "Selee"},
                                 new {Id    = "snx", Scope = "I", Type = "L", ReferenceName = "Sam"},
                                 new {Id    = "sny", Scope = "I", Type = "L", ReferenceName = "Saniyo-Hiyewe"},
                                 new {Id    = "snz", Scope = "I", Type = "L", ReferenceName = "Kou"},
                                 new {Id    = "soa", Scope = "I", Type = "L", ReferenceName = "Thai Song"},
                                 new {Id    = "sob", Scope = "I", Type = "L", ReferenceName = "Sobei"},
                                 new
                                 {
                                     Id            = "soc",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "So (Democratic Republic of Congo)"
                                 }, new {Id = "sod", Scope = "I", Type = "L", ReferenceName = "Songoora"},
                                 new {Id    = "soe", Scope = "I", Type = "L", ReferenceName = "Songomeno"},
                                 new
                                 {
                                     Id            = "sog",
                                     Part2B        = "sog",
                                     Part2T        = "sog",
                                     Scope         = "I",
                                     Type          = "A",
                                     ReferenceName = "Sogdian"
                                 }, new {Id = "soh", Scope = "I", Type = "L", ReferenceName = "Aka"},
                                 new {Id    = "soi", Scope = "I", Type = "L", ReferenceName = "Sonha"},
                                 new {Id    = "soj", Scope = "I", Type = "L", ReferenceName = "Soi"},
                                 new {Id    = "sok", Scope = "I", Type = "L", ReferenceName = "Sokoro"},
                                 new {Id    = "sol", Scope = "I", Type = "L", ReferenceName = "Solos"},
                                 new
                                 {
                                     Id            = "som",
                                     Part2B        = "som",
                                     Part2T        = "som",
                                     Part1         = "so",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Somali"
                                 }, new {Id = "soo", Scope = "I", Type = "L", ReferenceName = "Songo"},
                                 new {Id    = "sop", Scope = "I", Type = "L", ReferenceName = "Songe"},
                                 new {Id    = "soq", Scope = "I", Type = "L", ReferenceName = "Kanasi"},
                                 new {Id    = "sor", Scope = "I", Type = "L", ReferenceName = "Somrai"},
                                 new {Id    = "sos", Scope = "I", Type = "L", ReferenceName = "Seeku"},
                                 new
                                 {
                                     Id            = "sot",
                                     Part2B        = "sot",
                                     Part2T        = "sot",
                                     Part1         = "st",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Southern Sotho"
                                 }, new {Id = "sou", Scope = "I", Type = "L", ReferenceName = "Southern Thai"},
                                 new {Id    = "sov", Scope = "I", Type = "L", ReferenceName = "Sonsorol"},
                                 new {Id    = "sow", Scope = "I", Type = "L", ReferenceName = "Sowanda"},
                                 new {Id    = "sox", Scope = "I", Type = "L", ReferenceName = "Swo"},
                                 new {Id    = "soy", Scope = "I", Type = "L", ReferenceName = "Miyobe"},
                                 new {Id    = "soz", Scope = "I", Type = "L", ReferenceName = "Temi"},
                                 new
                                 {
                                     Id            = "spa",
                                     Part2B        = "spa",
                                     Part2T        = "spa",
                                     Part1         = "es",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Spanish"
                                 }, new {Id = "spb", Scope = "I", Type = "L", ReferenceName = "Sepa (Indonesia)"},
                                 new {Id    = "spc", Scope = "I", Type = "L", ReferenceName = "Sapé"},
                                 new {Id    = "spd", Scope = "I", Type = "L", ReferenceName = "Saep"},
                                 new
                                     {
                                         Id            = "spe",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Sepa (Papua New Guinea)"
                                     },
                                 new {Id = "spg", Scope = "I", Type = "L", ReferenceName = "Sian"},
                                 new {Id = "spi", Scope = "I", Type = "L", ReferenceName = "Saponi"},
                                 new {Id = "spk", Scope = "I", Type = "L", ReferenceName = "Sengo"},
                                 new {Id = "spl", Scope = "I", Type = "L", ReferenceName = "Selepet"},
                                 new {Id = "spm", Scope = "I", Type = "L", ReferenceName = "Akukem"},
                                 new {Id = "spn", Scope = "I", Type = "L", ReferenceName = "Sanapaná"},
                                 new {Id = "spo", Scope = "I", Type = "L", ReferenceName = "Spokane"},
                                 new {Id = "spp", Scope = "I", Type = "L", ReferenceName = "Supyire Senoufo"},
                                 new {Id = "spq", Scope = "I", Type = "L", ReferenceName = "Loreto-Ucayali Spanish"},
                                 new {Id = "spr", Scope = "I", Type = "L", ReferenceName = "Saparua"},
                                 new {Id = "sps", Scope = "I", Type = "L", ReferenceName = "Saposa"},
                                 new {Id = "spt", Scope = "I", Type = "L", ReferenceName = "Spiti Bhoti"},
                                 new {Id = "spu", Scope = "I", Type = "L", ReferenceName = "Sapuan"},
                                 new {Id = "spv", Scope = "I", Type = "L", ReferenceName = "Sambalpuri"},
                                 new {Id = "spx", Scope = "I", Type = "A", ReferenceName = "South Picene"},
                                 new {Id = "spy", Scope = "I", Type = "L", ReferenceName = "Sabaot"},
                                 new {Id = "sqa", Scope = "I", Type = "L", ReferenceName = "Shama-Sambuga"},
                                 new {Id = "sqh", Scope = "I", Type = "L", ReferenceName = "Shau"},
                                 new
                                 {
                                     Id            = "sqi",
                                     Part2B        = "alb",
                                     Part2T        = "sqi",
                                     Part1         = "sq",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Albanian"
                                 }, new {Id = "sqk", Scope = "I", Type = "L", ReferenceName = "Albanian Sign Language"},
                                 new {Id    = "sqm", Scope = "I", Type = "L", ReferenceName = "Suma"},
                                 new {Id    = "sqn", Scope = "I", Type = "E", ReferenceName = "Susquehannock"},
                                 new {Id    = "sqo", Scope = "I", Type = "L", ReferenceName = "Sorkhei"},
                                 new {Id    = "sqq", Scope = "I", Type = "L", ReferenceName = "Sou"},
                                 new {Id    = "sqr", Scope = "I", Type = "H", ReferenceName = "Siculo Arabic"},
                                 new
                                     {
                                         Id            = "sqs",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Sri Lankan Sign Language"
                                     },
                                 new {Id = "sqt", Scope = "I", Type = "L", ReferenceName = "Soqotri"},
                                 new {Id = "squ", Scope = "I", Type = "L", ReferenceName = "Squamish"},
                                 new {Id = "sra", Scope = "I", Type = "L", ReferenceName = "Saruga"},
                                 new {Id = "srb", Scope = "I", Type = "L", ReferenceName = "Sora"},
                                 new {Id = "src", Scope = "I", Type = "L", ReferenceName = "Logudorese Sardinian"},
                                 new
                                 {
                                     Id            = "srd",
                                     Part2B        = "srd",
                                     Part2T        = "srd",
                                     Part1         = "sc",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Sardinian"
                                 }, new {Id = "sre", Scope = "I", Type = "L", ReferenceName = "Sara"},
                                 new {Id    = "srf", Scope = "I", Type = "L", ReferenceName = "Nafi"},
                                 new {Id    = "srg", Scope = "I", Type = "L", ReferenceName = "Sulod"},
                                 new {Id    = "srh", Scope = "I", Type = "L", ReferenceName = "Sarikoli"},
                                 new {Id    = "sri", Scope = "I", Type = "L", ReferenceName = "Siriano"},
                                 new {Id    = "srk", Scope = "I", Type = "L", ReferenceName = "Serudung Murut"},
                                 new {Id    = "srl", Scope = "I", Type = "L", ReferenceName = "Isirawa"},
                                 new {Id    = "srm", Scope = "I", Type = "L", ReferenceName = "Saramaccan"},
                                 new
                                 {
                                     Id            = "srn",
                                     Part2B        = "srn",
                                     Part2T        = "srn",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Sranan Tongo"
                                 }, new {Id = "sro", Scope = "I", Type = "L", ReferenceName = "Campidanese Sardinian"},
                                 new
                                 {
                                     Id            = "srp",
                                     Part2B        = "srp",
                                     Part2T        = "srp",
                                     Part1         = "sr",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Serbian"
                                 }, new {Id = "srq", Scope = "I", Type = "L", ReferenceName = "Sirionó"},
                                 new
                                 {
                                     Id            = "srr",
                                     Part2B        = "srr",
                                     Part2T        = "srr",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Serer"
                                 }, new {Id = "srs", Scope = "I", Type = "L", ReferenceName = "Sarsi"},
                                 new {Id    = "srt", Scope = "I", Type = "L", ReferenceName = "Sauri"},
                                 new {Id    = "sru", Scope = "I", Type = "L", ReferenceName = "Suruí"},
                                 new {Id    = "srv", Scope = "I", Type = "L", ReferenceName = "Southern Sorsoganon"},
                                 new {Id    = "srw", Scope = "I", Type = "L", ReferenceName = "Serua"},
                                 new {Id    = "srx", Scope = "I", Type = "L", ReferenceName = "Sirmauri"},
                                 new {Id    = "sry", Scope = "I", Type = "L", ReferenceName = "Sera"},
                                 new {Id    = "srz", Scope = "I", Type = "L", ReferenceName = "Shahmirzadi"},
                                 new {Id    = "ssb", Scope = "I", Type = "L", ReferenceName = "Southern Sama"},
                                 new {Id    = "ssc", Scope = "I", Type = "L", ReferenceName = "Suba-Simbiti"},
                                 new {Id    = "ssd", Scope = "I", Type = "L", ReferenceName = "Siroi"},
                                 new {Id    = "sse", Scope = "I", Type = "L", ReferenceName = "Balangingi"},
                                 new {Id    = "ssf", Scope = "I", Type = "E", ReferenceName = "Thao"},
                                 new {Id    = "ssg", Scope = "I", Type = "L", ReferenceName = "Seimat"},
                                 new {Id    = "ssh", Scope = "I", Type = "L", ReferenceName = "Shihhi Arabic"},
                                 new {Id    = "ssi", Scope = "I", Type = "L", ReferenceName = "Sansi"},
                                 new {Id    = "ssj", Scope = "I", Type = "L", ReferenceName = "Sausi"},
                                 new {Id    = "ssk", Scope = "I", Type = "L", ReferenceName = "Sunam"},
                                 new {Id    = "ssl", Scope = "I", Type = "L", ReferenceName = "Western Sisaala"},
                                 new {Id    = "ssm", Scope = "I", Type = "L", ReferenceName = "Semnam"},
                                 new {Id    = "ssn", Scope = "I", Type = "L", ReferenceName = "Waata"},
                                 new {Id    = "sso", Scope = "I", Type = "L", ReferenceName = "Sissano"},
                                 new {Id    = "ssp", Scope = "I", Type = "L", ReferenceName = "Spanish Sign Language"},
                                 new {Id    = "ssq", Scope = "I", Type = "L", ReferenceName = "So'a"},
                                 new
                                 {
                                     Id            = "ssr",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Swiss-French Sign Language"
                                 }, new {Id = "sss", Scope = "I", Type = "L", ReferenceName = "Sô"},
                                 new {Id    = "sst", Scope = "I", Type = "L", ReferenceName = "Sinasina"},
                                 new {Id    = "ssu", Scope = "I", Type = "L", ReferenceName = "Susuami"},
                                 new {Id    = "ssv", Scope = "I", Type = "L", ReferenceName = "Shark Bay"},
                                 new
                                 {
                                     Id            = "ssw",
                                     Part2B        = "ssw",
                                     Part2T        = "ssw",
                                     Part1         = "ss",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Swati"
                                 }, new {Id = "ssx", Scope = "I", Type = "L", ReferenceName = "Samberigi"},
                                 new {Id    = "ssy", Scope = "I", Type = "L", ReferenceName = "Saho"},
                                 new {Id    = "ssz", Scope = "I", Type = "L", ReferenceName = "Sengseng"},
                                 new {Id    = "sta", Scope = "I", Type = "L", ReferenceName = "Settla"},
                                 new {Id    = "stb", Scope = "I", Type = "L", ReferenceName = "Northern Subanen"},
                                 new {Id    = "std", Scope = "I", Type = "L", ReferenceName = "Sentinel"},
                                 new {Id    = "ste", Scope = "I", Type = "L", ReferenceName = "Liana-Seti"},
                                 new {Id    = "stf", Scope = "I", Type = "L", ReferenceName = "Seta"},
                                 new {Id    = "stg", Scope = "I", Type = "L", ReferenceName = "Trieng"},
                                 new {Id    = "sth", Scope = "I", Type = "L", ReferenceName = "Shelta"},
                                 new {Id    = "sti", Scope = "I", Type = "L", ReferenceName = "Bulo Stieng"},
                                 new {Id    = "stj", Scope = "I", Type = "L", ReferenceName = "Matya Samo"},
                                 new {Id    = "stk", Scope = "I", Type = "L", ReferenceName = "Arammba"},
                                 new {Id    = "stl", Scope = "I", Type = "L", ReferenceName = "Stellingwerfs"},
                                 new {Id    = "stm", Scope = "I", Type = "L", ReferenceName = "Setaman"},
                                 new {Id    = "stn", Scope = "I", Type = "L", ReferenceName = "Owa"},
                                 new {Id    = "sto", Scope = "I", Type = "L", ReferenceName = "Stoney"},
                                 new {Id    = "stp", Scope = "I", Type = "L", ReferenceName = "Southeastern Tepehuan"},
                                 new {Id    = "stq", Scope = "I", Type = "L", ReferenceName = "Saterfriesisch"},
                                 new {Id    = "str", Scope = "I", Type = "L", ReferenceName = "Straits Salish"},
                                 new {Id    = "sts", Scope = "I", Type = "L", ReferenceName = "Shumashti"},
                                 new {Id    = "stt", Scope = "I", Type = "L", ReferenceName = "Budeh Stieng"},
                                 new {Id    = "stu", Scope = "I", Type = "L", ReferenceName = "Samtao"},
                                 new {Id    = "stv", Scope = "I", Type = "L", ReferenceName = "Silt'e"},
                                 new {Id    = "stw", Scope = "I", Type = "L", ReferenceName = "Satawalese"},
                                 new {Id    = "sty", Scope = "I", Type = "L", ReferenceName = "Siberian Tatar"},
                                 new {Id    = "sua", Scope = "I", Type = "L", ReferenceName = "Sulka"},
                                 new {Id    = "sub", Scope = "I", Type = "L", ReferenceName = "Suku"},
                                 new {Id    = "suc", Scope = "I", Type = "L", ReferenceName = "Western Subanon"},
                                 new {Id    = "sue", Scope = "I", Type = "L", ReferenceName = "Suena"},
                                 new {Id    = "sug", Scope = "I", Type = "L", ReferenceName = "Suganga"},
                                 new {Id    = "sui", Scope = "I", Type = "L", ReferenceName = "Suki"},
                                 new {Id    = "suj", Scope = "I", Type = "L", ReferenceName = "Shubi"},
                                 new
                                 {
                                     Id            = "suk",
                                     Part2B        = "suk",
                                     Part2T        = "suk",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Sukuma"
                                 }, new
                                 {
                                     Id            = "sun",
                                     Part2B        = "sun",
                                     Part2T        = "sun",
                                     Part1         = "su",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Sundanese"
                                 }, new {Id = "suq", Scope = "I", Type = "L", ReferenceName = "Suri"},
                                 new {Id    = "sur", Scope = "I", Type = "L", ReferenceName = "Mwaghavul"},
                                 new
                                 {
                                     Id            = "sus",
                                     Part2B        = "sus",
                                     Part2T        = "sus",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Susu"
                                 }, new {Id = "sut", Scope = "I", Type = "E", ReferenceName = "Subtiaba"},
                                 new {Id    = "suv", Scope = "I", Type = "L", ReferenceName = "Puroik"},
                                 new {Id    = "suw", Scope = "I", Type = "L", ReferenceName = "Sumbwa"},
                                 new
                                 {
                                     Id            = "sux",
                                     Part2B        = "sux",
                                     Part2T        = "sux",
                                     Scope         = "I",
                                     Type          = "A",
                                     ReferenceName = "Sumerian"
                                 }, new {Id = "suy", Scope = "I", Type = "L", ReferenceName = "Suyá"},
                                 new {Id    = "suz", Scope = "I", Type = "L", ReferenceName = "Sunwar"},
                                 new {Id    = "sva", Scope = "I", Type = "L", ReferenceName = "Svan"},
                                 new {Id    = "svb", Scope = "I", Type = "L", ReferenceName = "Ulau-Suain"},
                                 new
                                     {
                                         Id            = "svc",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Vincentian Creole English"
                                     },
                                 new {Id = "sve", Scope = "I", Type = "L", ReferenceName = "Serili"},
                                 new
                                     {
                                         Id            = "svk",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Slovakian Sign Language"
                                     },
                                 new {Id = "svm", Scope = "I", Type = "L", ReferenceName = "Slavomolisano"},
                                 new {Id = "svs", Scope = "I", Type = "L", ReferenceName = "Savosavo"},
                                 new {Id = "svx", Scope = "I", Type = "H", ReferenceName = "Skalvian"},
                                 new
                                 {
                                     Id            = "swa",
                                     Part2B        = "swa",
                                     Part2T        = "swa",
                                     Part1         = "sw",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Swahili (macrolanguage)"
                                 }, new {Id = "swb", Scope = "I", Type = "L", ReferenceName = "Maore Comorian"},
                                 new {Id    = "swc", Scope = "I", Type = "L", ReferenceName = "Congo Swahili"},
                                 new
                                 {
                                     Id            = "swe",
                                     Part2B        = "swe",
                                     Part2T        = "swe",
                                     Part1         = "sv",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Swedish"
                                 }, new {Id = "swf", Scope = "I", Type = "L", ReferenceName = "Sere"},
                                 new {Id    = "swg", Scope = "I", Type = "L", ReferenceName = "Swabian"},
                                 new
                                 {
                                     Id            = "swh",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Swahili (individual language)"
                                 }, new {Id = "swi", Scope = "I", Type = "L", ReferenceName = "Sui"},
                                 new {Id    = "swj", Scope = "I", Type = "L", ReferenceName = "Sira"},
                                 new {Id    = "swk", Scope = "I", Type = "L", ReferenceName = "Malawi Sena"},
                                 new
                                     {
                                         Id            = "swl",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Swedish Sign Language"
                                     },
                                 new {Id = "swm", Scope = "I", Type = "L", ReferenceName = "Samosa"},
                                 new {Id = "swn", Scope = "I", Type = "L", ReferenceName = "Sawknah"},
                                 new {Id = "swo", Scope = "I", Type = "L", ReferenceName = "Shanenawa"},
                                 new {Id = "swp", Scope = "I", Type = "L", ReferenceName = "Suau"},
                                 new {Id = "swq", Scope = "I", Type = "L", ReferenceName = "Sharwa"},
                                 new {Id = "swr", Scope = "I", Type = "L", ReferenceName = "Saweru"},
                                 new {Id = "sws", Scope = "I", Type = "L", ReferenceName = "Seluwasan"},
                                 new {Id = "swt", Scope = "I", Type = "L", ReferenceName = "Sawila"},
                                 new {Id = "swu", Scope = "I", Type = "L", ReferenceName = "Suwawa"},
                                 new {Id = "swv", Scope = "I", Type = "L", ReferenceName = "Shekhawati"},
                                 new {Id = "sww", Scope = "I", Type = "E", ReferenceName = "Sowa"},
                                 new {Id = "swx", Scope = "I", Type = "L", ReferenceName = "Suruahá"},
                                 new {Id = "swy", Scope = "I", Type = "L", ReferenceName = "Sarua"},
                                 new {Id = "sxb", Scope = "I", Type = "L", ReferenceName = "Suba"},
                                 new {Id = "sxc", Scope = "I", Type = "A", ReferenceName = "Sicanian"},
                                 new {Id = "sxe", Scope = "I", Type = "L", ReferenceName = "Sighu"},
                                 new {Id = "sxg", Scope = "I", Type = "L", ReferenceName = "Shuhi"},
                                 new {Id = "sxk", Scope = "I", Type = "E", ReferenceName = "Southern Kalapuya"},
                                 new {Id = "sxl", Scope = "I", Type = "E", ReferenceName = "Selian"},
                                 new {Id = "sxm", Scope = "I", Type = "L", ReferenceName = "Samre"},
                                 new {Id = "sxn", Scope = "I", Type = "L", ReferenceName = "Sangir"},
                                 new {Id = "sxo", Scope = "I", Type = "A", ReferenceName = "Sorothaptic"},
                                 new {Id = "sxr", Scope = "I", Type = "L", ReferenceName = "Saaroa"},
                                 new {Id = "sxs", Scope = "I", Type = "L", ReferenceName = "Sasaru"},
                                 new {Id = "sxu", Scope = "I", Type = "L", ReferenceName = "Upper Saxon"},
                                 new {Id = "sxw", Scope = "I", Type = "L", ReferenceName = "Saxwe Gbe"},
                                 new {Id = "sya", Scope = "I", Type = "L", ReferenceName = "Siang"},
                                 new {Id = "syb", Scope = "I", Type = "L", ReferenceName = "Central Subanen"},
                                 new
                                 {
                                     Id            = "syc",
                                     Part2B        = "syc",
                                     Part2T        = "syc",
                                     Scope         = "I",
                                     Type          = "H",
                                     ReferenceName = "Classical Syriac"
                                 }, new {Id = "syi", Scope = "I", Type = "L", ReferenceName = "Seki"},
                                 new {Id    = "syk", Scope = "I", Type = "L", ReferenceName = "Sukur"},
                                 new {Id    = "syl", Scope = "I", Type = "L", ReferenceName = "Sylheti"},
                                 new {Id    = "sym", Scope = "I", Type = "L", ReferenceName = "Maya Samo"},
                                 new {Id    = "syn", Scope = "I", Type = "L", ReferenceName = "Senaya"},
                                 new {Id    = "syo", Scope = "I", Type = "L", ReferenceName = "Suoy"},
                                 new
                                 {
                                     Id            = "syr",
                                     Part2B        = "syr",
                                     Part2T        = "syr",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Syriac"
                                 }, new {Id = "sys", Scope = "I", Type = "L", ReferenceName = "Sinyar"},
                                 new {Id    = "syw", Scope = "I", Type = "L", ReferenceName = "Kagate"},
                                 new {Id    = "syx", Scope = "I", Type = "L", ReferenceName = "Samay"},
                                 new
                                 {
                                     Id            = "syy",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Al-Sayyid Bedouin Sign Language"
                                 }, new {Id = "sza", Scope = "I", Type = "L", ReferenceName = "Semelai"},
                                 new {Id    = "szb", Scope = "I", Type = "L", ReferenceName = "Ngalum"},
                                 new {Id    = "szc", Scope = "I", Type = "L", ReferenceName = "Semaq Beri"},
                                 new {Id    = "szd", Scope = "I", Type = "E", ReferenceName = "Seru"},
                                 new {Id    = "sze", Scope = "I", Type = "L", ReferenceName = "Seze"},
                                 new {Id    = "szg", Scope = "I", Type = "L", ReferenceName = "Sengele"},
                                 new {Id    = "szl", Scope = "I", Type = "L", ReferenceName = "Silesian"},
                                 new {Id    = "szn", Scope = "I", Type = "L", ReferenceName = "Sula"},
                                 new {Id    = "szp", Scope = "I", Type = "L", ReferenceName = "Suabo"},
                                 new
                                 {
                                     Id            = "szs",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Solomon Islands Sign Language"
                                 },
                                 new {Id = "szv", Scope = "I", Type = "L", ReferenceName = "Isu (Fako Division)"},
                                 new {Id = "szw", Scope = "I", Type = "L", ReferenceName = "Sawai"},
                                 new {Id = "szy", Scope = "I", Type = "L", ReferenceName = "Sakizaya"});
        }
    }
}