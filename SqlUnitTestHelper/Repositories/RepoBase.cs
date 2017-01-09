using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace SqlUnitTestHelper.Repositories
{
    public abstract class RepoBase
    {
        public const string GetTheThingySql = "select pk, name, [desc], createDate, status from dbo.things where name = @name;";
        public const string GetTheThingyPropsSql = "select prop_name, prop_value from thing_props where thing_pk = @thing_pk;";
        public const string InsertThingySql = "insert into dbo.things (name,[desc],createDate,status) output INSERTED.pk values (@name,@desc,@createDate,@status);";
        public const string UpdateThingySql = "update dbo.things set name=@name, desc=@desc, status=@status where pk=@pk;";
        public const string DeleteThingyPropsSql = "delete from dbo.thing_props where thing_pk = @thing_pk;";
        public const string InsertThingyPropsSql = "insert into dbo.thing_props (thing_pk,prop_name,prop_value) values (@thing_pk,@prop_name,@prop_value);";
        protected const string ConnectionStringName = "myDb";

        #region staticy stuff
        protected static readonly DbProviderFactory StaticProviderFactory;
        protected static readonly string ConnectionString;
        // the factory is created via refl
        static RepoBase()
        {
            var connectionStringConfig = ConfigurationManager.ConnectionStrings[ConnectionStringName];
            if (connectionStringConfig == null)
                throw new ConfigurationErrorsException("The connection string " + ConnectionStringName + " is missing.");
            ConnectionString = connectionStringConfig.ConnectionString;
            StaticProviderFactory = DbProviderFactories.GetFactory(connectionStringConfig.ProviderName);
        }
        #endregion
        protected readonly DbProviderFactory dbProviderFactory;

        protected RepoBase(DbProviderFactory factory)
        {
            dbProviderFactory = factory ?? StaticProviderFactory;
        }
        protected ThingyStatus GetThingyStatus(string status)
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

        public static DbCommand AddParameter(this DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.Value = value;
            parameter.ParameterName = name;
            command.Parameters.Add(parameter);
            return command;
        }
    }
}
