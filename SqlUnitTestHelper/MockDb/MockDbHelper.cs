using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace SqlUnitTestHelper.MockDb
{
    public static class MockDbHelper
    {
        public static Mock<DbCommandWrapper> CreateCommandForNonQuery(Func<int> callbackDeliverRowCount)
        {
            var command = CreateCommand();
            command.Setup(c => c.ExecuteNonQuery())
                .Returns(callbackDeliverRowCount());
            command.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(callbackDeliverRowCount()));
            return command;
        }

        public static Mock<DbCommandWrapper> CreateCommandForScalarQuery(Func<object> callbackDeliverScalarObject)
        {
            var command = CreateCommand();
            command.Setup(c => c.ExecuteScalar())
                .Returns(callbackDeliverScalarObject());
            command.Setup(c => c.ExecuteScalarAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(callbackDeliverScalarObject()));
            return command;
        }

        public static Mock<DbCommandWrapper> CreateCommandForDatareader(string[] columnNames, List<object[]> dataValues)
        {
            var command = CreateCommand();
            command.Object.MockDatareader = CreateDataReader(columnNames, dataValues);
            command.Setup(c => c.PublicExecuteDbDataReader(It.IsAny<CommandBehavior>()))
                .Returns<CommandBehavior>(c => command.Object.MockDatareader.Object);
            command.Setup(c => c.PublicExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                .Returns<CommandBehavior, CancellationToken>((behavior, token) => Task.FromResult((DbDataReader) command.Object.MockDatareader.Object));
            return command;
        }

        public static Mock<DbCommandWrapper> CreateCommandCausesException(Exception exception)
        {
            var command = CreateCommand();
            command.Setup(c => c.ExecuteNonQuery())
                .Throws(exception);
            command.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .Throws(exception);
            command.Setup(c => c.ExecuteScalar())
                .Throws(exception);
            command.Setup(c => c.ExecuteScalarAsync(It.IsAny<CancellationToken>()))
                .Throws(exception);
            command.Setup(c => c.PublicExecuteDbDataReader(It.IsAny<CommandBehavior>()))
                .Throws(exception);
            command.Setup(c => c.PublicExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                .Throws(exception);
            return command;
        }

        public static MockDbProviderFactory CreateMockProviderFactory(params Mock<DbCommandWrapper>[] dbCommands)
        {
            var factory = new MockDbProviderFactory();

            foreach (var mc in dbCommands)
                factory.MockCommands.Add(mc);

            var connection = new MockDbConnection(factory.GetNextCommand);
            factory.MockConnection = connection;
            factory.Setup(f => f.CreateConnection())
                .Returns(connection.Object);
            factory.Setup(f => f.CreateCommand())
                .Returns(factory.GetNextCommand);
            return factory;
        }

        private static Mock<DbCommandWrapper> CreateCommand()
        {
            var command = new Mock<DbCommandWrapper>();
            command.SetupAllProperties();
            command.CallBase = true;
            command.Object.PublicParameters = new DbParameterCollectionWrapper();
            command.Setup(c => c.PublicCreateParameter())
                .Returns(() =>new DbParameterWrapper());
            return command;
        }

        private static Mock<DbDataReaderWrapper> CreateDataReader(string[] columnNames, List<object[]> dataValues)
        {
            var dataReader = new Mock<DbDataReaderWrapper>();
            dataReader.CallBase = true;
            dataReader.SetupAllProperties();
            dataReader.Object.ColumnNames = columnNames;
            dataReader.Object.DataValues = dataValues;
            dataReader.Object.DataReaderRowNumber = -1;

            dataReader.Setup(d => d.GetName(It.IsAny<int>()))
                .Returns<int>(i => dataReader.Object.ColumnNames[i]);
            dataReader.Setup(d => d.GetValues(It.IsAny<object[]>()))
                .Throws(new NotImplementedException());
            dataReader.Setup(d => d.IsDBNull(It.IsAny<int>()))
                .Returns<int>(i => dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i] == DBNull.Value);
            dataReader.SetupGet(d => d.FieldCount).Returns(dataReader.Object.ColumnNames.Length);
            dataReader.Setup(d => d[It.IsAny<string>()])
                .Returns<string>(s => dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][dataReader.Object.GetOrdinalWrapper(s)]);
            dataReader.Setup(d => d[It.IsAny<int>()])
                .Returns<int>(i => dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i]);
            dataReader.SetupGet(d => d.HasRows).Returns(dataReader.Object.DataValues.Count > 0);
            dataReader.SetupGet(d => d.IsClosed).Returns(false);
            dataReader.SetupGet(d => d.RecordsAffected).Returns(dataReader.Object.DataValues.Count);
            dataReader.Setup(d => d.NextResult()).Returns(false);
            dataReader.Setup(d => d.Read())
                .Returns(() =>
                {
                    dataReader.Object.DataReaderRowNumber++;
                    return dataReader.Object.DataReaderRowNumber < dataReader.Object.DataValues.Count;
                });
            dataReader.Setup(d => d.ReadAsync(It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    dataReader.Object.DataReaderRowNumber++;
                    return Task.FromResult(dataReader.Object.DataReaderRowNumber < dataReader.Object.DataValues.Count);
                });
            dataReader.SetupGet(d => d.Depth).Returns(1);
            dataReader.Setup(d => d.GetOrdinal(It.IsAny<string>()))
                .Returns<string>(s => dataReader.Object.GetOrdinalWrapper(s));
            dataReader.Setup(d => d.GetBoolean(It.IsAny<int>()))
                .Returns<int>(i => (bool)dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i]);
            dataReader.Setup(d => d.GetByte(It.IsAny<int>()))
                .Returns<int>(i => (byte)dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i]);
            dataReader.Setup(d => d.GetChar(It.IsAny<int>()))
                .Returns<int>(i => (char)dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i]);
            dataReader.Setup(d => d.GetGuid(It.IsAny<int>()))
                .Returns<int>(i => (Guid)dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i]);
            dataReader.Setup(d => d.GetInt16(It.IsAny<int>()))
                .Returns<int>(i => (short)dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i]);
            dataReader.Setup(d => d.GetInt32(It.IsAny<int>()))
                .Returns<int>(i => (int)dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i]);
            dataReader.Setup(d => d.GetInt64(It.IsAny<int>()))
                .Returns<int>(i => (long)dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i]);
            dataReader.Setup(d => d.GetDateTime(It.IsAny<int>()))
                .Returns<int>(i => (DateTime)dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i]);
            dataReader.Setup(d => d.GetString(It.IsAny<int>()))
                .Returns<int>(i => (string)dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i]);
            dataReader.Setup(d => d.GetDecimal(It.IsAny<int>()))
                .Returns<int>(i => (decimal)dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i]);
            dataReader.Setup(d => d.GetDouble(It.IsAny<int>()))
                .Returns<int>(i => (double)dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i]);
            dataReader.Setup(d => d.GetFloat(It.IsAny<int>()))
                .Returns<int>(i => (float)dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i]);
            dataReader.Setup(d => d.GetDataTypeName(It.IsAny<int>()))
                .Returns<int>(i => dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i].GetType().ToString());
            dataReader.Setup(d => d.GetFieldType(It.IsAny<int>()))
                .Returns<int>(i => dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i].GetType());
            dataReader.Setup(d => d.GetValue(It.IsAny<int>()))
                .Returns<int>(i => dataReader.Object.DataValues[dataReader.Object.DataReaderRowNumber][i]);
            return dataReader;
        }
    }
}
