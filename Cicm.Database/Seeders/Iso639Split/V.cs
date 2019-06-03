using Microsoft.EntityFrameworkCore;

namespace Cicm.Database.Seeders.Iso639Split
{
    public static class V
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Iso639>()
                        .HasData(new {Id = "vaa", Scope = "I", Type = "L", ReferenceName = "Vaagri Booli"},
                                 new {Id = "vae", Scope = "I", Type = "L", ReferenceName = "Vale"},
                                 new {Id = "vaf", Scope = "I", Type = "L", ReferenceName = "Vafsi"},
                                 new {Id = "vag", Scope = "I", Type = "L", ReferenceName = "Vagla"},
                                 new {Id = "vah", Scope = "I", Type = "L", ReferenceName = "Varhadi-Nagpuri"},
                                 new
                                 {
                                     Id            = "vai",
                                     Part2B        = "vai",
                                     Part2T        = "vai",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Vai"
                                 }, new {Id = "vaj", Scope = "I", Type = "L", ReferenceName = "Sekele"},
                                 new {Id    = "val", Scope = "I", Type = "L", ReferenceName = "Vehes"},
                                 new {Id    = "vam", Scope = "I", Type = "L", ReferenceName = "Vanimo"},
                                 new {Id    = "van", Scope = "I", Type = "L", ReferenceName = "Valman"},
                                 new {Id    = "vao", Scope = "I", Type = "L", ReferenceName = "Vao"},
                                 new {Id    = "vap", Scope = "I", Type = "L", ReferenceName = "Vaiphei"},
                                 new {Id    = "var", Scope = "I", Type = "L", ReferenceName = "Huarijio"},
                                 new {Id    = "vas", Scope = "I", Type = "L", ReferenceName = "Vasavi"},
                                 new {Id    = "vau", Scope = "I", Type = "L", ReferenceName = "Vanuma"},
                                 new {Id    = "vav", Scope = "I", Type = "L", ReferenceName = "Varli"},
                                 new {Id    = "vay", Scope = "I", Type = "L", ReferenceName = "Wayu"},
                                 new {Id    = "vbb", Scope = "I", Type = "L", ReferenceName = "Southeast Babar"},
                                 new {Id    = "vbk", Scope = "I", Type = "L", ReferenceName = "Southwestern Bontok"},
                                 new {Id    = "vec", Scope = "I", Type = "L", ReferenceName = "Venetian"},
                                 new {Id    = "ved", Scope = "I", Type = "L", ReferenceName = "Veddah"},
                                 new {Id    = "vel", Scope = "I", Type = "L", ReferenceName = "Veluws"},
                                 new {Id    = "vem", Scope = "I", Type = "L", ReferenceName = "Vemgo-Mabas"},
                                 new
                                 {
                                     Id            = "ven",
                                     Part2B        = "ven",
                                     Part2T        = "ven",
                                     Part1         = "ve",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Venda"
                                 }, new {Id = "veo", Scope = "I", Type = "E", ReferenceName = "Ventureño"},
                                 new {Id    = "vep", Scope = "I", Type = "L", ReferenceName = "Veps"},
                                 new {Id    = "ver", Scope = "I", Type = "L", ReferenceName = "Mom Jango"},
                                 new {Id    = "vgr", Scope = "I", Type = "L", ReferenceName = "Vaghri"},
                                 new {Id    = "vgt", Scope = "I", Type = "L", ReferenceName = "Vlaamse Gebarentaal"},
                                 new
                                 {
                                     Id            = "vic",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Virgin Islands Creole English"
                                 }, new {Id = "vid", Scope = "I", Type = "L", ReferenceName = "Vidunda"},
                                 new
                                 {
                                     Id            = "vie",
                                     Part2B        = "vie",
                                     Part2T        = "vie",
                                     Part1         = "vi",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Vietnamese"
                                 }, new {Id = "vif", Scope = "I", Type = "L", ReferenceName = "Vili"},
                                 new {Id    = "vig", Scope = "I", Type = "L", ReferenceName = "Viemo"},
                                 new {Id    = "vil", Scope = "I", Type = "L", ReferenceName = "Vilela"},
                                 new {Id    = "vin", Scope = "I", Type = "L", ReferenceName = "Vinza"},
                                 new {Id    = "vis", Scope = "I", Type = "L", ReferenceName = "Vishavan"},
                                 new {Id    = "vit", Scope = "I", Type = "L", ReferenceName = "Viti"},
                                 new {Id    = "viv", Scope = "I", Type = "L", ReferenceName = "Iduna"},
                                 new {Id    = "vka", Scope = "I", Type = "E", ReferenceName = "Kariyarra"},
                                 new {Id    = "vki", Scope = "I", Type = "L", ReferenceName = "Ija-Zuba"},
                                 new {Id    = "vkj", Scope = "I", Type = "L", ReferenceName = "Kujarge"},
                                 new {Id    = "vkk", Scope = "I", Type = "L", ReferenceName = "Kaur"},
                                 new {Id    = "vkl", Scope = "I", Type = "L", ReferenceName = "Kulisusu"},
                                 new {Id    = "vkm", Scope = "I", Type = "E", ReferenceName = "Kamakan"},
                                 new {Id    = "vko", Scope = "I", Type = "L", ReferenceName = "Kodeoha"},
                                 new
                                     {
                                         Id            = "vkp",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Korlai Creole Portuguese"
                                     },
                                 new {Id = "vkt", Scope = "I", Type = "L", ReferenceName = "Tenggarong Kutai Malay"},
                                 new {Id = "vku", Scope = "I", Type = "L", ReferenceName = "Kurrama"},
                                 new {Id = "vlp", Scope = "I", Type = "L", ReferenceName = "Valpei"},
                                 new {Id = "vls", Scope = "I", Type = "L", ReferenceName = "Vlaams"},
                                 new {Id = "vma", Scope = "I", Type = "L", ReferenceName = "Martuyhunira"},
                                 new {Id = "vmb", Scope = "I", Type = "E", ReferenceName = "Barbaram"},
                                 new {Id = "vmc", Scope = "I", Type = "L", ReferenceName = "Juxtlahuaca Mixtec"},
                                 new {Id = "vmd", Scope = "I", Type = "L", ReferenceName = "Mudu Koraga"},
                                 new {Id = "vme", Scope = "I", Type = "L", ReferenceName = "East Masela"},
                                 new {Id = "vmf", Scope = "I", Type = "L", ReferenceName = "Mainfränkisch"},
                                 new {Id = "vmg", Scope = "I", Type = "L", ReferenceName = "Lungalunga"},
                                 new {Id = "vmh", Scope = "I", Type = "L", ReferenceName = "Maraghei"},
                                 new {Id = "vmi", Scope = "I", Type = "E", ReferenceName = "Miwa"},
                                 new {Id = "vmj", Scope = "I", Type = "L", ReferenceName = "Ixtayutla Mixtec"},
                                 new {Id = "vmk", Scope = "I", Type = "L", ReferenceName = "Makhuwa-Shirima"},
                                 new {Id = "vml", Scope = "I", Type = "E", ReferenceName = "Malgana"},
                                 new {Id = "vmm", Scope = "I", Type = "L", ReferenceName = "Mitlatongo Mixtec"},
                                 new {Id = "vmp", Scope = "I", Type = "L", ReferenceName = "Soyaltepec Mazatec"},
                                 new {Id = "vmq", Scope = "I", Type = "L", ReferenceName = "Soyaltepec Mixtec"},
                                 new {Id = "vmr", Scope = "I", Type = "L", ReferenceName = "Marenje"},
                                 new {Id = "vms", Scope = "I", Type = "E", ReferenceName = "Moksela"},
                                 new {Id = "vmu", Scope = "I", Type = "E", ReferenceName = "Muluridyi"},
                                 new {Id = "vmv", Scope = "I", Type = "E", ReferenceName = "Valley Maidu"},
                                 new {Id = "vmw", Scope = "I", Type = "L", ReferenceName = "Makhuwa"},
                                 new {Id = "vmx", Scope = "I", Type = "L", ReferenceName = "Tamazola Mixtec"},
                                 new {Id = "vmy", Scope = "I", Type = "L", ReferenceName = "Ayautla Mazatec"},
                                 new {Id = "vmz", Scope = "I", Type = "L", ReferenceName = "Mazatlán Mazatec"},
                                 new {Id = "vnk", Scope = "I", Type = "L", ReferenceName = "Vano"},
                                 new {Id = "vnm", Scope = "I", Type = "L", ReferenceName = "Vinmavis"},
                                 new {Id = "vnp", Scope = "I", Type = "L", ReferenceName = "Vunapu"},
                                 new
                                 {
                                     Id            = "vol",
                                     Part2B        = "vol",
                                     Part2T        = "vol",
                                     Part1         = "vo",
                                     Scope         = "I",
                                     Type          = "C",
                                     ReferenceName = "Volapük"
                                 }, new {Id = "vor", Scope = "I", Type = "L", ReferenceName = "Voro"},
                                 new
                                 {
                                     Id            = "vot",
                                     Part2B        = "vot",
                                     Part2T        = "vot",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Votic"
                                 }, new {Id = "vra", Scope = "I", Type = "L", ReferenceName = "Vera'a"},
                                 new {Id    = "vro", Scope = "I", Type = "L", ReferenceName = "Võro"},
                                 new {Id    = "vrs", Scope = "I", Type = "L", ReferenceName = "Varisi"},
                                 new {Id    = "vrt", Scope = "I", Type = "L", ReferenceName = "Burmbar"},
                                 new {Id    = "vsi", Scope = "I", Type = "L", ReferenceName = "Moldova Sign Language"},
                                 new
                                     {
                                         Id            = "vsl",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Venezuelan Sign Language"
                                     },
                                 new
                                     {
                                         Id            = "vsv",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Valencian Sign Language"
                                     },
                                 new {Id = "vto", Scope = "I", Type = "L", ReferenceName = "Vitou"},
                                 new {Id = "vum", Scope = "I", Type = "L", ReferenceName = "Vumbu"},
                                 new {Id = "vun", Scope = "I", Type = "L", ReferenceName = "Vunjo"},
                                 new {Id = "vut", Scope = "I", Type = "L", ReferenceName = "Vute"},
                                 new {Id = "vwa", Scope = "I", Type = "L", ReferenceName = "Awa (China)"});
        }
    }
}