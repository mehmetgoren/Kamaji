namespace Kamaji.Data.Mongo
{
    using MongoDB.Bson;
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Kamaji.Data.Models;
    using Models;
    using System.Threading.Tasks;

    public sealed class KamajiModelFactory : IKamajiModelFactory
    {
        public static readonly KamajiModelFactory Instance = new KamajiModelFactory();

        private KamajiModelFactory()
        {

        }

        public IScanPrerequisiteModel CreateScanPrerequisiteModel() => new ScanPrerequisite() { ScanPrerequisiteId = ObjectId.GenerateNewId() };

        public IScanResourceModel CreateScanResourceModel() => new ScanResource() { ScanResourceId = ObjectId.GenerateNewId() };

        public IScanModel CreateScanModel() => new Scan() { ScanId = ObjectId.GenerateNewId() };



        public INodeModel CreateNodeModel() => new Node() { NodeId = ObjectId.GenerateNewId() };

        public IScanInstanceModel CreateScanInstanceModel() => new ScanInstance() { ScanInstanceId = ObjectId.GenerateNewId() };

        public IAuthModel CreateAuthModel() => new Auth() { AuthId = ObjectId.GenerateNewId() };
    }

    public sealed class KamajiContext : KamajiContext<ObjectId>
    {
        public KamajiContext()
        {
            this._authes = new Lazy<AuthRepository>(() => new AuthRepository(this.Db), true);
            this._nodes = new Lazy<NodeRepository>(() => new NodeRepository(this.Db), true);
            this._scans = new Lazy<ScanRepository>(() => new ScanRepository(this.Db), true);
            this._scanPrerequisites = new Lazy<ScanPrerequisiteRepository>(() => new ScanPrerequisiteRepository(this.Db), true);
            this._scanResources = new Lazy<ScanResourceRepository>(() => new ScanResourceRepository(this.Db), true);
            this._scanInstances = new Lazy<ScanInstanceRepository>(() => new ScanInstanceRepository(this.Db), true);
        }

        private readonly Lazy<DbContext> _dbContext = new Lazy<DbContext>(() => DI.Provider.GetService<DbContext>(), true);
        private DbContext Db => this._dbContext.Value;

        public override void Init(IConnectionStringProvider connectionStringProvider)
        {
            Startup.Register(connectionStringProvider);
        }

        public override IKamajiModelFactory ModelFactory => KamajiModelFactory.Instance;
        public override async Task<DateTime> GetDbDateTime() => (await this.Db.HostInfoAsync())?.System?.CurrentTime ?? DateTime.Now;




        private readonly Lazy<AuthRepository> _authes;
        public override IAuthRepository Authes => this._authes.Value;

        private readonly Lazy<NodeRepository> _nodes;
        public override INodeRepository Nodes => this._nodes.Value;

        private readonly Lazy<ScanRepository> _scans;
        public override IScanRepository Scans => this._scans.Value;

        private readonly Lazy<ScanPrerequisiteRepository> _scanPrerequisites;
        public override IScanPrerequisiteRepository ScanPrerequisites => this._scanPrerequisites.Value;

        private readonly Lazy<ScanResourceRepository> _scanResources;
        public override IScanResourceRepository ScanResources => this._scanResources.Value;

        private readonly Lazy<ScanInstanceRepository> _scanInstances;
        public override IScanInstanceRepository ScanInstances => this._scanInstances.Value;



        //MongoDbClient is not needed to be disposed.
        public override void Dispose(bool disposing)
        {

        }
    }
}
