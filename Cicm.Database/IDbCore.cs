using System.Data;

namespace Cicm.Database
{
    public interface IDbCore
    {
        Operations Operations { get; }

        long LastInsertRowId { get; }

        bool OpenDb(string server, string user, string database, string password, ushort port);

        void CloseDb();

        bool CreateDb(string database, string server, string user, string password, ushort port);

        IDbDataAdapter GetNewDataAdapter();
    }
}