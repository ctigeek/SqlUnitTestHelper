using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace SqlUnitTestHelper
{
    public class ThingySqlRepository
    {
        public const string getTheThingySql = "select pk, name, desc, createDate, status from dbo.things where name = @name;";
        public const string insertThingySql = "insert into dbo.things (name,desc,createDate,status) values (@name,@desc,@createDate,@status);";
        public const string retrieveThingyPkSql = "select SCOPE_IDENTITY();";
        public const string updateThingySql = "update dbo.things set name=@name, desc=@desc, status=@status where pk=@pk;";
        private const string ConnectionStringName = "myDb";

        private readonly DbProviderFactory dbProviderFactory;
        private readonly string connectionString;

        public ThingySqlRepository(DbProviderFactory factory = null)
        {
            var connectionStringConfig = ConfigurationManager.ConnectionStrings[ConnectionStringName];
            if (connectionStringConfig == null) throw new ConfigurationErrorsException("The connection string " + ConnectionStringName + " is missing.");
            connectionString = connectionStringConfig.ConnectionString;
            dbProviderFactory = factory ?? DbProviderFactories.GetFactory(connectionStringConfig.ProviderName);
        }

        public Thingy GetTheThingyByName(string name)
        {
            using (var conn = dbProviderFactory.CreateConnection(connectionString))
            {
                conn.Open();
                var command = conn.CreateCommand(getTheThingySql);
                command.AddParameter("name", name);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return BuildThingyFromReader(reader);
                    }
                }
            }
            return null;
        }

        public async Task<Thingy> GetTheThingyByNameAsync(string name)
        {
            using (var conn = dbProviderFactory.CreateConnection(connectionString))
            {
                await conn.OpenAsync();
                var command = conn.CreateCommand(getTheThingySql);
                command.AddParameter("name", name);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return BuildThingyFromReader(reader);
                    }
                }
            }
            return null;
        }

        private Thingy BuildThingyFromReader(DbDataReader reader)
        {
            var thing = new Thingy
            {
                PrimaryKey = (int)reader["pk"],
                Name = (string)reader["name"],
                Description = (string)reader["desc"],
                CreationDate = (DateTime)reader["createDate"],
                Status = GetThingyStatus((string)reader["status"])
            };
            return thing;
        }

        public void SaveOrUpdateTheThingy(Thingy theThingy)
        {
            bool isUpdate = theThingy.PrimaryKey > 0;
            if (isUpdate)
            {
                UpdateTheThingy(theThingy);
            }
            else
            {
                InsertTheThingy(theThingy);
            }
        }

        private void InsertTheThingy(Thingy theThingy)
        {
            using (var conn = dbProviderFactory.CreateConnection(connectionString))
            {
                conn.Open();
                var command = conn.CreateCommand(insertThingySql);
                command.AddParameter("name", theThingy.Name);
                command.AddParameter("desc", theThingy.Description);
                command.AddParameter("status", theThingy.Status.ToString());
                command.AddParameter("createDate", theThingy.CreationDate);
                var rowcount = command.ExecuteNonQuery();
                // check rowcount... if not right then throw error.
                command.Dispose();

                command = conn.CreateCommand(retrieveThingyPkSql);
                theThingy.PrimaryKey = (int)command.ExecuteScalar();
            }
        }

        private void UpdateTheThingy(Thingy theThingy)
        {
            using (var conn = dbProviderFactory.CreateConnection(connectionString))
            {
                conn.Open();
                var command = conn.CreateCommand(updateThingySql);
                command.AddParameter("name", theThingy.Name);
                command.AddParameter("desc", theThingy.Description);
                command.AddParameter("status", theThingy.Status.ToString());
                command.AddParameter("pk", theThingy.PrimaryKey);
                var rowcount = command.ExecuteNonQuery();
                // check rowcount... if not right then throw error.
            }
        }

        private ThingyStatus GetThingyStatus(string status)
        {
            ThingyStatus thingyStatus = ThingyStatus.Unknown;
            if (ThingyStatus.TryParse(status, true, out thingyStatus))
            {
                return thingyStatus;
            }
            return ThingyStatus.Unknown;
        }
    }

    static class DbHelpers
    {
        public static DbConnection CreateConnection(this DbProviderFactory factory, string connectionString)
        {
            var connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;
            return connection;
        }

        public static DbCommand CreateCommand(this DbConnection connection, string commandText, CommandType commandType = CommandType.Text)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            return command;
        }
        public static DbCommand AddParameter(this DbCommand command,string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.Value = value;
            parameter.ParameterName = name;
            command.Parameters.Add(parameter);
            return command;
        }
    }
}
