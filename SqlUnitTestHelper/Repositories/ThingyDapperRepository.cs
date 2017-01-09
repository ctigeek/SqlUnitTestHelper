using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;

namespace SqlUnitTestHelper.Repositories
{
    public class ThingyDapperRepository : RepoBase
    {
        public ThingyDapperRepository(DbProviderFactory factory = null) : base(factory)
        {
        }

        public Thingy GetTheThingyByName(string name)
        {
            Thingy thingy = null;
            using (var connection = dbProviderFactory.CreateConnection(ConnectionString))
            {
                connection.Open();
                var dynoThingy = connection.QueryFirstOrDefault(GetTheThingySql, new {name});
                if (dynoThingy != null)
                {
                    thingy = BuildThingyFromDyno(dynoThingy);
                    thingy.ThingyProperties = GetThingyProperties(thingy.PrimaryKey, connection);
                }
            }
            return thingy;
        }

        private Dictionary<string, string> GetThingyProperties(int pk, DbConnection connection)
        {
            var props = new Dictionary<string, string>();
            var dynoProps = connection.Query(GetTheThingyPropsSql, new {thing_pk = pk});

            foreach (var prop in dynoProps)
            {
                var name = (string) prop.prop_name;
                var val = (string) prop.prop_value;
                props.Add(name, val);
            }
            return props;
        }

        public async Task<Thingy> GetTheThingyByNameAsync(string name)
        {
            Thingy thingy = null;
            using (var connection = dbProviderFactory.CreateConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var dynoThingy = await connection.QueryFirstOrDefaultAsync(new CommandDefinition(GetTheThingySql, new {name}));
                if (dynoThingy != null)
                {
                    thingy = BuildThingyFromDyno(dynoThingy);
                    thingy.ThingyProperties = await GetThingyPropertiesAsync(thingy.PrimaryKey, connection);
                }
            }
            return thingy;
        }

        private async Task<Dictionary<string, string>> GetThingyPropertiesAsync(int pk, DbConnection connection)
        {
            var props = new Dictionary<string, string>();
            var dynoProps = await connection.QueryAsync(GetTheThingyPropsSql, new { thing_pk = pk });

            foreach (var prop in dynoProps)
            {
                var name = (string)prop.prop_name;
                var val = (string)prop.prop_value;
                props.Add(name, val);
            }
            return props;
        }

        private Thingy BuildThingyFromDyno(dynamic dynoThingy)
        {
            var thing = new Thingy
            {
                PrimaryKey = (int)dynoThingy.pk,
                Name = (string)dynoThingy.name,
                Description = (string)dynoThingy.desc,
                CreationDate = (DateTime)dynoThingy.createDate,
                Status = GetThingyStatus((string)dynoThingy.status)
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

                    var command = new CommandDefinition(InsertThingySql, new {name=theThingy.Name, desc=theThingy.Description, status=theThingy.Status.ToString(), createDate=theThingy.CreationDate}, transaction);
                    var pk = connection.ExecuteScalar(command);
                    theThingy.PrimaryKey = (int) pk;
                    UpdateThingyProperties(theThingy, connection, transaction);
                    transaction.Commit();
                }
            }
            catch (Exception)
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

                    var command = new CommandDefinition(UpdateThingySql, new {name=theThingy.Name, desc=theThingy.Description, status=theThingy.Status.ToString(), pk=theThingy.PrimaryKey}, transaction);
                    var rowcount = connection.Execute(command);

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
            var deleteCommand = new CommandDefinition(DeleteThingyPropsSql, new {thing_pk=theThingy.PrimaryKey}, transaction);
            connection.Execute(deleteCommand);

            foreach (var nvp in theThingy.ThingyProperties)
            {
                var insertCommand = new CommandDefinition(InsertThingyPropsSql, new {thing_pk = theThingy.PrimaryKey, prop_name = nvp.Key, prop_value = nvp.Value});
                connection.Execute(insertCommand);
            }
        }
    }
}
