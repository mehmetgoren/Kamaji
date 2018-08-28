namespace Kamaji.Data.Mongo
{
    using ionix.Data.Mongo;
    using ionix.Utils.Extensions;
    using Kamaji.Data.Models;
    using Kamaji.Data.Mongo.Models;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed class ScanInstanceRepository : RepositoryBase, IScanInstanceRepository
    {
        internal ScanInstanceRepository(DbContext Db) 
            : base(Db) { }


        public async Task<IScanInstanceModel> Save(IScanInstanceModel scanInstance)
        {
            if (null == scanInstance)
                return null;

            await this.Db.ScanInstances.ReplaceOrInsertOneAsync((ScanInstance)scanInstance);

            return scanInstance;
        }

        public async Task<IEnumerable<IScanInstanceModel>> GetListBy(IEnumerable<object> scanIds)
        {
            if (!scanIds.IsEmptyList())
            {
                var filter = Builders<ScanInstance>.Filter.In(p => p.ScanId, scanIds.Select(p => (ObjectId)p));
                var coll = MongoAdmin.GetCollection<ScanInstance>(this.Db.Database);
                var find = await coll.FindAsync(filter);
                return await find.ToListAsync();
            }

            return null;
        }
    }
}
