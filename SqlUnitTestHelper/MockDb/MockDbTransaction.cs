using System.Data.Common;
using Moq;

namespace SqlUnitTestHelper.MockDb
{
    public class MockDbTransaction : Mock<DbTransactionWrapper>
    {
        public MockDbTransaction(DbConnection connection) : base(MockBehavior.Default)
        {
            this.CallBase = true;
            this.SetupAllProperties();
            this.SetupGet(t => t.PublicDbConnection).Returns(connection);
        }
    }
}