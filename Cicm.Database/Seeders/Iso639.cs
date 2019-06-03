using Cicm.Database.Seeders.Iso639Split;
using Microsoft.EntityFrameworkCore;

namespace Cicm.Database.Seeders
{
    // Taken from https://iso639-3.sil.org/code_tables/download_tables
    // Last update 20190408
    public class Iso639
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            A.Seed(modelBuilder);
            B.Seed(modelBuilder);
            C.Seed(modelBuilder);
            D.Seed(modelBuilder);
            E.Seed(modelBuilder);
            F.Seed(modelBuilder);
            G.Seed(modelBuilder);
            H.Seed(modelBuilder);
            I.Seed(modelBuilder);
            J.Seed(modelBuilder);
            K.Seed(modelBuilder);
            L.Seed(modelBuilder);
            M.Seed(modelBuilder);
            N.Seed(modelBuilder);
            O.Seed(modelBuilder);
            P.Seed(modelBuilder);
            Q.Seed(modelBuilder);
            R.Seed(modelBuilder);
            S.Seed(modelBuilder);
            T.Seed(modelBuilder);
            U.Seed(modelBuilder);
            V.Seed(modelBuilder);
            W.Seed(modelBuilder);
            X.Seed(modelBuilder);
            Y.Seed(modelBuilder);
            Z.Seed(modelBuilder);
        }
    }
}