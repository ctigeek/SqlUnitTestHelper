using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace SqlUnitTestHelper.MockDb
{
    public class MockDbCommand : Mock<DbCommandWrapper>
    {
        public MockDbDataReader MockDatareader { get; }
        public DbParameterCollectionWrapper ParameterCollection { get; }
        public Func<object> CallbackDeliverScalarObject { get; }
        public Func<int> CallbackDeliverRowcount { get; }

        public MockDbCommand(string[] columnNames, List<object[]> dataValues) : this()
        {
            MockDatareader = new MockDbDataReader(columnNames, dataValues);
            Setup(c => c.PublicExecuteDbDataReader(It.IsAny<CommandBehavior>()))
                .Returns<CommandBehavior>(c => MockDatareader.Object);
            Setup(c => c.PublicExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                .Returns<CommandBehavior, CancellationToken>((behavior, token) => Task.FromResult((DbDataReader)MockDatareader.Object));
        }

        public MockDbCommand(Func<object> callbackDeliverScalarObject) : this()
        {
            CallbackDeliverScalarObject = callbackDeliverScalarObject;
            Setup(c => c.ExecuteScalar())
                .Returns(CallbackDeliverScalarObject);
            Setup(c => c.ExecuteScalarAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CallbackDeliverScalarObject()));
        }

        public MockDbCommand(Func<int> callbackDeliverRowcount) : this()
        {
            CallbackDeliverRowcount = callbackDeliverRowcount;
            Setup(c => c.ExecuteNonQuery())
                .Returns(CallbackDeliverRowcount());
            Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(CallbackDeliverRowcount()));
        }

        public MockDbCommand(Exception exception) : this()
        {
            Setup(c => c.ExecuteNonQuery())
                .Throws(exception);
            Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .Throws(exception);
            Setup(c => c.ExecuteScalar())
                .Throws(exception);
            Setup(c => c.ExecuteScalarAsync(It.IsAny<CancellationToken>()))
                .Throws(exception);
            Setup(c => c.PublicExecuteDbDataReader(It.IsAny<CommandBehavior>()))
                .Throws(exception);
            Setup(c => c.PublicExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                .Throws(exception);
        }

        private MockDbCommand() : base(MockBehavior.Default)
        {
            CallBase = true;
            SetupAllProperties();
            ParameterCollection = new DbParameterCollectionWrapper();

            SetupGet(c => c.PublicParameters).Returns(ParameterCollection);
            As<IDbCommand>().SetupGet(c => c.Parameters).Returns(ParameterCollection);
            
            Setup(c => c.PublicCreateParameter())
                .Returns(() => new DbParameterWrapper());
        }
    }
}