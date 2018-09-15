namespace Kamaji.Data.Mongo
{
    using Kamaji.Data.Models;
    using Kamaji.Data.Mongo.Models;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public sealed class ScanRepository : RepositoryBase, IScanRepository
    {
        internal ScanRepository(DbContext Db) 
            : base(Db) { }



        public async Task<IEnumerable<IScanModel>> GetListBy(bool enabled, params ScanState[] states)
        {
            List<IScanModel> results = new List<IScanModel>();
            if (null != states && states.Length > 0)
            {
                foreach (ScanState state in states)
                {
                    results.AddRange(await this.Db.Scans.AsQueryable().Where(p => p.Enabled == enabled && p.State == state).ToListAsync());//non scheduled.
                }
            }
            return results;
        }

        public async Task<IEnumerable<IScanModel>> GetOnDemandListBy(bool enabled, params ScanState[] states)
        {
            List<IScanModel> results = new List<IScanModel>();
            if (null != states && states.Length > 0)
            {
                foreach (ScanState state in states)
                {
                    results.AddRange(await this.Db.Scans.AsQueryable().Where(p => p.Enabled == enabled && p.ScanScheduleId == null && p.State == state).ToListAsync());//non scheduled.
                }
            }
            return results;
        }


        public async Task<IEnumerable<IScanModel>> GetScheduledListBy(bool enabled, DateTime date, params ScanState[] states)
        {
            List<IScanModel> results = new List<IScanModel>();

            if (null != states && states.Length > 0)
            {
                List<IScanModel> temp = new List<IScanModel>();
                foreach (ScanState state in states)
                {
                    temp.AddRange(await this.Db.Scans.AsQueryable().Where(p => p.Enabled == enabled && p.ScanScheduleId != null && p.State == state).ToListAsync());//non scheduled.
                }
                foreach (IScanModel scan in temp)
                {
                    IScanSchedule schedule = await this.Db.ScanSchedules.GetByIdAsync(scan.ScanScheduleId);
                    if (null != schedule && schedule.IsItTime(date))
                    {
                        results.Add(scan);
                    }
                }
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
            List<IScanModel> list = new List<IScanModel>();
            if (parentId is ObjectId id)
            {
                await GetRecursivelyChildList_Internal(list, id);
            }

            return list;
        }


        public async Task<IScanModel> GetBy(object scanId)
        {
            if (scanId is ObjectId id)
                return await this.Db.Scans.AsQueryable().FirstOrDefaultAsync(p => p.ScanId == id);

            return null;
        }


        public async Task<IEnumerable<IScanModel>> GetListByLastAssignedNodeId(bool enabled, object nodeId)
        {
            List<IScanModel> ret = new List<IScanModel>();
            if (nodeId is ObjectId id)
            {
                ret.AddRange(await this.Db.Scans.AsQueryable().Where(p => p.Enabled == enabled && p.LastAssignedNodeId == id).ToListAsync());
            }

            return ret;
        }
    }
}
