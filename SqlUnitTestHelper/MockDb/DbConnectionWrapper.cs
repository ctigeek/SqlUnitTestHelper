using System.Data;
using System.Data.Common;

namespace SqlUnitTestHelper.MockDb
{
    public abstract class DbConnectionWrapper : DbConnection
    {
        public abstract DbCommand PublicCreateDbCommand();
        protected override DbCommand CreateDbCommand()
        {
            return PublicCreateDbCommand();
        }

        public abstract DbTransaction PublicBeginDbTransaction(IsolationLevel isolationLevel);
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return PublicBeginDbTransaction(isolationLevel);
        }
    }
}