namespace Kamaji.Data.Mongo
{
    using ionix.Data.Mongo;
    using ionix.Data.Mongo.Serializers;
    using Kamaji.Data.Models;
    using Kamaji.Data.Mongo.Models;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;
    using System;
    using System.Threading.Tasks;

    public sealed class ScanResourceRepository : RepositoryBase, IScanResourceRepository
    {
        internal ScanResourceRepository(DbContext Db) 
            : base(Db) { }


        public async Task<IScanResourceModel> GetBy(string name)
        {
            if (String.IsNullOrEmpty(name))
                return null;

            return await this.Db.ScanResources.AsQueryable().FirstOrDefaultAsync(p => p.Name == name);
        }



        public async Task<IScanResourceModel> Save(IScanResourceModel scanResource)
        {
            if (null == scanResource)
                return null;

            await this.Db.ScanResources.ReplaceOrInsertOneAsync((ScanResource)scanResource);

            return scanResource;
        }

        public async Task<object> GetScanResourceIdBy(string name)
        {
            if (String.IsNullOrEmpty(name))
                return null;

            var template = Lookup<ScanResource>.Left(this.Db.Database)
                .Project(p => p.ScanResourceId)
                .Match().Equals(p => p.Name, name).EndMatch()
                .Limit(1);

            string script = template.ToString();

            var result = await template
                .ExecuteAsync(d => new {
                    ScanResource = d.To<ScanResource>()
                });

            return System.Linq.Enumerable.FirstOrDefault(result)?.ScanResource?.ScanResourceId;
        }

        public async Task<IScanResourceModel> GetBy(object scanResourceId)
        {
            if (scanResourceId is ObjectId id)
            {
                return await this.Db.ScanResources.GetByIdAsync(id);
            }

            return null;
        }
    }
}
