using System;
using System.Collections.Generic;
using System.Data.Common;
using Moq;

namespace SqlUnitTestHelper.MockDb
{
    public class MockDbProviderFactory : Mock<DbProviderFactory>
    {
        public readonly MockDbConnection MockConnection;
        public IReadOnlyList<MockDbCommand> MockCommands { get; }
        private int MockCommandsIndex { get; set; } = -1;

        public MockDbProviderFactory(MockDbCommand mockCommand) : this(new[] {mockCommand})
        {
        }

        public MockDbProviderFactory(IList<MockDbCommand> commands = null) : base(MockBehavior.Loose)
        {
            MockCommands = commands == null ? 
                new List<MockDbCommand>()  : new List<MockDbCommand>(commands);
            CallBase = true;
            SetupAllProperties();
            MockConnection = new MockDbConnection(GetNextCommand);
            Setup(f => f.CreateConnection())
                .Returns(MockConnection.Object);
            Setup(f => f.CreateCommand())
                .Returns(GetNextCommand);
        }

        public DbCommandWrapper GetNextCommand()
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
