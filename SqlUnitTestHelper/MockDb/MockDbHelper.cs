using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace SqlUnitTestHelper.MockDb
{
    public static class MockDbHelper
    {
        public static MockDbProviderFactory CreateMockProviderFactory(params Mock<DbCommandWrapper>[] dbCommands)
        {
            var factory = new MockDbProviderFactory(dbCommands.ToList());
            return factory;
        }
    }
}
