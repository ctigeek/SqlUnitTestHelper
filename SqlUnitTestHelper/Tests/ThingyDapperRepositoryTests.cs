using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SqlUnitTestHelper.MockDb;
using SqlUnitTestHelper.Repositories;

namespace SqlUnitTestHelper.Tests
{
    [TestFixture]
    public class ThingyDapperRepositoryTests
    {
        private readonly string name = "Thing-One";
        private readonly string description = "Not thing two!";
        private readonly int pk = 123;
        private readonly DateTime createDate = new DateTime(2000, 1, 1);
        private readonly ThingyStatus status = ThingyStatus.Sherbert;

        private ThingyDapperRepository repository;
        private MockDbProviderFactory mockDbProviderFactory;

        [Test]
        public void GetTheThingyReturnsThingyTest()
        {
            SetupMockFactoryForGet(true);
            var thingy = repository.GetTheThingyByName("Thing-One");

            CommonGetAssertions(thingy);

            //Validate the command and reader were used correctly....
            var mockSelectDbCommand = mockDbProviderFactory.MockCommands[0];
            mockSelectDbCommand.Verify(c => c.PublicExecuteDbDataReader(It.IsAny<CommandBehavior>()), Times.Once);
            mockSelectDbCommand.MockDatareader.Verify(dr => dr.Read(), Times.AtLeastOnce);
            mockDbProviderFactory.Verify(f => f.CreateConnection(), Times.Once);
            var mockPropertyCommand = mockDbProviderFactory.MockCommands[1];
            mockPropertyCommand.Verify(c => c.PublicExecuteDbDataReader(It.IsAny<CommandBehavior>()), Times.Once);
            mockPropertyCommand.MockDatareader.Verify(dr => dr.Read(), Times.Exactly(4));
        }

