using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace SqlUnitTestHelper.MockDb
{
    public class MockDbConnection : Mock<DbConnectionWrapper>
    {
        public MockDbTransaction MockTransaction { get; set; }

        public MockDbConnection(Func<DbCommandWrapper> getCommand ) : base(MockBehavior.Default)
        {
            CallBase = true;
            SetupAllProperties();
            Setup(c => c.PublicCreateDbCommand())
                .Returns(getCommand);
            Setup(c => c.Open());
            Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((object)null));
            MockTransaction = new MockDbTransaction(this.Object);
            Setup(c => c.PublicBeginDbTransaction(It.IsAny<IsolationLevel>()))
                .Returns<IsolationLevel>(level => MockTransaction.Object);
        }
    }
}