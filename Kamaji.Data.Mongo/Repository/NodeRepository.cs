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
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public sealed class NodeRepository : RepositoryBase, INodeRepository
    {
        internal NodeRepository(DbContext Db) 
            : base(Db) { }

        public async Task<IEnumerable<INodeModel>> GetAll()
        {
            return await this.Db.Nodes.AsQueryable().ToListAsync();
        }

        public async Task<INodeModel> GetBy(string address)
        {
            if (String.IsNullOrEmpty(address))
                return null;

            return await this.Db.Nodes.AsQueryable().FirstOrDefaultAsync(p => p.Address == address);
        }

        public async Task<INodeModel> Save(INodeModel node)
        {
            if (null == node)
                return null;

            await this.Db.Nodes.ReplaceOrInsertOneAsync((Node)node);
            return node;
        }

        public async Task<object> GetIdBy(string nodeAddress)
        {
            if (String.IsNullOrEmpty(nodeAddress))
                return null;

            var template = Lookup<Node>.Left(this.Db.Database)
                .Project(p => p.NodeId)
                .Match().Equals(p => p.Address, nodeAddress).EndMatch()
                .Limit(1);

            string script = template.ToString();

            var result = await template
                .ExecuteAsync(d => new {
                    Node = d.To<Node>()
                });

            return System.Linq.Enumerable.FirstOrDefault(result)?.Node?.NodeId;
        }

        public async Task<INodeModel> GetBy(object nodeId)
        {
            if (nodeId is ObjectId id)
                return await this.Db.Nodes.GetByIdAsync(id);

            return null;
        }
    }
}
