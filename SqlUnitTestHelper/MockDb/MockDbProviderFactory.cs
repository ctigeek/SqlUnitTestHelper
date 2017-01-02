using System;
using System.Collections.Generic;
using System.Data.Common;
using Moq;

namespace SqlUnitTestHelper.MockDb
{
    public class MockDbProviderFactory : Mock<DbProviderFactory>
    {
        public MockDbConnection MockConnection { get; set; }
        public List<Mock<DbCommandWrapper>> MockCommands { get; set; } = new List<Mock<DbCommandWrapper>>();
        private int MockCommandsIndex { get; set; } = -1;

        public MockDbProviderFactory() : base(MockBehavior.Loose)
        {
            this.CallBase = true;
            this.SetupAllProperties();
        }

        public DbCommandWrapper GetNextCommand()
        {
            MockCommandsIndex++;
            return MockCommands[MockCommandsIndex].Object;
        }

    }
}
