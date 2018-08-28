﻿namespace Kamaji.Data.Mongo
{
    using System;
    using MongoDB.Driver;
    using ionix.Data.Mongo;
    using Kamaji.Data.Mongo.Models;

    public sealed class DbContext
    {
        public IMongoDatabase Database { get; }
        public IMongoClient MongoClient { get; }

        public DbContext(IConnectionStringProvider provider)
        {
            if (null == provider)
                throw new ArgumentNullException("Please register ConnectionStringProvider using Startup.Register function");

            this.Database = MongoAdmin.GetDatabase(new MongoClient(provider.ConnectionString), provider.DatabaseName);

            this.MongoClient = this.Database.Client;//for all clients.

            this._scanPrerequisites = this.GetLazy<ScanPrerequisite>();
            this._scanResources = this.GetLazy<ScanResource>();
            this._scans = this.GetLazy<Scan>();
            this._scanInstances = this.GetLazy<ScanInstance>();
            this._nodes = this.GetLazy<Node>();
            this._authes = this.GetLazy<Auth>();
        }

        public HostInfo HostInfo()
        {
            return MongoAdmin.GetHostInfo(this.Database);
        }


        private Lazy<MongoRepository<TEntity>> GetLazy<TEntity>()
        {
            return new Lazy<MongoRepository<TEntity>>(() => new MongoRepository<TEntity>(this.Database), true);
        }


        private readonly Lazy<MongoRepository<ScanPrerequisite>> _scanPrerequisites;
        public MongoRepository<ScanPrerequisite> ScanPrerequisites => this._scanPrerequisites.Value;

        private readonly Lazy<MongoRepository<ScanResource>> _scanResources;
        public MongoRepository<ScanResource> ScanResources => this._scanResources.Value;

        private readonly Lazy<MongoRepository<Scan>> _scans;
        public MongoRepository<Scan> Scans => this._scans.Value;



        private readonly Lazy<MongoRepository<ScanInstance>> _scanInstances;
        public MongoRepository<ScanInstance> ScanInstances => this._scanInstances.Value;


        private readonly Lazy<MongoRepository<Node>> _nodes;
        public MongoRepository<Node> Nodes => this._nodes.Value;


        private readonly Lazy<MongoRepository<Auth>> _authes;
        public MongoRepository<Auth> Authes => this._authes.Value;
    }
}
