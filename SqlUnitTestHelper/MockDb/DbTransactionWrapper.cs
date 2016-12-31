using System;
using System.Data.Common;

namespace SqlUnitTestHelper.MockDb
{
    public abstract class DbTransactionWrapper : DbTransaction
    {
        public abstract DbConnection PublicDbConnection { get; set; }
        protected override DbConnection DbConnection => PublicDbConnection;
    }
}
