namespace Kamaji.Data.Mongo
{
    using Kamaji.Data.Models;
    using Kamaji.Data.Mongo.Models;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;
    using System;
    using System.Threading.Tasks;

    public sealed class AuthRepository : RepositoryBase, IAuthRepository
    {
        internal AuthRepository(DbContext Db) 
            : base(Db) { }


        public async Task<IAuthModel> GetBy(Guid token)
        {
            if (token == Guid.Empty)
                return null;

            return await this.Db.Authes.AsQueryable().FirstOrDefaultAsync(p => p.Token == token);
        }

        public async Task<IAuthModel> GetBy(string address)
        {
            if (String.IsNullOrEmpty(address))
                return null;

            return await this.Db.Authes.AsQueryable().FirstOrDefaultAsync(p => p.Address == address);
        }


        public async Task<IAuthModel> Save(IAuthModel auth)
        {
            if (null == auth || String.IsNullOrEmpty(auth.Address))
                return null;

            // await this.Db.Authes.ReplaceOrInsertOneAsync((m) => m.Address == auth.Address, (Auth)auth);

            Auth exist = await this.Db.Authes.AsQueryable().FirstOrDefaultAsync(p => p.Address == auth.Address);
            if (null != exist)
                await this.Db.Authes.ReplaceOrInsertOneAsync(exist);
            else
                await this.Db.Authes.InsertOneAsync((Auth)auth);

            return auth;
        }

        public async Task<int> EditLoginCount(object authId, uint loginCount)
        {
            if (null == authId)
                return 0;

            Auth auth = await this.Db.Authes.GetByIdAsync(authId);
            if (null != auth)
            {
                auth.LoginCount = loginCount;
                await this.Db.Authes.ReplaceOrInsertOneAsync(auth);
                return 1;
            }

            return 0;
        }
    }
}
