namespace Kamaji.Node.Offline
{
    using System;
    using System.Data.Common;
    using System.Reflection;
    using ionix.Data;
    using ionix.Data.SQLite;
    using ionix.Migration.SQLite;
    using Kamaji.Common;
    using Microsoft.Data.Sqlite;

    internal static class ionixFactory
    {
        private static readonly string _directoryPath = Utility.GetExecutionPath(); 

        private static string GetConnectionString()
        {
            string dbFileName = "Offline.db";
            string dbFilePath = _directoryPath + "\\" + dbFileName;

            return "Data Source=" + dbFilePath;
        }

        private static DbConnection CreateDbConnection()
        {
            try
            {
                DbConnection conn = new SqliteConnection();
                conn.ConnectionString = GetConnectionString();
                conn.Open();

                return conn;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static IDbAccess CreatDataAccess()
        {
            var connection = CreateDbConnection();
            DbAccess dataAccess = new DbAccess(connection);
            return dataAccess;
        }
        public static ITransactionalDbAccess CreateTransactionalDataAccess()
        {
            var connection = CreateDbConnection();
            TransactionalDbAccess dataAccess = new TransactionalDbAccess(connection);
            return dataAccess;
        }

        internal static ICommandFactory CreateFactory(IDbAccess dataAccess)
        {
            return new CommandFactory(dataAccess);
        }

        //Orn Custom type ve select işlemleri için.
        internal static ICommandAdapter CreateCommand(IDbAccess dataAccess)
        {
            return new CommandAdapter(CreateFactory(dataAccess), CreateEntityMetaDataProvider);
        }


        public static DbClient CreateDbClient()
        {
            return new DbClient(CreatDataAccess());
        }

        public static TransactionalDbClient CreateTransactionalDbClient()
        {
            return new TransactionalDbClient(CreateTransactionalDataAccess());
        }



        private static readonly IEntityMetaDataProvider DefaultMetaDataProvider = new DbSchemaMetaDataProvider();

        public static IEntityMetaDataProvider CreateEntityMetaDataProvider()
        {
            return DefaultMetaDataProvider;
        }

        public static void InitMigration()
        {
            using (var client = CreateDbClient())
            {
                new MigrationInitializer(null).Execute(
                    Assembly.GetExecutingAssembly()
                    , client.Cmd, false);

                client.DataAccess.ExecuteNonQuery("vacuum".ToQuery());
            }
        }
    }
}