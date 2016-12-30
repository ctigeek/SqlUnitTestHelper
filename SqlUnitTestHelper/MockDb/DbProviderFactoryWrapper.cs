using System.Collections.Generic;
using System.Data.Common;
using Moq;

namespace SqlUnitTestHelper.MockDb
{
    public abstract class DbProviderFactoryWrapper : DbProviderFactory
    {
        public virtual Mock<DbConnectionWrapper> MockConnection { get; set; }
        public virtual List<Mock<DbCommandWrapper>> MockCommands { get; set; }
        public int CurrentIndex { get; set; }
    }
}