namespace Kamaji.Data.Mongo
{
    using Kamaji.Data.Models;
    using Kamaji.Data.Mongo.Models;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed class ScanRepository : RepositoryBase, IScanRepository
    {
        internal ScanRepository(DbContext Db) 
            : base(Db) { }

        public async Task<IEnumerable<IScanModel>> GetListBy(bool enabled, params ScanState[] states)
        {
            if (null == states || states.Length == 0)
                return null;

            List<IScanModel> results = new List<IScanModel>();
            foreach (ScanState state in states)
            {
                results.AddRange(await this.Db.Scans.AsQueryable().Where(p => p.Enabled && p.State == state).ToListAsync());
            }
            return results;
        }

        public async Task<IScanModel> Save(IScanModel scan)
        {
            if (null == scan)
                return null;

            await this.Db.Scans.ReplaceOrInsertOneAsync((Scan)scan);

            return scan;
        }

        public async Task<IScanModel> GetBy(string assset, object scanResourceId)
        {
            if (!String.IsNullOrEmpty(assset) && scanResourceId is ObjectId id)
                return await this.Db.Scans.AsQueryable().FirstOrDefaultAsync(p => p.Asset == assset && p.ScanResourceId == id);

            return null;
        }

        public async Task<int> Edit(IScanModel model)
        {
            if (null == model)
                return 0;

            await this.Db.Scans.ReplaceOneAsync((Scan)model);

            return 1;
        }


        private async Task GetRecursivelyChildList_Internal(List<IScanModel> list, ObjectId? parentId)
        {
            if (null == parentId)
                return;

            List<Scan> childs = await this.Db.Scans.AsQueryable().Where(p => p.ParentId == parentId).ToListAsync();
            foreach (Scan child in childs)
            {
                await GetRecursivelyChildList_Internal(list, child.ScanId);
            }

            list.AddRange(childs);
        }
        public async Task<IEnumerable<IScanModel>> GetRecursivelyChildList(object parentId)
        {
            if (parentId is ObjectId id)
            {
                List<IScanModel> list = new List<IScanModel>();
                await GetRecursivelyChildList_Internal(list, id);
                return list;
            }

            return null;
        }


        public async Task<IScanModel> GetBy(object scanId)
        {
            if (scanId is ObjectId id)
                return await this.Db.Scans.AsQueryable().FirstOrDefaultAsync(p => p.ScanId == id);

            return null;
        }
    }
}
