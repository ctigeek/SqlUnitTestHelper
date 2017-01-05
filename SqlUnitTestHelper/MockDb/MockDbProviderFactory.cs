using System;
using System.Collections.Generic;
using System.Data.Common;
using Moq;

namespace SqlUnitTestHelper.MockDb
{
    public class MockDbProviderFactory : Mock<DbProviderFactory>
    {
        public readonly MockDbConnection MockConnection;
        public IReadOnlyList<MockDbCommand> MockCommands { get; private set; }
        private int MockCommandsIndex { get; set; } = -1;

        public MockDbProviderFactory(IList<MockDbCommand> commands = null) : base(MockBehavior.Loose)
        {
            MockCommands = commands == null ? new List<MockDbCommand>() : new List<MockDbCommand>(commands);
            CallBase = true;
            SetupAllProperties();
            MockConnection = new MockDbConnection(GetNextCommand);
            Setup(f => f.CreateConnection())
                .Returns(MockConnection.Object);
            Setup(f => f.CreateCommand())
                .Returns(GetNextCommand);
        }

        public MockDbProviderFactory AddDatareaderCommand(string[] columnNames, List<object[]> dataValues)
        {
            AddCommandToList(new MockDbCommand(columnNames, dataValues));
            return this;
        }

        public MockDbProviderFactory AddScalarCommand(object response, params object[] responses)
        {
            AddCommandToList(new MockDbCommand(() => response));
            if (responses != null && responses.Length > 0)
            {
                foreach (var res in responses)
                {
                    AddCommandToList(new MockDbCommand(() => res));
                }
            }
            return this;
        }

        public MockDbProviderFactory AddNonQueryCommand(int rowCount, params int[] rowCounts)
        { 
            AddCommandToList(new MockDbCommand(() => rowCount));
            if (rowCounts != null && rowCounts.Length > 0)
            {
                foreach (var count in rowCounts)
                {
                    AddCommandToList(new MockDbCommand(() => count));
                }
            }
            return this;
        }

        public MockDbProviderFactory AddExceptionThrowingCommand(Exception exception)
        {
            AddCommandToList(new MockDbCommand(exception));
            return this;
        }

        public MockDbProviderFactory AddCommand(MockDbCommand command)
        {
            AddCommandToList(command);
            return this;
        }

        private void AddCommandToList(MockDbCommand mockDbCommand)
        {
            var newList = new List<MockDbCommand>(MockCommands);
            newList.Add(mockDbCommand);
            MockCommands = newList;
        }

        private DbCommandWrapper GetNextCommand()
        {
            MockCommandsIndex++;
            if (MockCommandsIndex >= MockCommands.Count)
            {
                throw new InvalidOperationException("Your code tried to create more commands than what you have set up.");
            }
            return MockCommands[MockCommandsIndex].Object;
        }
    }
}
