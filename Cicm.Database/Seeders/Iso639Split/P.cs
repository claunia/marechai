using Microsoft.EntityFrameworkCore;

namespace Cicm.Database.Seeders.Iso639Split
{
    public static class P
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Iso639>()
                        .HasData(new {Id = "pab", Scope = "I", Type = "L", ReferenceName = "Parecís"},
                                 new {Id = "pac", Scope = "I", Type = "L", ReferenceName = "Pacoh"},
                                 new {Id = "pad", Scope = "I", Type = "L", ReferenceName = "Paumarí"},
                                 new {Id = "pae", Scope = "I", Type = "L", ReferenceName = "Pagibete"},
                                 new {Id = "paf", Scope = "I", Type = "E", ReferenceName = "Paranawát"},
                                 new
                                 {
                                     Id            = "pag",
                                     Part2B        = "pag",
                                     Part2T        = "pag",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Pangasinan"
                                 }, new {Id = "pah", Scope = "I", Type = "L", ReferenceName = "Tenharim"},
                                 new {Id    = "pai", Scope = "I", Type = "L", ReferenceName = "Pe"},
                                 new {Id    = "pak", Scope = "I", Type = "L", ReferenceName = "Parakanã"},
                                 new
                                 {
                                     Id            = "pal",
                                     Part2B        = "pal",
                                     Part2T        = "pal",
                                     Scope         = "I",
                                     Type          = "A",
                                     ReferenceName = "Pahlavi"
                                 }, new
                                 {
                                     Id            = "pam",
                                     Part2B        = "pam",
                                     Part2T        = "pam",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Pampanga"
                                 }, new
                                 {
                                     Id            = "pan",
                                     Part2B        = "pan",
                                     Part2T        = "pan",
                                     Part1         = "pa",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Panjabi"
                                 }, new {Id = "pao", Scope = "I", Type = "L", ReferenceName = "Northern Paiute"},
                                 new
                                 {
                                     Id            = "pap",
                                     Part2B        = "pap",
                                     Part2T        = "pap",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Papiamento"
                                 }, new {Id = "paq", Scope = "I", Type = "L", ReferenceName = "Parya"},
                                 new {Id    = "par", Scope = "I", Type = "L", ReferenceName = "Panamint"},
                                 new {Id    = "pas", Scope = "I", Type = "L", ReferenceName = "Papasena"},
                                 new {Id    = "pat", Scope = "I", Type = "L", ReferenceName = "Papitalai"},
                                 new
                                 {
                                     Id            = "pau",
                                     Part2B        = "pau",
                                     Part2T        = "pau",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Palauan"
                                 }, new {Id = "pav", Scope = "I", Type = "L", ReferenceName = "Pakaásnovos"},
                                 new {Id    = "paw", Scope = "I", Type = "L", ReferenceName = "Pawnee"},
                                 new {Id    = "pax", Scope = "I", Type = "E", ReferenceName = "Pankararé"},
                                 new {Id    = "pay", Scope = "I", Type = "L", ReferenceName = "Pech"},
                                 new {Id    = "paz", Scope = "I", Type = "E", ReferenceName = "Pankararú"},
                                 new {Id    = "pbb", Scope = "I", Type = "L", ReferenceName = "Páez"},
                                 new {Id    = "pbc", Scope = "I", Type = "L", ReferenceName = "Patamona"},
                                 new {Id    = "pbe", Scope = "I", Type = "L", ReferenceName = "Mezontla Popoloca"},
                                 new {Id    = "pbf", Scope = "I", Type = "L", ReferenceName = "Coyotepec Popoloca"},
                                 new {Id    = "pbg", Scope = "I", Type = "E", ReferenceName = "Paraujano"},
                                 new {Id    = "pbh", Scope = "I", Type = "L", ReferenceName = "E'ñapa Woromaipu"},
                                 new {Id    = "pbi", Scope = "I", Type = "L", ReferenceName = "Parkwa"},
                                 new {Id    = "pbl", Scope = "I", Type = "L", ReferenceName = "Mak (Nigeria)"},
                                 new {Id    = "pbm", Scope = "I", Type = "L", ReferenceName = "Puebla Mazatec"},
                                 new {Id    = "pbn", Scope = "I", Type = "L", ReferenceName = "Kpasam"},
                                 new {Id    = "pbo", Scope = "I", Type = "L", ReferenceName = "Papel"},
                                 new {Id    = "pbp", Scope = "I", Type = "L", ReferenceName = "Badyara"},
                                 new {Id    = "pbr", Scope = "I", Type = "L", ReferenceName = "Pangwa"},
                                 new {Id    = "pbs", Scope = "I", Type = "L", ReferenceName = "Central Pame"},
                                 new {Id    = "pbt", Scope = "I", Type = "L", ReferenceName = "Southern Pashto"},
                                 new {Id    = "pbu", Scope = "I", Type = "L", ReferenceName = "Northern Pashto"},
                                 new {Id    = "pbv", Scope = "I", Type = "L", ReferenceName = "Pnar"},
                                 new {Id    = "pby", Scope = "I", Type = "L", ReferenceName = "Pyu (Papua New Guinea)"},
                                 new
                                 {
                                     Id            = "pca",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Santa Inés Ahuatempan Popoloca"
                                 }, new {Id = "pcb", Scope = "I", Type = "L", ReferenceName = "Pear"},
                                 new {Id    = "pcc", Scope = "I", Type = "L", ReferenceName = "Bouyei"},
                                 new {Id    = "pcd", Scope = "I", Type = "L", ReferenceName = "Picard"},
                                 new {Id    = "pce", Scope = "I", Type = "L", ReferenceName = "Ruching Palaung"},
                                 new {Id    = "pcf", Scope = "I", Type = "L", ReferenceName = "Paliyan"},
                                 new {Id    = "pcg", Scope = "I", Type = "L", ReferenceName = "Paniya"},
                                 new {Id    = "pch", Scope = "I", Type = "L", ReferenceName = "Pardhan"},
                                 new {Id    = "pci", Scope = "I", Type = "L", ReferenceName = "Duruwa"},
                                 new {Id    = "pcj", Scope = "I", Type = "L", ReferenceName = "Parenga"},
                                 new {Id    = "pck", Scope = "I", Type = "L", ReferenceName = "Paite Chin"},
                                 new {Id    = "pcl", Scope = "I", Type = "L", ReferenceName = "Pardhi"},
                                 new {Id    = "pcm", Scope = "I", Type = "L", ReferenceName = "Nigerian Pidgin"},
                                 new {Id    = "pcn", Scope = "I", Type = "L", ReferenceName = "Piti"},
                                 new {Id    = "pcp", Scope = "I", Type = "L", ReferenceName = "Pacahuara"},
                                 new {Id    = "pcw", Scope = "I", Type = "L", ReferenceName = "Pyapun"},
                                 new {Id    = "pda", Scope = "I", Type = "L", ReferenceName = "Anam"},
                                 new
                                     {
                                         Id            = "pdc",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Pennsylvania German"
                                     },
                                 new {Id = "pdi", Scope = "I", Type = "L", ReferenceName = "Pa Di"},
                                 new {Id = "pdn", Scope = "I", Type = "L", ReferenceName = "Podena"},
                                 new {Id = "pdo", Scope = "I", Type = "L", ReferenceName = "Padoe"},
                                 new {Id = "pdt", Scope = "I", Type = "L", ReferenceName = "Plautdietsch"},
                                 new {Id = "pdu", Scope = "I", Type = "L", ReferenceName = "Kayan"},
                                 new
                                     {
                                         Id            = "pea",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Peranakan Indonesian"
                                     },
                                 new {Id = "peb", Scope = "I", Type = "E", ReferenceName = "Eastern Pomo"},
                                 new
                                     {
                                         Id            = "ped",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Mala (Papua New Guinea)"
                                     },
                                 new {Id = "pee", Scope = "I", Type = "L", ReferenceName = "Taje"},
                                 new {Id = "pef", Scope = "I", Type = "E", ReferenceName = "Northeastern Pomo"},
                                 new {Id = "peg", Scope = "I", Type = "L", ReferenceName = "Pengo"},
                                 new {Id = "peh", Scope = "I", Type = "L", ReferenceName = "Bonan"},
                                 new {Id = "pei", Scope = "I", Type = "L", ReferenceName = "Chichimeca-Jonaz"},
                                 new {Id = "pej", Scope = "I", Type = "E", ReferenceName = "Northern Pomo"},
                                 new {Id = "pek", Scope = "I", Type = "L", ReferenceName = "Penchal"},
                                 new {Id = "pel", Scope = "I", Type = "L", ReferenceName = "Pekal"},
                                 new {Id = "pem", Scope = "I", Type = "L", ReferenceName = "Phende"},
                                 new
                                 {
                                     Id            = "peo",
                                     Part2B        = "peo",
                                     Part2T        = "peo",
                                     Scope         = "I",
                                     Type          = "H",
                                     ReferenceName = "Old Persian (ca. 600-400 B.C.)"
                                 }, new {Id = "pep", Scope = "I", Type = "L", ReferenceName = "Kunja"},
                                 new {Id    = "peq", Scope = "I", Type = "L", ReferenceName = "Southern Pomo"},
                                 new {Id    = "pes", Scope = "I", Type = "L", ReferenceName = "Iranian Persian"},
                                 new {Id    = "pev", Scope = "I", Type = "L", ReferenceName = "Pémono"},
                                 new {Id    = "pex", Scope = "I", Type = "L", ReferenceName = "Petats"},
                                 new {Id    = "pey", Scope = "I", Type = "L", ReferenceName = "Petjo"},
                                 new {Id    = "pez", Scope = "I", Type = "L", ReferenceName = "Eastern Penan"},
                                 new {Id    = "pfa", Scope = "I", Type = "L", ReferenceName = "Pááfang"},
                                 new {Id    = "pfe", Scope = "I", Type = "L", ReferenceName = "Peere"},
                                 new {Id    = "pfl", Scope = "I", Type = "L", ReferenceName = "Pfaelzisch"},
                                 new {Id    = "pga", Scope = "I", Type = "L", ReferenceName = "Sudanese Creole Arabic"},
                                 new {Id    = "pgd", Scope = "I", Type = "H", ReferenceName = "Gāndhārī"},
                                 new {Id    = "pgg", Scope = "I", Type = "L", ReferenceName = "Pangwali"},
                                 new {Id    = "pgi", Scope = "I", Type = "L", ReferenceName = "Pagi"},
                                 new {Id    = "pgk", Scope = "I", Type = "L", ReferenceName = "Rerep"},
                                 new {Id    = "pgl", Scope = "I", Type = "A", ReferenceName = "Primitive Irish"},
                                 new {Id    = "pgn", Scope = "I", Type = "A", ReferenceName = "Paelignian"},
                                 new {Id    = "pgs", Scope = "I", Type = "L", ReferenceName = "Pangseng"},
                                 new {Id    = "pgu", Scope = "I", Type = "L", ReferenceName = "Pagu"},
                                 new
                                 {
                                     Id            = "pgz",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Papua New Guinean Sign Language"
                                 }, new {Id = "pha", Scope = "I", Type = "L", ReferenceName = "Pa-Hng"},
                                 new {Id    = "phd", Scope = "I", Type = "L", ReferenceName = "Phudagi"},
                                 new {Id    = "phg", Scope = "I", Type = "L", ReferenceName = "Phuong"},
                                 new {Id    = "phh", Scope = "I", Type = "L", ReferenceName = "Phukha"},
                                 new {Id    = "phk", Scope = "I", Type = "L", ReferenceName = "Phake"},
                                 new {Id    = "phl", Scope = "I", Type = "L", ReferenceName = "Phalura"},
                                 new {Id    = "phm", Scope = "I", Type = "L", ReferenceName = "Phimbi"},
                                 new
                                 {
                                     Id            = "phn",
                                     Part2B        = "phn",
                                     Part2T        = "phn",
                                     Scope         = "I",
                                     Type          = "A",
                                     ReferenceName = "Phoenician"
                                 }, new {Id = "pho", Scope = "I", Type = "L", ReferenceName = "Phunoi"},
                                 new {Id    = "phq", Scope = "I", Type = "L", ReferenceName = "Phana'"},
                                 new {Id    = "phr", Scope = "I", Type = "L", ReferenceName = "Pahari-Potwari"},
                                 new {Id    = "pht", Scope = "I", Type = "L", ReferenceName = "Phu Thai"},
                                 new {Id    = "phu", Scope = "I", Type = "L", ReferenceName = "Phuan"},
                                 new {Id    = "phv", Scope = "I", Type = "L", ReferenceName = "Pahlavani"},
                                 new {Id    = "phw", Scope = "I", Type = "L", ReferenceName = "Phangduwali"},
                                 new {Id    = "pia", Scope = "I", Type = "L", ReferenceName = "Pima Bajo"},
                                 new {Id    = "pib", Scope = "I", Type = "L", ReferenceName = "Yine"},
                                 new {Id    = "pic", Scope = "I", Type = "L", ReferenceName = "Pinji"},
                                 new {Id    = "pid", Scope = "I", Type = "L", ReferenceName = "Piaroa"},
                                 new {Id    = "pie", Scope = "I", Type = "E", ReferenceName = "Piro"},
                                 new {Id    = "pif", Scope = "I", Type = "L", ReferenceName = "Pingelapese"},
                                 new {Id    = "pig", Scope = "I", Type = "L", ReferenceName = "Pisabo"},
                                 new {Id    = "pih", Scope = "I", Type = "L", ReferenceName = "Pitcairn-Norfolk"},
                                 new {Id    = "pii", Scope = "I", Type = "L", ReferenceName = "Pini"},
                                 new {Id    = "pij", Scope = "I", Type = "E", ReferenceName = "Pijao"},
                                 new {Id    = "pil", Scope = "I", Type = "L", ReferenceName = "Yom"},
                                 new {Id    = "pim", Scope = "I", Type = "E", ReferenceName = "Powhatan"},
                                 new {Id    = "pin", Scope = "I", Type = "L", ReferenceName = "Piame"},
                                 new {Id    = "pio", Scope = "I", Type = "L", ReferenceName = "Piapoco"},
                                 new {Id    = "pip", Scope = "I", Type = "L", ReferenceName = "Pero"},
                                 new {Id    = "pir", Scope = "I", Type = "L", ReferenceName = "Piratapuyo"},
                                 new {Id    = "pis", Scope = "I", Type = "L", ReferenceName = "Pijin"},
                                 new {Id    = "pit", Scope = "I", Type = "E", ReferenceName = "Pitta Pitta"},
                                 new {Id    = "piu", Scope = "I", Type = "L", ReferenceName = "Pintupi-Luritja"},
                                 new {Id    = "piv", Scope = "I", Type = "L", ReferenceName = "Pileni"},
                                 new {Id    = "piw", Scope = "I", Type = "L", ReferenceName = "Pimbwe"},
                                 new {Id    = "pix", Scope = "I", Type = "L", ReferenceName = "Piu"},
                                 new {Id    = "piy", Scope = "I", Type = "L", ReferenceName = "Piya-Kwonci"},
                                 new {Id    = "piz", Scope = "I", Type = "L", ReferenceName = "Pije"},
                                 new {Id    = "pjt", Scope = "I", Type = "L", ReferenceName = "Pitjantjatjara"},
                                 new {Id    = "pka", Scope = "I", Type = "H", ReferenceName = "Ardhamāgadhī Prākrit"},
                                 new {Id    = "pkb", Scope = "I", Type = "L", ReferenceName = "Pokomo"},
                                 new {Id    = "pkc", Scope = "I", Type = "A", ReferenceName = "Paekche"},
                                 new {Id    = "pkg", Scope = "I", Type = "L", ReferenceName = "Pak-Tong"},
                                 new {Id    = "pkh", Scope = "I", Type = "L", ReferenceName = "Pankhu"},
                                 new {Id    = "pkn", Scope = "I", Type = "L", ReferenceName = "Pakanha"},
                                 new {Id    = "pko", Scope = "I", Type = "L", ReferenceName = "Pökoot"},
                                 new {Id    = "pkp", Scope = "I", Type = "L", ReferenceName = "Pukapuka"},
                                 new {Id    = "pkr", Scope = "I", Type = "L", ReferenceName = "Attapady Kurumba"},
                                 new {Id    = "pks", Scope = "I", Type = "L", ReferenceName = "Pakistan Sign Language"},
                                 new {Id    = "pkt", Scope = "I", Type = "L", ReferenceName = "Maleng"},
                                 new {Id    = "pku", Scope = "I", Type = "L", ReferenceName = "Paku"},
                                 new {Id    = "pla", Scope = "I", Type = "L", ReferenceName = "Miani"},
                                 new {Id    = "plb", Scope = "I", Type = "L", ReferenceName = "Polonombauk"},
                                 new {Id    = "plc", Scope = "I", Type = "L", ReferenceName = "Central Palawano"},
                                 new {Id    = "pld", Scope = "I", Type = "L", ReferenceName = "Polari"},
                                 new {Id    = "ple", Scope = "I", Type = "L", ReferenceName = "Palu'e"},
                                 new {Id    = "plg", Scope = "I", Type = "L", ReferenceName = "Pilagá"},
                                 new {Id    = "plh", Scope = "I", Type = "L", ReferenceName = "Paulohi"},
                                 new
                                 {
                                     Id            = "pli",
                                     Part2B        = "pli",
                                     Part2T        = "pli",
                                     Part1         = "pi",
                                     Scope         = "I",
                                     Type          = "A",
                                     ReferenceName = "Pali"
                                 }, new {Id = "plj", Scope = "I", Type = "L", ReferenceName = "Polci"},
                                 new {Id    = "plk", Scope = "I", Type = "L", ReferenceName = "Kohistani Shina"},
                                 new {Id    = "pll", Scope = "I", Type = "L", ReferenceName = "Shwe Palaung"},
                                 new {Id    = "pln", Scope = "I", Type = "L", ReferenceName = "Palenquero"},
                                 new {Id    = "plo", Scope = "I", Type = "L", ReferenceName = "Oluta Popoluca"},
                                 new {Id    = "plp", Scope = "I", Type = "L", ReferenceName = "Palpa"},
                                 new {Id    = "plq", Scope = "I", Type = "A", ReferenceName = "Palaic"},
                                 new {Id    = "plr", Scope = "I", Type = "L", ReferenceName = "Palaka Senoufo"},
                                 new
                                 {
                                     Id            = "pls",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "San Marcos Tlacoyalco Popoloca"
                                 }, new {Id = "plt", Scope = "I", Type = "L", ReferenceName = "Plateau Malagasy"},
                                 new {Id    = "plu", Scope = "I", Type = "L", ReferenceName = "Palikúr"},
                                 new {Id    = "plv", Scope = "I", Type = "L", ReferenceName = "Southwest Palawano"},
                                 new
                                     {
                                         Id            = "plw",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Brooke's Point Palawano"
                                     },
                                 new {Id = "ply", Scope = "I", Type = "L", ReferenceName = "Bolyu"},
                                 new {Id = "plz", Scope = "I", Type = "L", ReferenceName = "Paluan"},
                                 new {Id = "pma", Scope = "I", Type = "L", ReferenceName = "Paama"},
                                 new {Id = "pmb", Scope = "I", Type = "L", ReferenceName = "Pambia"},
                                 new {Id = "pmd", Scope = "I", Type = "E", ReferenceName = "Pallanganmiddang"},
                                 new {Id = "pme", Scope = "I", Type = "L", ReferenceName = "Pwaamei"},
                                 new {Id = "pmf", Scope = "I", Type = "L", ReferenceName = "Pamona"},
                                 new {Id = "pmh", Scope = "I", Type = "H", ReferenceName = "Māhārāṣṭri Prākrit"},
                                 new {Id = "pmi", Scope = "I", Type = "L", ReferenceName = "Northern Pumi"},
                                 new {Id = "pmj", Scope = "I", Type = "L", ReferenceName = "Southern Pumi"},
                                 new {Id = "pmk", Scope = "I", Type = "E", ReferenceName = "Pamlico"},
                                 new {Id = "pml", Scope = "I", Type = "E", ReferenceName = "Lingua Franca"},
                                 new {Id = "pmm", Scope = "I", Type = "L", ReferenceName = "Pomo"},
                                 new {Id = "pmn", Scope = "I", Type = "L", ReferenceName = "Pam"},
                                 new {Id = "pmo", Scope = "I", Type = "L", ReferenceName = "Pom"},
                                 new {Id = "pmq", Scope = "I", Type = "L", ReferenceName = "Northern Pame"},
                                 new {Id = "pmr", Scope = "I", Type = "L", ReferenceName = "Paynamar"},
                                 new {Id = "pms", Scope = "I", Type = "L", ReferenceName = "Piemontese"},
                                 new {Id = "pmt", Scope = "I", Type = "L", ReferenceName = "Tuamotuan"},
                                 new {Id = "pmw", Scope = "I", Type = "L", ReferenceName = "Plains Miwok"},
                                 new {Id = "pmx", Scope = "I", Type = "L", ReferenceName = "Poumei Naga"},
                                 new {Id = "pmy", Scope = "I", Type = "L", ReferenceName = "Papuan Malay"},
                                 new {Id = "pmz", Scope = "I", Type = "E", ReferenceName = "Southern Pame"},
                                 new {Id = "pna", Scope = "I", Type = "L", ReferenceName = "Punan Bah-Biau"},
                                 new {Id = "pnb", Scope = "I", Type = "L", ReferenceName = "Western Panjabi"},
                                 new {Id = "pnc", Scope = "I", Type = "L", ReferenceName = "Pannei"},
                                 new {Id = "pnd", Scope = "I", Type = "L", ReferenceName = "Mpinda"},
                                 new {Id = "pne", Scope = "I", Type = "L", ReferenceName = "Western Penan"},
                                 new {Id = "png", Scope = "I", Type = "L", ReferenceName = "Pongu"},
                                 new {Id = "pnh", Scope = "I", Type = "L", ReferenceName = "Penrhyn"},
                                 new {Id = "pni", Scope = "I", Type = "L", ReferenceName = "Aoheng"},
                                 new {Id = "pnj", Scope = "I", Type = "E", ReferenceName = "Pinjarup"},
                                 new {Id = "pnk", Scope = "I", Type = "L", ReferenceName = "Paunaka"},
                                 new {Id = "pnl", Scope = "I", Type = "L", ReferenceName = "Paleni"},
                                 new {Id = "pnm", Scope = "I", Type = "L", ReferenceName = "Punan Batu 1"},
                                 new {Id = "pnn", Scope = "I", Type = "L", ReferenceName = "Pinai-Hagahai"},
                                 new {Id = "pno", Scope = "I", Type = "E", ReferenceName = "Panobo"},
                                 new {Id = "pnp", Scope = "I", Type = "L", ReferenceName = "Pancana"},
                                 new
                                     {
                                         Id            = "pnq",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Pana (Burkina Faso)"
                                     },
                                 new {Id = "pnr", Scope = "I", Type = "L", ReferenceName = "Panim"},
                                 new {Id = "pns", Scope = "I", Type = "L", ReferenceName = "Ponosakan"},
                                 new {Id = "pnt", Scope = "I", Type = "L", ReferenceName = "Pontic"},
                                 new {Id = "pnu", Scope = "I", Type = "L", ReferenceName = "Jiongnai Bunu"},
                                 new {Id = "pnv", Scope = "I", Type = "L", ReferenceName = "Pinigura"},
                                 new {Id = "pnw", Scope = "I", Type = "L", ReferenceName = "Banyjima"},
                                 new {Id = "pnx", Scope = "I", Type = "L", ReferenceName = "Phong-Kniang"},
                                 new {Id = "pny", Scope = "I", Type = "L", ReferenceName = "Pinyin"},
                                 new
                                 {
                                     Id            = "pnz",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Pana (Central African Republic)"
                                 }, new {Id = "poc", Scope = "I", Type = "L", ReferenceName = "Poqomam"},
                                 new
                                     {
                                         Id            = "poe",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "San Juan Atzingo Popoloca"
                                     },
                                 new {Id = "pof", Scope = "I", Type = "L", ReferenceName = "Poke"},
                                 new {Id = "pog", Scope = "I", Type = "E", ReferenceName = "Potiguára"},
                                 new {Id = "poh", Scope = "I", Type = "L", ReferenceName = "Poqomchi'"},
                                 new {Id = "poi", Scope = "I", Type = "L", ReferenceName = "Highland Popoluca"},
                                 new {Id = "pok", Scope = "I", Type = "L", ReferenceName = "Pokangá"},
                                 new
                                 {
                                     Id            = "pol",
                                     Part2B        = "pol",
                                     Part2T        = "pol",
                                     Part1         = "pl",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Polish"
                                 }, new {Id = "pom", Scope = "I", Type = "L", ReferenceName = "Southeastern Pomo"},
                                 new
                                 {
                                     Id            = "pon",
                                     Part2B        = "pon",
                                     Part2T        = "pon",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Pohnpeian"
                                 }, new {Id = "poo", Scope = "I", Type = "E", ReferenceName = "Central Pomo"},
                                 new {Id    = "pop", Scope = "I", Type = "L", ReferenceName = "Pwapwâ"},
                                 new {Id    = "poq", Scope = "I", Type = "L", ReferenceName = "Texistepec Popoluca"},
                                 new
                                 {
                                     Id            = "por",
                                     Part2B        = "por",
                                     Part2T        = "por",
                                     Part1         = "pt",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Portuguese"
                                 }, new {Id = "pos", Scope = "I", Type = "L", ReferenceName = "Sayula Popoluca"},
                                 new {Id    = "pot", Scope = "I", Type = "L", ReferenceName = "Potawatomi"},
                                 new {Id    = "pov", Scope = "I", Type = "L", ReferenceName = "Upper Guinea Crioulo"},
                                 new
                                 {
                                     Id            = "pow",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "San Felipe Otlaltepec Popoloca"
                                 }, new {Id = "pox", Scope = "I", Type = "E", ReferenceName = "Polabian"},
                                 new {Id    = "poy", Scope = "I", Type = "L", ReferenceName = "Pogolo"},
                                 new {Id    = "ppe", Scope = "I", Type = "L", ReferenceName = "Papi"},
                                 new {Id    = "ppi", Scope = "I", Type = "L", ReferenceName = "Paipai"},
                                 new {Id    = "ppk", Scope = "I", Type = "L", ReferenceName = "Uma"},
                                 new {Id    = "ppl", Scope = "I", Type = "L", ReferenceName = "Pipil"},
                                 new {Id    = "ppm", Scope = "I", Type = "L", ReferenceName = "Papuma"},
                                 new {Id    = "ppn", Scope = "I", Type = "L", ReferenceName = "Papapana"},
                                 new {Id    = "ppo", Scope = "I", Type = "L", ReferenceName = "Folopa"},
                                 new {Id    = "ppp", Scope = "I", Type = "L", ReferenceName = "Pelende"},
                                 new {Id    = "ppq", Scope = "I", Type = "L", ReferenceName = "Pei"},
                                 new
                                 {
                                     Id            = "pps",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "San Luís Temalacayuca Popoloca"
                                 }, new {Id = "ppt", Scope = "I", Type = "L", ReferenceName = "Pare"},
                                 new {Id    = "ppu", Scope = "I", Type = "E", ReferenceName = "Papora"},
                                 new {Id    = "pqa", Scope = "I", Type = "L", ReferenceName = "Pa'a"},
                                 new
                                     {
                                         Id            = "pqm",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Malecite-Passamaquoddy"
                                     },
                                 new {Id = "prc", Scope = "I", Type = "L", ReferenceName = "Parachi"},
                                 new {Id = "prd", Scope = "I", Type = "L", ReferenceName = "Parsi-Dari"},
                                 new {Id = "pre", Scope = "I", Type = "L", ReferenceName = "Principense"},
                                 new {Id = "prf", Scope = "I", Type = "L", ReferenceName = "Paranan"},
                                 new {Id = "prg", Scope = "I", Type = "L", ReferenceName = "Prussian"},
                                 new {Id = "prh", Scope = "I", Type = "L", ReferenceName = "Porohanon"},
                                 new {Id = "pri", Scope = "I", Type = "L", ReferenceName = "Paicî"},
                                 new {Id = "prk", Scope = "I", Type = "L", ReferenceName = "Parauk"},
                                 new
                                     {
                                         Id            = "prl",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Peruvian Sign Language"
                                     },
                                 new {Id = "prm", Scope = "I", Type = "L", ReferenceName = "Kibiri"},
                                 new {Id = "prn", Scope = "I", Type = "L", ReferenceName = "Prasuni"},
                                 new
                                 {
                                     Id            = "pro",
                                     Part2B        = "pro",
                                     Part2T        = "pro",
                                     Scope         = "I",
                                     Type          = "H",
                                     ReferenceName = "Old Provençal (to 1500)"
                                 }, new {Id = "prp", Scope = "I", Type = "L", ReferenceName = "Parsi"},
                                 new {Id    = "prq", Scope = "I", Type = "L", ReferenceName = "Ashéninka Perené"},
                                 new {Id    = "prr", Scope = "I", Type = "E", ReferenceName = "Puri"},
                                 new {Id    = "prs", Scope = "I", Type = "L", ReferenceName = "Dari"},
                                 new {Id    = "prt", Scope = "I", Type = "L", ReferenceName = "Phai"},
                                 new {Id    = "pru", Scope = "I", Type = "L", ReferenceName = "Puragi"},
                                 new {Id    = "prw", Scope = "I", Type = "L", ReferenceName = "Parawen"},
                                 new {Id    = "prx", Scope = "I", Type = "L", ReferenceName = "Purik"},
                                 new
                                     {
                                         Id            = "prz",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Providencia Sign Language"
                                     },
                                 new {Id = "psa", Scope = "I", Type = "L", ReferenceName = "Asue Awyu"},
                                 new {Id = "psc", Scope = "I", Type = "L", ReferenceName = "Persian Sign Language"},
                                 new
                                 {
                                     Id            = "psd",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Plains Indian Sign Language"
                                 }, new {Id = "pse", Scope = "I", Type = "L", ReferenceName = "Central Malay"},
                                 new
                                     {
                                         Id            = "psg",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Penang Sign Language"
                                     },
                                 new {Id = "psh", Scope = "I", Type = "L", ReferenceName = "Southwest Pashai"},
                                 new {Id = "psi", Scope = "I", Type = "L", ReferenceName = "Southeast Pashai"},
                                 new
                                 {
                                     Id            = "psl",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Puerto Rican Sign Language"
                                 }, new {Id = "psm", Scope = "I", Type = "E", ReferenceName = "Pauserna"},
                                 new {Id    = "psn", Scope = "I", Type = "L", ReferenceName = "Panasuan"},
                                 new
                                     {
                                         Id            = "pso",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Polish Sign Language"
                                     },
                                 new
                                     {
                                         Id            = "psp",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Philippine Sign Language"
                                     },
                                 new {Id = "psq", Scope = "I", Type = "L", ReferenceName = "Pasi"},
                                 new
                                     {
                                         Id            = "psr",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Portuguese Sign Language"
                                     },
                                 new {Id = "pss", Scope = "I", Type = "L", ReferenceName = "Kaulong"},
                                 new {Id = "pst", Scope = "I", Type = "L", ReferenceName = "Central Pashto"},
                                 new {Id = "psu", Scope = "I", Type = "H", ReferenceName = "Sauraseni Prākrit"},
                                 new {Id = "psw", Scope = "I", Type = "L", ReferenceName = "Port Sandwich"},
                                 new {Id = "psy", Scope = "I", Type = "E", ReferenceName = "Piscataway"},
                                 new {Id = "pta", Scope = "I", Type = "L", ReferenceName = "Pai Tavytera"},
                                 new {Id = "pth", Scope = "I", Type = "E", ReferenceName = "Pataxó Hã-Ha-Hãe"},
                                 new {Id = "pti", Scope = "I", Type = "L", ReferenceName = "Pindiini"},
                                 new {Id = "ptn", Scope = "I", Type = "L", ReferenceName = "Patani"},
                                 new {Id = "pto", Scope = "I", Type = "L", ReferenceName = "Zo'é"},
                                 new {Id = "ptp", Scope = "I", Type = "L", ReferenceName = "Patep"},
                                 new {Id = "ptq", Scope = "I", Type = "L", ReferenceName = "Pattapu"},
                                 new {Id = "ptr", Scope = "I", Type = "L", ReferenceName = "Piamatsina"},
                                 new {Id = "ptt", Scope = "I", Type = "L", ReferenceName = "Enrekang"},
                                 new {Id = "ptu", Scope = "I", Type = "L", ReferenceName = "Bambam"},
                                 new {Id = "ptv", Scope = "I", Type = "L", ReferenceName = "Port Vato"},
                                 new {Id = "ptw", Scope = "I", Type = "E", ReferenceName = "Pentlatch"},
                                 new {Id = "pty", Scope = "I", Type = "L", ReferenceName = "Pathiya"},
                                 new
                                 {
                                     Id            = "pua",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Western Highland Purepecha"
                                 }, new {Id = "pub", Scope = "I", Type = "L", ReferenceName = "Purum"},
                                 new {Id    = "puc", Scope = "I", Type = "L", ReferenceName = "Punan Merap"},
                                 new {Id    = "pud", Scope = "I", Type = "L", ReferenceName = "Punan Aput"},
                                 new {Id    = "pue", Scope = "I", Type = "E", ReferenceName = "Puelche"},
                                 new {Id    = "puf", Scope = "I", Type = "L", ReferenceName = "Punan Merah"},
                                 new {Id    = "pug", Scope = "I", Type = "L", ReferenceName = "Phuie"},
                                 new {Id    = "pui", Scope = "I", Type = "L", ReferenceName = "Puinave"},
                                 new {Id    = "puj", Scope = "I", Type = "L", ReferenceName = "Punan Tubu"},
                                 new {Id    = "pum", Scope = "I", Type = "L", ReferenceName = "Puma"},
                                 new {Id    = "puo", Scope = "I", Type = "L", ReferenceName = "Puoc"},
                                 new {Id    = "pup", Scope = "I", Type = "L", ReferenceName = "Pulabu"},
                                 new {Id    = "puq", Scope = "I", Type = "E", ReferenceName = "Puquina"},
                                 new {Id    = "pur", Scope = "I", Type = "L", ReferenceName = "Puruborá"},
                                 new
                                 {
                                     Id            = "pus",
                                     Part2B        = "pus",
                                     Part2T        = "pus",
                                     Part1         = "ps",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Pushto"
                                 }, new {Id = "put", Scope = "I", Type = "L", ReferenceName = "Putoh"},
                                 new {Id    = "puu", Scope = "I", Type = "L", ReferenceName = "Punu"},
                                 new {Id    = "puw", Scope = "I", Type = "L", ReferenceName = "Puluwatese"},
                                 new {Id    = "pux", Scope = "I", Type = "L", ReferenceName = "Puare"},
                                 new {Id    = "puy", Scope = "I", Type = "E", ReferenceName = "Purisimeño"},
                                 new {Id    = "pwa", Scope = "I", Type = "L", ReferenceName = "Pawaia"},
                                 new {Id    = "pwb", Scope = "I", Type = "L", ReferenceName = "Panawa"},
                                 new {Id    = "pwg", Scope = "I", Type = "L", ReferenceName = "Gapapaiwa"},
                                 new {Id    = "pwi", Scope = "I", Type = "E", ReferenceName = "Patwin"},
                                 new {Id    = "pwm", Scope = "I", Type = "L", ReferenceName = "Molbog"},
                                 new {Id    = "pwn", Scope = "I", Type = "L", ReferenceName = "Paiwan"},
                                 new {Id    = "pwo", Scope = "I", Type = "L", ReferenceName = "Pwo Western Karen"},
                                 new {Id    = "pwr", Scope = "I", Type = "L", ReferenceName = "Powari"},
                                 new {Id    = "pww", Scope = "I", Type = "L", ReferenceName = "Pwo Northern Karen"},
                                 new {Id    = "pxm", Scope = "I", Type = "L", ReferenceName = "Quetzaltepec Mixe"},
                                 new {Id    = "pye", Scope = "I", Type = "L", ReferenceName = "Pye Krumen"},
                                 new {Id    = "pym", Scope = "I", Type = "L", ReferenceName = "Fyam"},
                                 new {Id    = "pyn", Scope = "I", Type = "L", ReferenceName = "Poyanáwa"},
                                 new
                                     {
                                         Id            = "pys",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Paraguayan Sign Language"
                                     },
                                 new {Id = "pyu", Scope = "I", Type = "L", ReferenceName = "Puyuma"},
                                 new {Id = "pyx", Scope = "I", Type = "A", ReferenceName = "Pyu (Myanmar)"},
                                 new {Id = "pyy", Scope = "I", Type = "L", ReferenceName = "Pyen"},
                                 new {Id = "pzn", Scope = "I", Type = "L", ReferenceName = "Para Naga"});
        }
    }
}