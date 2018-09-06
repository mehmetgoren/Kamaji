namespace Kamaji.Data.Mongo
{
    using ionix.Data.Mongo;
    using ionix.Utils.Extensions;
    using Kamaji.Data.Models;
    using Kamaji.Data.Mongo.Models;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;
    using System;
    using System.Collections.Generic;
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
                var filter = Builders<ScanInstance>.Filter.In(p => p.ScanId, System.Linq.Enumerable.Select(scanIds, p => (ObjectId)p));
                var coll = MongoAdmin.GetCollection<ScanInstance>(this.Db.Database);
                var find = await coll.FindAsync(filter);
                return await find.ToListAsync();
            }

            return null;
        }


        //needed to be tested.
        public async Task<int> EditResult(object scanId, object nodeId, DateTime startTime, DateTime? endTime, string result)
        {
            if (scanId is ObjectId sid)
            {
                var found = await this.Db.ScanInstances.AsQueryable().FirstOrDefaultAsync(p=> p.ScanId == sid);
                if (null != found)
                {
                    //if (nodeId is ObjectId nid)
                    //{
                    //    found.NodeId = nid;
                    //}
                    //found.StartTime = startTime;
                    //found.EndTime = endTime;
                    //found.Result = result;

                    //await this.Db.ScanInstances.ReplaceOrInsertOneAsync(found);
                    var r = await this.Db.ScanInstances.UpdateOneAsync(found.ScanInstanceId,
                           (builder) => builder.Set(p => p.NodeId, nodeId).Set(p => p.StartTime, startTime).Set(p => p.EndTime, endTime).Set(p => p.Result, result));

                    return 1;
                }
            }

            return 0;
        }

        public async Task<IScanInstanceModel> GetFirstBy(object scanId)
        {
            if (scanId is ObjectId sid)
            {
                return await this.Db.ScanInstances.AsQueryable().FirstOrDefaultAsync(p => p.ScanId == sid);
            }

            return null;
        }

        public async Task<int> TrimToSize(object scanId, int max)
        {
            int ret = 0;
            if (max > 0 && scanId is ObjectId id)
            {
                while(await this.Db.ScanInstances.AsQueryable().Where(p => p.ScanId == id).CountAsync() > max)
                {
                    var first = await this.Db.ScanInstances.AsQueryable().OrderBy(p => p.ScanInstanceId).Take(1).FirstOrDefaultAsync();
                    if (null != first)
                    {
                        await this.Db.ScanInstances.DeleteOneAsync(p => p.ScanInstanceId == first.ScanInstanceId);
                        ++ret;
                    }
                }
            }

            return ret;
        }

    }
}
