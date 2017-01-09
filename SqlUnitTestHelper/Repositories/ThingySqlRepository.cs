using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace SqlUnitTestHelper.Repositories
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

    public class ThingySqlRepository : RepoBase
    {
        public ThingySqlRepository(DbProviderFactory factory = null) : base(factory)
        {
        }

        public Thingy GetTheThingyByName(string name)
        {
            using (var connection = dbProviderFactory.CreateConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand(GetTheThingySql);
                command.AddParameter("name", name);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var thingy = BuildThingyFromReader(reader);
                        thingy.ThingyProperties = GetThingyProperties(thingy.PrimaryKey, connection);
                        return thingy;
                    }
                }
            }
            return null;
        }

        private Dictionary<string, string> GetThingyProperties(int pk, DbConnection connection)
        {
            var props = new Dictionary<string, string>();
            var command = connection.CreateCommand(GetTheThingyPropsSql);
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
            using (var conn = dbProviderFactory.CreateConnection(ConnectionString))
            {
                await conn.OpenAsync();
                var command = conn.CreateCommand(GetTheThingySql);
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
            var command = connection.CreateCommand(GetTheThingyPropsSql);
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
                using (var connection = dbProviderFactory.CreateConnection(ConnectionString))
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    var insertCommand = connection.CreateCommand(InsertThingySql,transaction);
                    insertCommand.AddParameter("name", theThingy.Name);
                    insertCommand.AddParameter("desc", theThingy.Description);
                    insertCommand.AddParameter("status", theThingy.Status.ToString());
                    insertCommand.AddParameter("createDate", theThingy.CreationDate);
                    var pk = (int) insertCommand.ExecuteScalar();
                    theThingy.PrimaryKey = pk;
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
                using (var connection = dbProviderFactory.CreateConnection(ConnectionString))
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    var command = connection.CreateCommand(UpdateThingySql, transaction);
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
            var deleteCommand = connection.CreateCommand(DeleteThingyPropsSql, transaction);
            deleteCommand.AddParameter("thing_pk", theThingy.PrimaryKey);
            deleteCommand.ExecuteNonQuery();

            foreach (var nvp in theThingy.ThingyProperties)
            {
                var insertCommand = connection.CreateCommand(InsertThingyPropsSql, transaction);
                insertCommand.AddParameter("thing_pk", theThingy.PrimaryKey);
                insertCommand.AddParameter("prop_name", nvp.Key);
                insertCommand.AddParameter("prop_value", nvp.Value);
                insertCommand.ExecuteNonQuery();
            }
        }
    }
}
