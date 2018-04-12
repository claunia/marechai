using System.Data;

namespace Cicm.Database
{
    public partial class Operations
    {
        const int DB_VERSION = 2;

        readonly IDbConnection dbCon;
        readonly IDbCore       dbCore;

        public Operations(IDbConnection connection, IDbCore core)
        {
            dbCon  = connection;
            dbCore = core;
        }
    }
}