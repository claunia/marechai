using Microsoft.EntityFrameworkCore;

namespace Cicm.Database.Seeders.Iso639Split
{
    public static class Q
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Iso639>()
                        .HasData(new {Id = "qua", Scope = "I", Type = "L", ReferenceName = "Quapaw"},
                                 new {Id = "qub", Scope = "I", Type = "L", ReferenceName = "Huallaga Huánuco Quechua"},
                                 new {Id = "quc", Scope = "I", Type = "L", ReferenceName = "K'iche'"},
                                 new {Id = "qud", Scope = "I", Type = "L", ReferenceName = "Calderón Highland Quichua"},
                                 new
                                 {
                                     Id            = "que",
                                     Part2B        = "que",
                                     Part2T        = "que",
                                     Part1         = "qu",
                                     Scope         = "M",
                                     Type          = "L",
                                     ReferenceName = "Quechua"
                                 }, new {Id = "quf", Scope = "I", Type = "L", ReferenceName = "Lambayeque Quechua"},
                                 new
                                 {
                                     Id            = "qug",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Chimborazo Highland Quichua"
                                 },
                                 new {Id = "quh", Scope = "I", Type = "L", ReferenceName = "South Bolivian Quechua"},
                                 new {Id = "qui", Scope = "I", Type = "L", ReferenceName = "Quileute"},
                                 new {Id = "quk", Scope = "I", Type = "L", ReferenceName = "Chachapoyas Quechua"},
                                 new {Id = "qul", Scope = "I", Type = "L", ReferenceName = "North Bolivian Quechua"},
                                 new {Id = "qum", Scope = "I", Type = "L", ReferenceName = "Sipacapense"},
                                 new {Id = "qun", Scope = "I", Type = "E", ReferenceName = "Quinault"},
                                 new {Id = "qup", Scope = "I", Type = "L", ReferenceName = "Southern Pastaza Quechua"},
                                 new {Id = "quq", Scope = "I", Type = "L", ReferenceName = "Quinqui"},
                                 new {Id = "qur", Scope = "I", Type = "L", ReferenceName = "Yanahuanca Pasco Quechua"},
                                 new
                                 {
                                     Id            = "qus",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Santiago del Estero Quichua"
                                 }, new {Id = "quv", Scope = "I", Type = "L", ReferenceName = "Sacapulteco"},
                                 new
                                     {
                                         Id            = "quw",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Tena Lowland Quichua"
                                     },
                                 new {Id = "qux", Scope = "I", Type = "L", ReferenceName = "Yauyos Quechua"},
                                 new {Id = "quy", Scope = "I", Type = "L", ReferenceName = "Ayacucho Quechua"},
                                 new {Id = "quz", Scope = "I", Type = "L", ReferenceName = "Cusco Quechua"},
                                 new {Id = "qva", Scope = "I", Type = "L", ReferenceName = "Ambo-Pasco Quechua"},
                                 new {Id = "qvc", Scope = "I", Type = "L", ReferenceName = "Cajamarca Quechua"},
                                 new
                                     {
                                         Id            = "qve",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Eastern Apurímac Quechua"
                                     },
                                 new
                                 {
                                     Id            = "qvh",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Huamalíes-Dos de Mayo Huánuco Quechua"
                                 },
                                 new {Id = "qvi", Scope = "I", Type = "L", ReferenceName = "Imbabura Highland Quichua"},
                                 new {Id = "qvj", Scope = "I", Type = "L", ReferenceName = "Loja Highland Quichua"},
                                 new
                                 {
                                     Id            = "qvl",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Cajatambo North Lima Quechua"
                                 },
                                 new
                                 {
                                     Id            = "qvm",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Margos-Yarowilca-Lauricocha Quechua"
                                 },
                                 new {Id = "qvn", Scope = "I", Type = "L", ReferenceName = "North Junín Quechua"},
                                 new {Id = "qvo", Scope = "I", Type = "L", ReferenceName = "Napo Lowland Quechua"},
                                 new {Id = "qvp", Scope = "I", Type = "L", ReferenceName = "Pacaraos Quechua"},
                                 new {Id = "qvs", Scope = "I", Type = "L", ReferenceName = "San Martín Quechua"},
                                 new {Id = "qvw", Scope = "I", Type = "L", ReferenceName = "Huaylla Wanca Quechua"},
                                 new {Id = "qvy", Scope = "I", Type = "L", ReferenceName = "Queyu"},
                                 new {Id = "qvz", Scope = "I", Type = "L", ReferenceName = "Northern Pastaza Quichua"},
                                 new {Id = "qwa", Scope = "I", Type = "L", ReferenceName = "Corongo Ancash Quechua"},
                                 new {Id = "qwc", Scope = "I", Type = "H", ReferenceName = "Classical Quechua"},
                                 new {Id = "qwh", Scope = "I", Type = "L", ReferenceName = "Huaylas Ancash Quechua"},
                                 new {Id = "qwm", Scope = "I", Type = "E", ReferenceName = "Kuman (Russia)"},
                                 new {Id = "qws", Scope = "I", Type = "L", ReferenceName = "Sihuas Ancash Quechua"},
                                 new {Id = "qwt", Scope = "I", Type = "E", ReferenceName = "Kwalhioqua-Tlatskanai"},
                                 new {Id = "qxa", Scope = "I", Type = "L", ReferenceName = "Chiquián Ancash Quechua"},
                                 new {Id = "qxc", Scope = "I", Type = "L", ReferenceName = "Chincha Quechua"},
                                 new {Id = "qxh", Scope = "I", Type = "L", ReferenceName = "Panao Huánuco Quechua"},
                                 new {Id = "qxl", Scope = "I", Type = "L", ReferenceName = "Salasaca Highland Quichua"},
                                 new
                                 {
                                     Id            = "qxn",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Northern Conchucos Ancash Quechua"
                                 },
                                 new
                                 {
                                     Id            = "qxo",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Southern Conchucos Ancash Quechua"
                                 }, new {Id = "qxp", Scope = "I", Type = "L", ReferenceName = "Puno Quechua"},
                                 new {Id    = "qxq", Scope = "I", Type = "L", ReferenceName = "Qashqa'i"},
                                 new
                                     {
                                         Id            = "qxr",
                                         Scope         = "I",
                                         Type          = "L",
                                         ReferenceName = "Cañar Highland Quichua"
                                     },
                                 new {Id = "qxs", Scope = "I", Type = "L", ReferenceName = "Southern Qiang"},
                                 new
                                 {
                                     Id            = "qxt",
                                     Scope         = "I",
                                     Type          = "L",
                                     ReferenceName = "Santa Ana de Tusi Pasco Quechua"
                                 },
                                 new {Id = "qxu", Scope = "I", Type = "L", ReferenceName = "Arequipa-La Unión Quechua"},
                                 new {Id = "qxw", Scope = "I", Type = "L", ReferenceName = "Jauja Wanca Quechua"},
                                 new {Id = "qya", Scope = "I", Type = "C", ReferenceName = "Quenya"},
                                 new {Id = "qyp", Scope = "I", Type = "E", ReferenceName = "Quiripi"});
        }
    }
}