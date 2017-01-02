using System;
using System.Collections.Generic;
using System.Data.Common;
using Moq;

namespace SqlUnitTestHelper.MockDb
{
    public class MockDbProviderFactory : Mock<DbProviderFactory>
    {
        public readonly MockDbConnection MockConnection;
        public IReadOnlyList<Mock<DbCommandWrapper>> MockCommands { get; private set; }
        private int MockCommandsIndex { get; set; } = -1;

        public MockDbProviderFactory(IList<Mock<DbCommandWrapper>> commands = null) : base(MockBehavior.Loose)
        {
            MockCommands = commands == null ? 
                new List<Mock<DbCommandWrapper>>()  : new List<Mock<DbCommandWrapper>>(commands);
            this.CallBase = true;
            this.SetupAllProperties();
            MockConnection = new MockDbConnection(GetNextCommand);
            this.Setup(f => f.CreateConnection())
                .Returns(MockConnection.Object);
            this.Setup(f => f.CreateCommand())
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
