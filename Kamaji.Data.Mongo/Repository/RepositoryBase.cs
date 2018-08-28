namespace Kamaji.Data.Mongo
{
    using System;

    public abstract class RepositoryBase
    {
        public DbContext Db { get; }

        protected RepositoryBase(DbContext Db)
        {
            this.Db = Db;
        }
    }
}
