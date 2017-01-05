using System.Data.Common;
using Moq;

namespace SqlUnitTestHelper.MockDb
{
    public class MockDbTransaction : Mock<DbTransactionWrapper>
    {
        public MockDbTransaction(DbConnection connection) : base(MockBehavior.Default)
        {
            CallBase = true;
            SetupAllProperties();
            SetupGet(t => t.PublicDbConnection).Returns(connection);
        }
    }
}