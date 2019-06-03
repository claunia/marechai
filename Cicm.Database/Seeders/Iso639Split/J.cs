using Microsoft.EntityFrameworkCore;

namespace Cicm.Database.Seeders.Iso639Split
{
    public static class J
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Iso639>()
                        .HasData(new {Id = "jaa", Scope = "I", Type = "L", ReferenceName = "Jamamadí"},
                                 new {Id = "jab", Scope = "I", Type = "L", ReferenceName = "Hyam"},
                                 new {Id = "jac", Scope = "I", Type = "L", ReferenceName = "Popti'"},
                                 new {Id = "jad", Scope = "I", Type = "L", ReferenceName = "Jahanka"},
                                 new {Id = "jae", Scope = "I", Type = "L", ReferenceName = "Yabem"},
                                 new {Id = "jaf", Scope = "I", Type = "L", ReferenceName = "Jara"},
                                 new {Id = "jah", Scope = "I", Type = "L", ReferenceName = "Jah Hut"},
                                 new {Id = "jaj", Scope = "I", Type = "L", ReferenceName = "Zazao"},
                                 new {Id = "jak", Scope = "I", Type = "L", ReferenceName = "Jakun"},
                                 new {Id = "jal", Scope = "I", Type = "L", ReferenceName = "Yalahatan"},
                                 new {Id = "jam", Scope = "I", Type = "L", ReferenceName = "Jamaican Creole English"},
                                 new {Id = "jan", Scope = "I", Type = "E", ReferenceName = "Jandai"},
                                 new {Id = "jao", Scope = "I", Type = "L", ReferenceName = "Yanyuwa"},
                                 new {Id = "jaq", Scope = "I", Type = "L", ReferenceName = "Yaqay"},
                                 new {Id = "jas", Scope = "I", Type = "L", ReferenceName = "New Caledonian Javanese"},
                                 new {Id = "jat", Scope = "I", Type = "L", ReferenceName = "Jakati"},
                                 new {Id = "jau", Scope = "I", Type = "L", ReferenceName = "Yaur"},
                                 new
                                 {
                                     Id            = "jav",
                                     Part2B        = "jav",
                                     Part2T        = "jav",
                                     Part1         = "jv",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Javanese"
                                 }, new {Id = "jax", Scope = "I", Type = "L", ReferenceName = "Jambi Malay"},
                                 new {Id    = "jay", Scope = "I", Type = "L", ReferenceName = "Yan-nhangu"},
                                 new {Id    = "jaz", Scope = "I", Type = "L", ReferenceName = "Jawe"},
                                 new {Id    = "jbe", Scope = "I", Type = "L", ReferenceName = "Judeo-Berber"},
                                 new {Id    = "jbi", Scope = "I", Type = "E", ReferenceName = "Badjiri"},
                                 new {Id    = "jbj", Scope = "I", Type = "L", ReferenceName = "Arandai"},
                                 new {Id    = "jbk", Scope = "I", Type = "L", ReferenceName = "Barikewa"},
                                 new {Id    = "jbn", Scope = "I", Type = "L", ReferenceName = "Nafusi"},
                                 new
                                 {
                                     Id            = "jbo",
                                     Part2B        = "jbo",
                                     Part2T        = "jbo",
                                     Scope         = "I",
                                     Type          = "C",
                                     ReferenceName = "Lojban"
                                 }, new {Id = "jbr", Scope = "I", Type = "L", ReferenceName = "Jofotek-Bromnya"},
                                 new {Id    = "jbt", Scope = "I", Type = "L", ReferenceName = "Jabutí"},
                                 new {Id    = "jbu", Scope = "I", Type = "L", ReferenceName = "Jukun Takum"},
                                 new {Id    = "jbw", Scope = "I", Type = "E", ReferenceName = "Yawijibaya"},
                                 new
                                 {
                                     Id            = "jcs",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Jamaican Country Sign Language"
                                 }, new {Id = "jct", Scope = "I", Type = "L", ReferenceName = "Krymchak"},
                                 new {Id    = "jda", Scope = "I", Type = "L", ReferenceName = "Jad"},
                                 new {Id    = "jdg", Scope = "I", Type = "L", ReferenceName = "Jadgali"},
                                 new {Id    = "jdt", Scope = "I", Type = "L", ReferenceName = "Judeo-Tat"},
                                 new {Id    = "jeb", Scope = "I", Type = "L", ReferenceName = "Jebero"},
                                 new {Id    = "jee", Scope = "I", Type = "L", ReferenceName = "Jerung"},
                                 new {Id    = "jeh", Scope = "I", Type = "L", ReferenceName = "Jeh"},
                                 new {Id    = "jei", Scope = "I", Type = "L", ReferenceName = "Yei"},
                                 new {Id    = "jek", Scope = "I", Type = "L", ReferenceName = "Jeri Kuo"},
                                 new {Id    = "jel", Scope = "I", Type = "L", ReferenceName = "Yelmek"},
                                 new {Id    = "jen", Scope = "I", Type = "L", ReferenceName = "Dza"},
                                 new {Id    = "jer", Scope = "I", Type = "L", ReferenceName = "Jere"},
                                 new {Id    = "jet", Scope = "I", Type = "L", ReferenceName = "Manem"},
                                 new
                                     {
                                         Id            = "jeu",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Jonkor Bourmataguil"
                                     },
                                 new {Id = "jgb", Scope = "I", Type = "E", ReferenceName = "Ngbee"},
                                 new {Id = "jge", Scope = "I", Type = "L", ReferenceName = "Judeo-Georgian"},
                                 new {Id = "jgk", Scope = "I", Type = "L", ReferenceName = "Gwak"},
                                 new {Id = "jgo", Scope = "I", Type = "L", ReferenceName = "Ngomba"},
                                 new {Id = "jhi", Scope = "I", Type = "L", ReferenceName = "Jehai"},
                                 new
                                     {
                                         Id            = "jhs",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Jhankot Sign Language"
                                     },
                                 new {Id = "jia", Scope = "I", Type = "L", ReferenceName = "Jina"},
                                 new {Id = "jib", Scope = "I", Type = "L", ReferenceName = "Jibu"},
                                 new {Id = "jic", Scope = "I", Type = "L", ReferenceName = "Tol"},
                                 new {Id = "jid", Scope = "I", Type = "L", ReferenceName = "Bu"},
                                 new {Id = "jie", Scope = "I", Type = "L", ReferenceName = "Jilbe"},
                                 new {Id = "jig", Scope = "I", Type = "L", ReferenceName = "Jingulu"},
                                 new {Id = "jih", Scope = "I", Type = "L", ReferenceName = "sTodsde"},
                                 new {Id = "jii", Scope = "I", Type = "L", ReferenceName = "Jiiddu"},
                                 new {Id = "jil", Scope = "I", Type = "L", ReferenceName = "Jilim"},
                                 new {Id = "jim", Scope = "I", Type = "L", ReferenceName = "Jimi (Cameroon)"},
                                 new {Id = "jio", Scope = "I", Type = "L", ReferenceName = "Jiamao"},
                                 new {Id = "jiq", Scope = "I", Type = "L", ReferenceName = "Guanyinqiao"},
                                 new {Id = "jit", Scope = "I", Type = "L", ReferenceName = "Jita"},
                                 new {Id = "jiu", Scope = "I", Type = "L", ReferenceName = "Youle Jinuo"},
                                 new {Id = "jiv", Scope = "I", Type = "L", ReferenceName = "Shuar"},
                                 new {Id = "jiy", Scope = "I", Type = "L", ReferenceName = "Buyuan Jinuo"},
                                 new {Id = "jje", Scope = "I", Type = "L", ReferenceName = "Jejueo"},
                                 new {Id = "jjr", Scope = "I", Type = "L", ReferenceName = "Bankal"},
                                 new {Id = "jka", Scope = "I", Type = "L", ReferenceName = "Kaera"},
                                 new {Id = "jkm", Scope = "I", Type = "L", ReferenceName = "Mobwa Karen"},
                                 new {Id = "jko", Scope = "I", Type = "L", ReferenceName = "Kubo"},
                                 new {Id = "jkp", Scope = "I", Type = "L", ReferenceName = "Paku Karen"},
                                 new {Id = "jkr", Scope = "I", Type = "L", ReferenceName = "Koro (India)"},
                                 new {Id = "jku", Scope = "I", Type = "L", ReferenceName = "Labir"},
                                 new {Id = "jle", Scope = "I", Type = "L", ReferenceName = "Ngile"},
                                 new
                                     {
                                         Id            = "jls",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Jamaican Sign Language"
                                     },
                                 new {Id = "jma", Scope = "I", Type = "L", ReferenceName = "Dima"},
                                 new {Id = "jmb", Scope = "I", Type = "L", ReferenceName = "Zumbun"},
                                 new {Id = "jmc", Scope = "I", Type = "L", ReferenceName = "Machame"},
                                 new {Id = "jmd", Scope = "I", Type = "L", ReferenceName = "Yamdena"},
                                 new {Id = "jmi", Scope = "I", Type = "L", ReferenceName = "Jimi (Nigeria)"},
                                 new {Id = "jml", Scope = "I", Type = "L", ReferenceName = "Jumli"},
                                 new {Id = "jmn", Scope = "I", Type = "L", ReferenceName = "Makuri Naga"},
                                 new {Id = "jmr", Scope = "I", Type = "L", ReferenceName = "Kamara"},
                                 new {Id = "jms", Scope = "I", Type = "L", ReferenceName = "Mashi (Nigeria)"},
                                 new {Id = "jmw", Scope = "I", Type = "L", ReferenceName = "Mouwase"},
                                 new
                                 {
                                     Id            = "jmx",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Western Juxtlahuaca Mixtec"
                                 }, new {Id = "jna", Scope = "I", Type = "L", ReferenceName = "Jangshung"},
                                 new {Id    = "jnd", Scope = "I", Type = "L", ReferenceName = "Jandavra"},
                                 new {Id    = "jng", Scope = "I", Type = "E", ReferenceName = "Yangman"},
                                 new {Id    = "jni", Scope = "I", Type = "L", ReferenceName = "Janji"},
                                 new {Id    = "jnj", Scope = "I", Type = "L", ReferenceName = "Yemsa"},
                                 new {Id    = "jnl", Scope = "I", Type = "L", ReferenceName = "Rawat"},
                                 new {Id    = "jns", Scope = "I", Type = "L", ReferenceName = "Jaunsari"},
                                 new {Id    = "job", Scope = "I", Type = "L", ReferenceName = "Joba"},
                                 new {Id    = "jod", Scope = "I", Type = "L", ReferenceName = "Wojenaka"},
                                 new {Id    = "jog", Scope = "I", Type = "L", ReferenceName = "Jogi"},
                                 new {Id    = "jor", Scope = "I", Type = "E", ReferenceName = "Jorá"},
                                 new
                                     {
                                         Id            = "jos",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Jordanian Sign Language"
                                     },
                                 new {Id = "jow", Scope = "I", Type = "L", ReferenceName = "Jowulu"},
                                 new
                                 {
                                     Id            = "jpa",
                                     Scope         = "I",
                                     Type          = "H",
                                     ReferenceName = "Jewish Palestinian Aramaic"
                                 }, new
                                 {
                                     Id            = "jpn",
                                     Part2B        = "jpn",
                                     Part2T        = "jpn",
                                     Part1         = "ja",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Japanese"
                                 }, new
                                 {
                                     Id            = "jpr",
                                     Part2B        = "jpr",
                                     Part2T        = "jpr",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Judeo-Persian"
                                 }, new {Id = "jqr", Scope = "I", Type = "L", ReferenceName = "Jaqaru"},
                                 new {Id    = "jra", Scope = "I", Type = "L", ReferenceName = "Jarai"},
                                 new
                                 {
                                     Id            = "jrb",
                                     Part2B        = "jrb",
                                     Part2T        = "jrb",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Judeo-Arabic"
                                 }, new {Id = "jrr", Scope = "I", Type = "L", ReferenceName = "Jiru"},
                                 new {Id    = "jrt", Scope = "I", Type = "L", ReferenceName = "Jorto"},
                                 new {Id    = "jru", Scope = "I", Type = "L", ReferenceName = "Japrería"},
                                 new {Id    = "jsl", Scope = "I", Type = "L", ReferenceName = "Japanese Sign Language"},
                                 new {Id    = "jua", Scope = "I", Type = "L", ReferenceName = "Júma"},
                                 new {Id    = "jub", Scope = "I", Type = "L", ReferenceName = "Wannu"},
                                 new {Id    = "juc", Scope = "I", Type = "E", ReferenceName = "Jurchen"},
                                 new {Id    = "jud", Scope = "I", Type = "L", ReferenceName = "Worodougou"},
                                 new {Id    = "juh", Scope = "I", Type = "L", ReferenceName = "Hõne"},
                                 new {Id    = "jui", Scope = "I", Type = "E", ReferenceName = "Ngadjuri"},
                                 new {Id    = "juk", Scope = "I", Type = "L", ReferenceName = "Wapan"},
                                 new {Id    = "jul", Scope = "I", Type = "L", ReferenceName = "Jirel"},
                                 new {Id    = "jum", Scope = "I", Type = "L", ReferenceName = "Jumjum"},
                                 new {Id    = "jun", Scope = "I", Type = "L", ReferenceName = "Juang"},
                                 new {Id    = "juo", Scope = "I", Type = "L", ReferenceName = "Jiba"},
                                 new {Id    = "jup", Scope = "I", Type = "L", ReferenceName = "Hupdë"},
                                 new {Id    = "jur", Scope = "I", Type = "L", ReferenceName = "Jurúna"},
                                 new {Id    = "jus", Scope = "I", Type = "L", ReferenceName = "Jumla Sign Language"},
                                 new {Id    = "jut", Scope = "I", Type = "H", ReferenceName = "Jutish"},
                                 new {Id    = "juu", Scope = "I", Type = "L", ReferenceName = "Ju"},
                                 new {Id    = "juw", Scope = "I", Type = "L", ReferenceName = "Wãpha"},
                                 new {Id    = "juy", Scope = "I", Type = "L", ReferenceName = "Juray"},
                                 new {Id    = "jvd", Scope = "I", Type = "L", ReferenceName = "Javindo"},
                                 new {Id    = "jvn", Scope = "I", Type = "L", ReferenceName = "Caribbean Javanese"},
                                 new {Id    = "jwi", Scope = "I", Type = "L", ReferenceName = "Jwira-Pepesa"},
                                 new {Id    = "jya", Scope = "I", Type = "L", ReferenceName = "Jiarong"},
                                 new {Id    = "jye", Scope = "I", Type = "L", ReferenceName = "Judeo-Yemeni Arabic"},
                                 new {Id    = "jyy", Scope = "I", Type = "L", ReferenceName = "Jaya"});
        }
    }
}