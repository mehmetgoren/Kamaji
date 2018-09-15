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

    public sealed class ScanPrerequisiteRepository : RepositoryBase, IScanPrerequisiteRepository
    {
        internal ScanPrerequisiteRepository(DbContext Db) 
            : base(Db) { }


        public async Task<IScanPrerequisiteModel> GetBy(string name)
        {
            if (String.IsNullOrEmpty(name))
                return null;

            return await this.Db.ScanPrerequisites.AsQueryable().FirstOrDefaultAsync(p => p.Name == name);
        }


        public async Task<IScanPrerequisiteModel> Save(IScanPrerequisiteModel scanPrerequisite)
        {
            if (null == scanPrerequisite)
                return null;

            await this.Db.ScanPrerequisites.ReplaceOrInsertOneAsync((ScanPrerequisite)scanPrerequisite);

            return scanPrerequisite;
        }

        public async Task<object> GetScanPrerequisiteId(string name)
        {
            if (String.IsNullOrEmpty(name))
                return null;

            var template = Lookup<ScanPrerequisite>.Left(this.Db.Database)
                .Project(p => p.ScanPrerequisiteId)
                .Match().Equals(p => p.Name, name).EndMatch()
                .Limit(1);

            string script = template.ToString();

            var result = await template
                .ExecuteAsync(d => new {
                    ScanPrerequisite = d.To<ScanPrerequisite>()
                });

            return System.Linq.Enumerable.FirstOrDefault(result)?.ScanPrerequisite?.ScanPrerequisiteId;
        }

        public async Task<string> GetNameBy(object prerequisiteId)
        {
            if (prerequisiteId is ObjectId id)
            {
                var template = Lookup<ScanPrerequisite>.Left(this.Db.Database)
                    .Project(p => p.Name)
                    .Match().Equals(p => p.ScanPrerequisiteId, id).EndMatch()
                    .Limit(1);

                string script = template.ToString();

                var result = await template
                    .ExecuteAsync(d => new {
                        ScanPrerequisite = d.To<ScanPrerequisite>()
                    });

                return System.Linq.Enumerable.FirstOrDefault(result)?.ScanPrerequisite?.Name;
            }

            return null;
        }
    }
}
