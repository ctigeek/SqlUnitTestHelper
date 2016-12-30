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
    }
}