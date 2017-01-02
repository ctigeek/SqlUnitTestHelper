using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace SqlUnitTestHelper.MockDb
{
    public class MockDbConnection : Mock<DbConnectionWrapper>
    {
        public Mock<DbTransactionWrapper> MockTransaction { get; set; }

        public MockDbConnection(Func<DbCommandWrapper> getCommand ) : base(MockBehavior.Default)
        {
            this.CallBase = true;
            this.SetupAllProperties();
            this.Setup(c => c.PublicCreateDbCommand())
                .Returns(getCommand);
            this.Setup(c => c.Open());
            this.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((object)null));
            this.Setup(c => c.PublicBeginDbTransaction(It.IsAny<IsolationLevel>()))
                .Returns<IsolationLevel>(level =>
                {
                    if (MockTransaction == null)
                    {
                        MockTransaction = new Mock<DbTransactionWrapper>();
                        MockTransaction.CallBase = true;
                        MockTransaction.SetupAllProperties();
                        MockTransaction.Object.PublicDbConnection = this.Object;
                    }
                    return MockTransaction.Object;
                });
        }
    }
}