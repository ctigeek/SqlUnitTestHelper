using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace SqlUnitTestHelper
{
    #region table_definitions
        // create table dbo.things (
        //      pk int not null,
        //      name varchar(255) not null,
        //      desc varchar(1500) not null,
        //      createDate datetime not null,
        //      status varchar(50) not null)
        //  create table dbo.thing_props (
        //      pk int not null,
        //      thing_pk int not null FOREIGN KEY things.pk,
        //      prop_name varchar(255) not null,
        //      prop_value varchar(1500) not null )
    #endregion

    public class ThingySqlRepository
    {
        public const string getTheThingySql = "select pk, name, desc, createDate, status from dbo.things where name = @name;";
        public const string getTheThingyPropsSql = "select prop_name, prop_value from thing_props where thing_pk = @thing_pk;";
        public const string insertThingySql = "insert into dbo.things (name,desc,createDate,status) values (@name,@desc,@createDate,@status);";
        public const string retrieveThingyPkSql = "select SCOPE_IDENTITY();";
        public const string updateThingySql = "update dbo.things set name=@name, desc=@desc, status=@status where pk=@pk;";
        public const string deleteThingyPropsSql = "delete from dbo.thing_props where thing_pk = @thing_pk;";
        public const string insertThingyPropsSql = "insert into dbo.thing_props (thing_pk,prop_name,prop_value) values (@thing_pk,@prop_name,@prop_value);";
        private const string ConnectionStringName = "myDb";
        private static readonly DbProviderFactory staticProviderFactory;
        private static readonly string connectionString;

        static ThingySqlRepository()
        {
            var connectionStringConfig = ConfigurationManager.ConnectionStrings[ConnectionStringName];
            if (connectionStringConfig == null)
                throw new ConfigurationErrorsException("The connection string " + ConnectionStringName + " is missing.");
            connectionString = connectionStringConfig.ConnectionString;
            staticProviderFactory = DbProviderFactories.GetFactory(connectionStringConfig.ProviderName);
        }

        private readonly DbProviderFactory dbProviderFactory;
        public ThingySqlRepository(DbProviderFactory factory = null)
        {
            dbProviderFactory = factory ?? staticProviderFactory;
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
                        var thingy = BuildThingyFromReader(reader);
                        thingy.ThingyProperties = GetThingyProperties(thingy.PrimaryKey, conn);
                        return thingy;
                    }
                }
            }
            return null;
        }

        private Dictionary<string, string> GetThingyProperties(int pk, DbConnection connection)
        {
            var props = new Dictionary<string, string>();
            var command = connection.CreateCommand(getTheThingyPropsSql);
            command.AddParameter("thing_pk", pk);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var name = (string) reader["prop_name"];
                    var val = (string) reader["prop_value"];
                    props.Add(name, val);
                }
            }
            return props;
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
                        var thingy = BuildThingyFromReader(reader);
                        thingy.ThingyProperties = await GetThingyPropertiesAsync(thingy.PrimaryKey, conn);
                        return thingy;
                    }
                }
            }
            return null;
        }

        private async Task<Dictionary<string, string>> GetThingyPropertiesAsync(int pk, DbConnection connection)
        {
            var props = new Dictionary<string, string>();
            var command = connection.CreateCommand(getTheThingyPropsSql);
            command.AddParameter("thing_pk", pk);
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var name = (string)reader["prop_name"];
                    var val = (string)reader["prop_value"];
                    props.Add(name, val);
                }
            }
            return props;
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
            DbTransaction transaction = null;
            try
            {
                using (var connection = dbProviderFactory.CreateConnection(connectionString))
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    var insertCommand = connection.CreateCommand(insertThingySql,transaction);
                    insertCommand.AddParameter("name", theThingy.Name);
                    insertCommand.AddParameter("desc", theThingy.Description);
                    insertCommand.AddParameter("status", theThingy.Status.ToString());
                    insertCommand.AddParameter("createDate", theThingy.CreationDate);
                    var rowcount = insertCommand.ExecuteNonQuery();
                    //TODO: check rowcount... if not right then throw error.

                    var pkCommand = connection.CreateCommand(retrieveThingyPkSql);
                    theThingy.PrimaryKey = (int) pkCommand.ExecuteScalar();

                    UpdateThingyProperties(theThingy, connection, transaction);
                    transaction.Commit();
                }
            }
            catch
            {
                transaction?.Rollback();
                throw;
            }
        }

        private void UpdateTheThingy(Thingy theThingy)
        {
            DbTransaction transaction = null;
            try
            {
                using (var connection = dbProviderFactory.CreateConnection(connectionString))
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    var command = connection.CreateCommand(updateThingySql, transaction);
                    command.AddParameter("name", theThingy.Name);
                    command.AddParameter("desc", theThingy.Description);
                    command.AddParameter("status", theThingy.Status.ToString());
                    command.AddParameter("pk", theThingy.PrimaryKey);
                    var rowcount = command.ExecuteNonQuery();
                    //TODO: check rowcount... if not right then throw error.
                    UpdateThingyProperties(theThingy, connection, transaction);
                    transaction.Commit();
                }
            }
            catch
            {
                transaction?.Rollback();
                throw;
            }
        }

        private void UpdateThingyProperties(Thingy theThingy, DbConnection connection, DbTransaction transaction)
        {
            var deleteCommand = connection.CreateCommand(deleteThingyPropsSql, transaction);
            deleteCommand.AddParameter("thing_pk", theThingy.PrimaryKey);
            deleteCommand.ExecuteNonQuery();

            foreach (var nvp in theThingy.ThingyProperties)
            {
                var insertCommand = connection.CreateCommand(insertThingyPropsSql, transaction);
                insertCommand.AddParameter("thing_pk", theThingy.PrimaryKey);
                insertCommand.AddParameter("prop_name", nvp.Key);
                insertCommand.AddParameter("prop_value", nvp.Value);
                insertCommand.ExecuteNonQuery();
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

        public static DbCommand CreateCommand(this DbConnection connection, string commandText, DbTransaction transaction = null, CommandType commandType = CommandType.Text)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            command.Transaction = transaction;
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
