using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SqlUnitTestHelper.MockDb;

namespace SqlUnitTestHelper.Tests
{
    [TestFixture]
    public class ThingySqlRepositoryTests
    {
        private readonly string name = "Thing-One";
        private readonly string description = "Not thing two!";
        private readonly int pk = 123;
        private readonly DateTime createDate = new DateTime(2000,1,1);
        private readonly ThingyStatus status = ThingyStatus.Sherbert;

        private ThingySqlRepository repository;
        private Mock<DbProviderFactoryWrapper> mockDbProviderFactory;
        private Mock<DbCommandWrapper> mockDbCommand;
        private Mock<DbCommandWrapper> mockPropertyCommand;

        [Test]
        public void GetTheThingyReturnsThingyTest()
        {
            SetupMockFactoryForGet(true);
            var thingy = repository.GetTheThingyByName("Thing-One");

            CommonGetAssertions(thingy);

            //Validate the command and reader were used correctly....
            mockDbCommand.Verify(c => c.PublicExecuteDbDataReader(It.IsAny<CommandBehavior>()), Times.Once);
            mockDbCommand.Object.MockDatareader.Verify(dr => dr.Read(), Times.Once);
            mockDbProviderFactory.Verify(f => f.CreateConnection(), Times.Once);

            mockPropertyCommand.Verify(c => c.PublicExecuteDbDataReader(It.IsAny<CommandBehavior>()), Times.Once);
            mockPropertyCommand.Object.MockDatareader.Verify(dr => dr.Read(), Times.Exactly(4));
        }

        [Test]
        public async Task GetTheThingyAsyncReturnsThingyTest()
        {
            SetupMockFactoryForGet(true);
            var thingy = await repository.GetTheThingyByNameAsync("Thing-One");

            CommonGetAssertions(thingy);

            //Validate the command and reader were used correctly....
            mockDbCommand.Verify(c=>c.PublicExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()), Times.Once);
            mockDbCommand.Object.MockDatareader.Verify(dr=>dr.ReadAsync(It.IsAny<CancellationToken>()),Times.Once);
            mockDbProviderFactory.Verify(f=>f.CreateConnection(), Times.Once);

            mockPropertyCommand.Verify(c => c.PublicExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()), Times.Once);
            mockPropertyCommand.Object.MockDatareader.Verify(dr => dr.ReadAsync(It.IsAny<CancellationToken>()), Times.Exactly(4));
        }

        private void CommonGetAssertions(Thingy thingy)
        {
            //Validate the object returned contains the data returned from the database...
            Assert.That(thingy.PrimaryKey, Is.EqualTo(pk));
            Assert.That(thingy.Name, Is.EqualTo(name));
            Assert.That(thingy.Description, Is.EqualTo(description));
            Assert.That(thingy.CreationDate, Is.EqualTo(createDate));
            Assert.That(thingy.Status, Is.EqualTo(status));
            Assert.That(thingy.ThingyProperties.Count, Is.EqualTo(3));
            Assert.That(thingy.ThingyProperties["name1"], Is.EqualTo("value1"));
            Assert.That(thingy.ThingyProperties["name2"], Is.EqualTo("value2"));
            Assert.That(thingy.ThingyProperties["name3"], Is.EqualTo("value3"));

            //Validate the parameters sent with the query are correct...
            var queryParameter = mockDbCommand.Object.PublicParameters?.Parameters[0];
            Assert.That(queryParameter, Is.Not.Null);
            Assert.That(queryParameter.ParameterName, Is.EqualTo("name"));
            Assert.That(queryParameter.Value, Is.EqualTo(name));

            //validate the query parameters sent with the property query are correct...
            var propParameter = mockPropertyCommand.Object.PublicParameters?.Parameters[0];
            Assert.That(propParameter, Is.Not.Null);
            Assert.That(propParameter.ParameterName, Is.EqualTo("thing_pk"));
            Assert.That(propParameter.Value, Is.EqualTo(pk));

            //Validate the correct SQL was used....
            Assert.That(mockDbCommand.Object.CommandText, Is.EqualTo(ThingySqlRepository.getTheThingySql));
            Assert.That(mockPropertyCommand.Object.CommandText, Is.EqualTo(ThingySqlRepository.getTheThingyPropsSql));

        }

        [Test]
        public void GetTheNonExistantThingyReturnsNullTest()
        {
            SetupMockFactoryForGet(false);
            var thingy = repository.GetTheThingyByName("Thing-One");
            //Assert that if no row is returned from the DB, the returned object is null
            Assert.That(thingy, Is.Null);
        }

        [Test]
        public async Task GetTheNonExistantThingyAsyncReturnsNullTest()
        {
            SetupMockFactoryForGet(false);
            var thingy = await repository.GetTheThingyByNameAsync("Thing-One");
            //Assert that if no row is returned from the DB, the returned object is null
            Assert.That(thingy, Is.Null);
        }

        private void SetupMockFactoryForGet(bool withData)
        {
            var columnNames = new[] {"pk", "name", "desc", "createDate", "status"};
            var row1 = new object[] {pk, name, description, createDate, status.ToString()};
            var dataValues = new List<object[]>();
            if (withData)
            {
                dataValues.Add(row1);
            }
            mockDbCommand = MockDbHelper.CreateCommandForDatareader(columnNames, dataValues);

            var propColumnNames = new string[] {"prop_name", "prop_value"};
            var prop1 = new object[] {"name1", "value1"};
            var prop2 = new object[] {"name2", "value2"};
            var prop3 = new object[] {"name3", "value3"};
            var propValues = new List<object[]>(new[] {prop1, prop2, prop3});
            mockPropertyCommand = MockDbHelper.CreateCommandForDatareader(propColumnNames, propValues);

            mockDbProviderFactory = MockDbHelper.CreateMockProviderFactory(mockDbCommand, mockPropertyCommand);
            repository = new ThingySqlRepository(mockDbProviderFactory.Object);
        }
    }
}
