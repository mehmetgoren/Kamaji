namespace Kamaji.Data.Mongo
{
    using ionix.Data.Mongo;
    using ionix.Data.Mongo.Migration;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;
    using System;
    using System.Reflection;

    public static class Startup
    {
        internal static ServiceCollection ServiceCollection { get; } = new ServiceCollection();


        public static void Register(IConnectionStringProvider connectionStringProvider)
        {
            if (null == connectionStringProvider)
                throw new ArgumentNullException(nameof(connectionStringProvider));

            ServiceCollection.AddSingleton(typeof(IConnectionStringProvider), connectionStringProvider.GetType());

            ServiceCollection.AddTransient<DbContext, DbContext>();



            MongoClientProxy.SetConnectionString(connectionStringProvider.ConnectionString);

            var _db = MongoAdmin.GetDatabase(MongoClientProxy.Instance, connectionStringProvider.DatabaseName);
            InitializeMigration(_db);
        }

        private static bool InitializeMigration(IMongoDatabase db)
        {
            if (null != db)
            {
                var runner = new MigrationRunner(db);

                runner.MigrationLocator.LookForMigrationsInAssembly(Assembly.GetExecutingAssembly());
                // runner.MigrationLocator.LookForMigrationsInAssemblyOfType<Migration1>();

                runner.DatabaseStatus.ValidateMigrationsVersions();

                runner.UpdateToLatest();
                return true;
            }

            return false;
        }
    }
}
