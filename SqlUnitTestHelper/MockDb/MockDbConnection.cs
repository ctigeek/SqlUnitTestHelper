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
            this.CallBase = true;
            this.SetupAllProperties();
            this.Setup(c => c.PublicCreateDbCommand())
                .Returns(getCommand);
            this.Setup(c => c.Open());
            this.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((object)null));
            MockTransaction = new MockDbTransaction(this.Object);
            this.Setup(c => c.PublicBeginDbTransaction(It.IsAny<IsolationLevel>()))
                .Returns<IsolationLevel>(level => MockTransaction.Object);
        }
    }
}