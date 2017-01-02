using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace SqlUnitTestHelper.MockDb
{
    public abstract class DbCommandWrapper : DbCommand
    {
        public abstract DbParameterCollection PublicParameters { get; set; }
        protected override DbParameterCollection DbParameterCollection => PublicParameters;

        protected override DbConnection DbConnection { get; set; }
        protected override DbTransaction DbTransaction { get; set; }

        public abstract DbDataReader PublicExecuteDbDataReader(CommandBehavior behavior);
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return PublicExecuteDbDataReader(behavior);
        }

        public abstract Task<DbDataReader> PublicExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken);
        protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            return PublicExecuteDbDataReaderAsync(behavior, cancellationToken);
        }

        public abstract DbParameter PublicCreateParameter();
        protected override DbParameter CreateDbParameter()
        {
            return PublicCreateParameter();
        }
    }
}