        [Test]
        public async Task GetTheThingyAsyncReturnsThingyTest()
        {
            SetupMockFactoryForGet(true);
            var thingy = await repository.GetTheThingyByNameAsync("Thing-One");

            CommonGetAssertions(thingy);

            //Validate the command and reader were used correctly....
            var mockSelectDbCommand = mockDbProviderFactory.MockCommands[0];
            mockSelectDbCommand.Verify(c => c.PublicExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSelectDbCommand.MockDatareader.Verify(dr => dr.ReadAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            mockDbProviderFactory.Verify(f => f.CreateConnection(), Times.Once);
            var mockPropertyCommand = mockDbProviderFactory.MockCommands[1];
            mockPropertyCommand.Verify(c => c.PublicExecuteDbDataReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()), Times.Once);
            mockPropertyCommand.MockDatareader.Verify(dr => dr.ReadAsync(It.IsAny<CancellationToken>()), Times.Exactly(4));
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
            var mockSelectDbCommand = mockDbProviderFactory.MockCommands[0];
            var queryParameter = mockSelectDbCommand.ParameterCollection.Parameters[0];
            Assert.That(queryParameter.ParameterName, Is.EqualTo("name"));
            Assert.That(queryParameter.Value, Is.EqualTo(name));

            //validate the query parameters sent with the property query are correct...
            var mockPropertyCommand = mockDbProviderFactory.MockCommands[1];
            var propParameter = mockPropertyCommand.ParameterCollection.Parameters[0];
            Assert.That(propParameter.ParameterName, Is.EqualTo("thing_pk"));
            Assert.That(propParameter.Value, Is.EqualTo(pk));

            //Validate the correct SQL was used....
            Assert.That(mockSelectDbCommand.Object.CommandText, Is.EqualTo(ThingySqlRepository.GetTheThingySql));
            Assert.That(mockPropertyCommand.Object.CommandText, Is.EqualTo(ThingySqlRepository.GetTheThingyPropsSql));

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
            
            var dataValues = new List<object[]>();
            if (withData)
            {
                var row1 = new object[] { pk, name, description, createDate, status.ToString() };
                dataValues.Add(row1);
            }

            var propColumnNames = new string[] {"prop_name", "prop_value"};
            var prop1 = new object[] {"name1", "value1"};
            var prop2 = new object[] {"name2", "value2"};
            var prop3 = new object[] {"name3", "value3"};
            var propValues = new List<object[]>(new[] {prop1, prop2, prop3});

            mockDbProviderFactory = new MockDbProviderFactory()
                .AddDatareaderCommand(columnNames, dataValues)
                .AddDatareaderCommand(propColumnNames, propValues);

            repository = new ThingyDapperRepository(mockDbProviderFactory.Object);
        }

        [Test]
        public void InsertTheThingyTest()
        {
            SetupMockFactoryForInsert();
            var thingy = CreateThingy();
            thingy.PrimaryKey = 0;
            repository.SaveOrUpdateTheThingy(thingy);
            int i = 0;
            //assert inserting the row...
            var mockInsertCommand = mockDbProviderFactory.MockCommands[i];
            var queryParameters = mockInsertCommand.ParameterCollection.Parameters;
            Assert.That(queryParameters.First(q => q.ParameterName == "name").Value, Is.EqualTo(name));
            Assert.That(queryParameters.First(q => q.ParameterName == "desc").Value, Is.EqualTo(description));
            Assert.That(queryParameters.First(q => q.ParameterName == "status").Value, Is.EqualTo(status.ToString()));
            Assert.That(queryParameters.First(q => q.ParameterName == "createDate").Value, Is.EqualTo(createDate));
            Assert.That(mockInsertCommand.Object.CommandText, Is.EqualTo(ThingySqlRepository.InsertThingySql));
            mockInsertCommand.Verify(c => c.ExecuteScalar(), Times.Once);

            //assert deleting the props....
            i++;
            var mockDeletePropCommand = mockDbProviderFactory.MockCommands[i];
            var deleteParameters = mockDeletePropCommand.ParameterCollection.Parameters;
            Assert.That(deleteParameters.First(q => q.ParameterName == "thing_pk").Value, Is.EqualTo(pk));
            Assert.That(mockDeletePropCommand.Object.CommandText, Is.EqualTo(ThingySqlRepository.DeleteThingyPropsSql));
            mockDeletePropCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // assert props inserts...
            i++;
            foreach (var prop in thingy.ThingyProperties)
            {
                var propCommand = mockDbProviderFactory.MockCommands[i];
                var propParameters = propCommand.ParameterCollection.Parameters;
                Assert.That(propParameters.First(q => q.ParameterName == "thing_pk").Value, Is.EqualTo(pk));
                Assert.That(propParameters.First(q => q.ParameterName == "prop_name").Value, Is.EqualTo(prop.Key));
                Assert.That(propParameters.First(q => q.ParameterName == "prop_value").Value, Is.EqualTo(prop.Value));
                Assert.That(propCommand.Object.CommandText, Is.EqualTo(ThingySqlRepository.InsertThingyPropsSql));
                propCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
                i++;
            }

            //assert the transaction was committed.
            mockDbProviderFactory.MockConnection.MockTransaction.Verify(t => t.Commit(), Times.Once);
        }

        [Test]
        public void InsertTheThingyCausesExceptionAndRollback()
        {
            mockDbProviderFactory = new MockDbProviderFactory()
                .AddExceptionThrowingCommand(new ApplicationException());
            repository = new ThingyDapperRepository(mockDbProviderFactory.Object);
            var thingy = CreateThingy();
            thingy.PrimaryKey = 0;

            Assert.Throws(typeof(ApplicationException), () => repository.SaveOrUpdateTheThingy(thingy));

            // verify rollback
            mockDbProviderFactory.MockConnection.MockTransaction.Verify(t => t.Rollback(), Times.Once);
        }

        private void SetupMockFactoryForInsert()
        {
            mockDbProviderFactory = new MockDbProviderFactory()
                .AddScalarCommand(123)
                .AddNonQueryCommand(1, 1, 1, 1);
            repository = new ThingyDapperRepository(mockDbProviderFactory.Object);
        }

        private Thingy CreateThingy()
        {
            var thingy = new Thingy
            {
                PrimaryKey = pk,
                Name = name,
                Description = description,
                CreationDate = createDate,
                Status = status
            };
            thingy.ThingyProperties.Add("name1", "value1");
            thingy.ThingyProperties.Add("name2", "value2");
            thingy.ThingyProperties.Add("name3", "value3");
            return thingy;
        }
    }
}
