using Microsoft.EntityFrameworkCore;

namespace Cicm.Database.Seeders.Iso639Split
{
    public static class I
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Iso639>()
                        .HasData(new {Id = "iai", Scope = "I", Type = "L", ReferenceName = "Iaai"},
                                 new {Id = "ian", Scope = "I", Type = "L", ReferenceName = "Iatmul"},
                                 new {Id = "iar", Scope = "I", Type = "L", ReferenceName = "Purari"},
                                 new
                                 {
                                     Id            = "iba",
                                     Part2B        = "iba",
                                     Part2T        = "iba",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Iban"
                                 }, new {Id = "ibb", Scope = "I", Type = "L", ReferenceName = "Ibibio"},
                                 new {Id    = "ibd", Scope = "I", Type = "L", ReferenceName = "Iwaidja"},
                                 new {Id    = "ibe", Scope = "I", Type = "L", ReferenceName = "Akpes"},
                                 new {Id    = "ibg", Scope = "I", Type = "L", ReferenceName = "Ibanag"},
                                 new {Id    = "ibh", Scope = "I", Type = "L", ReferenceName = "Bih"},
                                 new {Id    = "ibl", Scope = "I", Type = "L", ReferenceName = "Ibaloi"},
                                 new {Id    = "ibm", Scope = "I", Type = "L", ReferenceName = "Agoi"},
                                 new {Id    = "ibn", Scope = "I", Type = "L", ReferenceName = "Ibino"},
                                 new
                                 {
                                     Id            = "ibo",
                                     Part2B        = "ibo",
                                     Part2T        = "ibo",
                                     Part1         = "ig",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Igbo"
                                 }, new {Id = "ibr", Scope = "I", Type = "L", ReferenceName = "Ibuoro"},
                                 new {Id    = "ibu", Scope = "I", Type = "L", ReferenceName = "Ibu"},
                                 new {Id    = "iby", Scope = "I", Type = "L", ReferenceName = "Ibani"},
                                 new {Id    = "ica", Scope = "I", Type = "L", ReferenceName = "Ede Ica"},
                                 new {Id    = "ich", Scope = "I", Type = "L", ReferenceName = "Etkywan"},
                                 new
                                     {
                                         Id            = "icl",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Icelandic Sign Language"
                                     },
                                 new
                                     {
                                         Id            = "icr",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Islander Creole English"
                                     },
                                 new {Id = "ida", Scope = "I", Type = "L", ReferenceName = "Idakho-Isukha-Tiriki"},
                                 new {Id = "idb", Scope = "I", Type = "L", ReferenceName = "Indo-Portuguese"},
                                 new {Id = "idc", Scope = "I", Type = "L", ReferenceName = "Idon"},
                                 new {Id = "idd", Scope = "I", Type = "L", ReferenceName = "Ede Idaca"},
                                 new {Id = "ide", Scope = "I", Type = "L", ReferenceName = "Idere"},
                                 new {Id = "idi", Scope = "I", Type = "L", ReferenceName = "Idi"},
                                 new
                                 {
                                     Id            = "ido",
                                     Part2B        = "ido",
                                     Part2T        = "ido",
                                     Part1         = "io",
                                     Scope         = "I",
                                     Type          = "C",
                                     ReferenceName = "Ido"
                                 }, new {Id = "idr", Scope = "I", Type = "L", ReferenceName = "Indri"},
                                 new {Id    = "ids", Scope = "I", Type = "L", ReferenceName = "Idesa"},
                                 new {Id    = "idt", Scope = "I", Type = "L", ReferenceName = "Idaté"},
                                 new {Id    = "idu", Scope = "I", Type = "L", ReferenceName = "Idoma"},
                                 new {Id    = "ifa", Scope = "I", Type = "L", ReferenceName = "Amganad Ifugao"},
                                 new {Id    = "ifb", Scope = "I", Type = "L", ReferenceName = "Batad Ifugao"},
                                 new {Id    = "ife", Scope = "I", Type = "L", ReferenceName = "Ifè"},
                                 new {Id    = "iff", Scope = "I", Type = "E", ReferenceName = "Ifo"},
                                 new {Id    = "ifk", Scope = "I", Type = "L", ReferenceName = "Tuwali Ifugao"},
                                 new {Id    = "ifm", Scope = "I", Type = "L", ReferenceName = "Teke-Fuumu"},
                                 new {Id    = "ifu", Scope = "I", Type = "L", ReferenceName = "Mayoyao Ifugao"},
                                 new {Id    = "ify", Scope = "I", Type = "L", ReferenceName = "Keley-I Kallahan"},
                                 new {Id    = "igb", Scope = "I", Type = "L", ReferenceName = "Ebira"},
                                 new {Id    = "ige", Scope = "I", Type = "L", ReferenceName = "Igede"},
                                 new {Id    = "igg", Scope = "I", Type = "L", ReferenceName = "Igana"},
                                 new {Id    = "igl", Scope = "I", Type = "L", ReferenceName = "Igala"},
                                 new {Id    = "igm", Scope = "I", Type = "L", ReferenceName = "Kanggape"},
                                 new {Id    = "ign", Scope = "I", Type = "L", ReferenceName = "Ignaciano"},
                                 new {Id    = "igo", Scope = "I", Type = "L", ReferenceName = "Isebe"},
                                 new {Id    = "igs", Scope = "I", Type = "C", ReferenceName = "Interglossa"},
                                 new {Id    = "igw", Scope = "I", Type = "L", ReferenceName = "Igwe"},
                                 new {Id    = "ihb", Scope = "I", Type = "L", ReferenceName = "Iha Based Pidgin"},
                                 new {Id    = "ihi", Scope = "I", Type = "L", ReferenceName = "Ihievbe"},
                                 new {Id    = "ihp", Scope = "I", Type = "L", ReferenceName = "Iha"},
                                 new {Id    = "ihw", Scope = "I", Type = "E", ReferenceName = "Bidhawal"},
                                 new
                                 {
                                     Id            = "iii",
                                     Part2B        = "iii",
                                     Part2T        = "iii",
                                     Part1         = "ii",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Sichuan Yi"
                                 }, new {Id = "iin", Scope = "I", Type = "E", ReferenceName = "Thiin"},
                                 new {Id    = "ijc", Scope = "I", Type = "L", ReferenceName = "Izon"},
                                 new {Id    = "ije", Scope = "I", Type = "L", ReferenceName = "Biseni"},
                                 new {Id    = "ijj", Scope = "I", Type = "L", ReferenceName = "Ede Ije"},
                                 new {Id    = "ijn", Scope = "I", Type = "L", ReferenceName = "Kalabari"},
                                 new {Id    = "ijs", Scope = "I", Type = "L", ReferenceName = "Southeast Ijo"},
                                 new
                                 {
                                     Id            = "ike",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Eastern Canadian Inuktitut"
                                 }, new {Id = "iki", Scope = "I", Type = "L", ReferenceName = "Iko"},
                                 new {Id    = "ikk", Scope = "I", Type = "L", ReferenceName = "Ika"},
                                 new {Id    = "ikl", Scope = "I", Type = "L", ReferenceName = "Ikulu"},
                                 new {Id    = "iko", Scope = "I", Type = "L", ReferenceName = "Olulumo-Ikom"},
                                 new {Id    = "ikp", Scope = "I", Type = "L", ReferenceName = "Ikpeshi"},
                                 new {Id    = "ikr", Scope = "I", Type = "E", ReferenceName = "Ikaranggal"},
                                 new
                                     {
                                         Id            = "iks",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Inuit Sign Language"
                                     },
                                 new {Id = "ikt", Scope = "I", Type = "L", ReferenceName = "Inuinnaqtun"},
                                 new
                                 {
                                     Id            = "iku",
                                     Part2B        = "iku",
                                     Part2T        = "iku",
                                     Part1         = "iu",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Inuktitut"
                                 }, new {Id = "ikv", Scope = "I", Type = "L", ReferenceName = "Iku-Gora-Ankwa"},
                                 new {Id    = "ikw", Scope = "I", Type = "L", ReferenceName = "Ikwere"},
                                 new {Id    = "ikx", Scope = "I", Type = "L", ReferenceName = "Ik"},
                                 new {Id    = "ikz", Scope = "I", Type = "L", ReferenceName = "Ikizu"},
                                 new {Id    = "ila", Scope = "I", Type = "L", ReferenceName = "Ile Ape"},
                                 new {Id    = "ilb", Scope = "I", Type = "L", ReferenceName = "Ila"},
                                 new
                                 {
                                     Id            = "ile",
                                     Part2B        = "ile",
                                     Part2T        = "ile",
                                     Part1         = "ie",
                                     Scope         = "I",
                                     Type          = "C",
                                     ReferenceName = "Interlingue"
                                 }, new {Id = "ilg", Scope = "I", Type = "E", ReferenceName = "Garig-Ilgar"},
                                 new {Id    = "ili", Scope = "I", Type = "L", ReferenceName = "Ili Turki"},
                                 new {Id    = "ilk", Scope = "I", Type = "L", ReferenceName = "Ilongot"},
                                 new {Id    = "ilm", Scope = "I", Type = "L", ReferenceName = "Iranun (Malaysia)"},
                                 new
                                 {
                                     Id            = "ilo",
                                     Part2B        = "ilo",
                                     Part2T        = "ilo",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Iloko"
                                 }, new {Id = "ilp", Scope = "I", Type = "L", ReferenceName = "Iranun (Philippines)"},
                                 new {Id    = "ils", Scope = "I", Type = "L", ReferenceName = "International Sign"},
                                 new {Id    = "ilu", Scope = "I", Type = "L", ReferenceName = "Ili'uun"},
                                 new {Id    = "ilv", Scope = "I", Type = "L", ReferenceName = "Ilue"},
                                 new {Id    = "ima", Scope = "I", Type = "L", ReferenceName = "Mala Malasar"},
                                 new {Id    = "imi", Scope = "I", Type = "L", ReferenceName = "Anamgura"},
                                 new {Id    = "iml", Scope = "I", Type = "E", ReferenceName = "Miluk"},
                                 new {Id    = "imn", Scope = "I", Type = "L", ReferenceName = "Imonda"},
                                 new {Id    = "imo", Scope = "I", Type = "L", ReferenceName = "Imbongu"},
                                 new {Id    = "imr", Scope = "I", Type = "L", ReferenceName = "Imroing"},
                                 new {Id    = "ims", Scope = "I", Type = "A", ReferenceName = "Marsian"},
                                 new {Id    = "imy", Scope = "I", Type = "A", ReferenceName = "Milyan"},
                                 new
                                 {
                                     Id            = "ina",
                                     Part2B        = "ina",
                                     Part2T        = "ina",
                                     Part1         = "ia",
                                     Scope         = "I",
                                     Type          = "C",
                                     ReferenceName = "Interlingua (International Auxiliary Language Association)"
                                 }, new {Id = "inb", Scope = "I", Type = "L", ReferenceName = "Inga"},
                                 new
                                 {
                                     Id            = "ind",
                                     Part2B        = "ind",
                                     Part2T        = "ind",
                                     Part1         = "id",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Indonesian"
                                 }, new {Id = "ing", Scope = "I", Type = "L", ReferenceName = "Degexit'an"},
                                 new
                                 {
                                     Id            = "inh",
                                     Part2B        = "inh",
                                     Part2T        = "inh",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Ingush"
                                 }, new {Id = "inj", Scope = "I", Type = "L", ReferenceName = "Jungle Inga"},
                                 new
                                     {
                                         Id            = "inl",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Indonesian Sign Language"
                                     },
                                 new {Id = "inm", Scope = "I", Type = "A", ReferenceName = "Minaean"},
                                 new {Id = "inn", Scope = "I", Type = "L", ReferenceName = "Isinai"},
                                 new {Id = "ino", Scope = "I", Type = "L", ReferenceName = "Inoke-Yate"},
                                 new {Id = "inp", Scope = "I", Type = "L", ReferenceName = "Iñapari"},
                                 new {Id = "ins", Scope = "I", Type = "L", ReferenceName = "Indian Sign Language"},
                                 new {Id = "int", Scope = "I", Type = "L", ReferenceName = "Intha"},
                                 new {Id = "inz", Scope = "I", Type = "E", ReferenceName = "Ineseño"},
                                 new {Id = "ior", Scope = "I", Type = "L", ReferenceName = "Inor"},
                                 new {Id = "iou", Scope = "I", Type = "L", ReferenceName = "Tuma-Irumu"},
                                 new {Id = "iow", Scope = "I", Type = "E", ReferenceName = "Iowa-Oto"},
                                 new {Id = "ipi", Scope = "I", Type = "L", ReferenceName = "Ipili"},
                                 new
                                 {
                                     Id            = "ipk",
                                     Part2B        = "ipk",
                                     Part2T        = "ipk",
                                     Part1         = "ik",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Inupiaq"
                                 }, new {Id = "ipo", Scope = "I", Type = "L", ReferenceName = "Ipiko"},
                                 new {Id    = "iqu", Scope = "I", Type = "L", ReferenceName = "Iquito"},
                                 new {Id    = "iqw", Scope = "I", Type = "L", ReferenceName = "Ikwo"},
                                 new {Id    = "ire", Scope = "I", Type = "L", ReferenceName = "Iresim"},
                                 new {Id    = "irh", Scope = "I", Type = "L", ReferenceName = "Irarutu"},
                                 new {Id    = "iri", Scope = "I", Type = "L", ReferenceName = "Rigwe"},
                                 new {Id    = "irk", Scope = "I", Type = "L", ReferenceName = "Iraqw"},
                                 new {Id    = "irn", Scope = "I", Type = "L", ReferenceName = "Irántxe"},
                                 new {Id    = "irr", Scope = "I", Type = "L", ReferenceName = "Ir"},
                                 new {Id    = "iru", Scope = "I", Type = "L", ReferenceName = "Irula"},
                                 new {Id    = "irx", Scope = "I", Type = "L", ReferenceName = "Kamberau"},
                                 new {Id    = "iry", Scope = "I", Type = "L", ReferenceName = "Iraya"},
                                 new {Id    = "isa", Scope = "I", Type = "L", ReferenceName = "Isabi"},
                                 new {Id    = "isc", Scope = "I", Type = "L", ReferenceName = "Isconahua"},
                                 new {Id    = "isd", Scope = "I", Type = "L", ReferenceName = "Isnag"},
                                 new {Id    = "ise", Scope = "I", Type = "L", ReferenceName = "Italian Sign Language"},
                                 new {Id    = "isg", Scope = "I", Type = "L", ReferenceName = "Irish Sign Language"},
                                 new {Id    = "ish", Scope = "I", Type = "L", ReferenceName = "Esan"},
                                 new {Id    = "isi", Scope = "I", Type = "L", ReferenceName = "Nkem-Nkum"},
                                 new {Id    = "isk", Scope = "I", Type = "L", ReferenceName = "Ishkashimi"},
                                 new
                                 {
                                     Id            = "isl",
                                     Part2B        = "ice",
                                     Part2T        = "isl",
                                     Part1         = "is",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Icelandic"
                                 }, new {Id = "ism", Scope = "I", Type = "L", ReferenceName = "Masimasi"},
                                 new {Id    = "isn", Scope = "I", Type = "L", ReferenceName = "Isanzu"},
                                 new {Id    = "iso", Scope = "I", Type = "L", ReferenceName = "Isoko"},
                                 new {Id    = "isr", Scope = "I", Type = "L", ReferenceName = "Israeli Sign Language"},
                                 new {Id    = "ist", Scope = "I", Type = "L", ReferenceName = "Istriot"},
                                 new {Id    = "isu", Scope = "I", Type = "L", ReferenceName = "Isu (Menchum Division)"},
                                 new
                                 {
                                     Id            = "ita",
                                     Part2B        = "ita",
                                     Part2T        = "ita",
                                     Part1         = "it",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Italian"
                                 }, new {Id = "itb", Scope = "I", Type = "L", ReferenceName = "Binongan Itneg"},
                                 new {Id    = "itd", Scope = "I", Type = "L", ReferenceName = "Southern Tidung"},
                                 new {Id    = "ite", Scope = "I", Type = "E", ReferenceName = "Itene"},
                                 new {Id    = "iti", Scope = "I", Type = "L", ReferenceName = "Inlaod Itneg"},
                                 new {Id    = "itk", Scope = "I", Type = "L", ReferenceName = "Judeo-Italian"},
                                 new {Id    = "itl", Scope = "I", Type = "L", ReferenceName = "Itelmen"},
                                 new {Id    = "itm", Scope = "I", Type = "L", ReferenceName = "Itu Mbon Uzo"},
                                 new {Id    = "ito", Scope = "I", Type = "L", ReferenceName = "Itonama"},
                                 new {Id    = "itr", Scope = "I", Type = "L", ReferenceName = "Iteri"},
                                 new {Id    = "its", Scope = "I", Type = "L", ReferenceName = "Isekiri"},
                                 new {Id    = "itt", Scope = "I", Type = "L", ReferenceName = "Maeng Itneg"},
                                 new {Id    = "itv", Scope = "I", Type = "L", ReferenceName = "Itawit"},
                                 new {Id    = "itw", Scope = "I", Type = "L", ReferenceName = "Ito"},
                                 new {Id    = "itx", Scope = "I", Type = "L", ReferenceName = "Itik"},
                                 new {Id    = "ity", Scope = "I", Type = "L", ReferenceName = "Moyadan Itneg"},
                                 new {Id    = "itz", Scope = "I", Type = "L", ReferenceName = "Itzá"},
                                 new {Id    = "ium", Scope = "I", Type = "L", ReferenceName = "Iu Mien"},
                                 new {Id    = "ivb", Scope = "I", Type = "L", ReferenceName = "Ibatan"},
                                 new {Id    = "ivv", Scope = "I", Type = "L", ReferenceName = "Ivatan"},
                                 new {Id    = "iwk", Scope = "I", Type = "L", ReferenceName = "I-Wak"},
                                 new {Id    = "iwm", Scope = "I", Type = "L", ReferenceName = "Iwam"},
                                 new {Id    = "iwo", Scope = "I", Type = "L", ReferenceName = "Iwur"},
                                 new {Id    = "iws", Scope = "I", Type = "L", ReferenceName = "Sepik Iwam"},
                                 new {Id    = "ixc", Scope = "I", Type = "L", ReferenceName = "Ixcatec"},
                                 new {Id    = "ixl", Scope = "I", Type = "L", ReferenceName = "Ixil"},
                                 new {Id    = "iya", Scope = "I", Type = "L", ReferenceName = "Iyayu"},
                                 new {Id    = "iyo", Scope = "I", Type = "L", ReferenceName = "Mesaka"},
                                 new {Id    = "iyx", Scope = "I", Type = "L", ReferenceName = "Yaka (Congo)"},
                                 new {Id    = "izh", Scope = "I", Type = "L", ReferenceName = "Ingrian"},
                                 new {Id    = "izr", Scope = "I", Type = "L", ReferenceName = "Izere"},
                                 new {Id    = "izz", Scope = "I", Type = "L", ReferenceName = "Izii"});
        }
    }
}