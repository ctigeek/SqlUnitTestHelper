# SqlUnitTestHelper
Library for Mocking dbConnection, dbCommand, etc.

 1. Change your repo to use a DbProviderFactory. (See the ThingySqlRepository.cs for an example.)

 2. For unit tests, inject a mock DbProviderFactory into the repo.  See the example unit tests for how to set up the dbCommands to "return" data.

https://github.com/ctigeek/SqlUnitTestHelper/blob/master/SqlUnitTestHelper/Tests/ThingySqlRepositoryTests.cs

