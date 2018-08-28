namespace ReliableQueue
{
    using ionix.Data;
    using Newtonsoft.Json;
    using ReliableQueue.SQLite;
    using System;

    public sealed class Queue
    {
        public static readonly Queue Instance = new Queue();
        private Queue() { }

        public int Count()
        {
            try
            {
                using (DbClient client = ionixFactory.CreateDbClient())
                {
                    return client.Cmd.QuerySingle<int>("select count(*) as count from Model".ToQuery());
                }
            }
            catch { return 0; }
        }


        private bool _isInitialized;
        private static readonly object _syncRoot = new object();
        public Transaction<T> StartTransaction<T>()
        {
            if (!this._isInitialized)
            {
                lock(_syncRoot)
                {
                    if (!this._isInitialized)
                    {
                        ionixFactory.InitMigration();
                        using (DbClient client = ionixFactory.CreateDbClient())
                        {
                            client.Cmd.QuerySingle<int>("delete from Model".ToQuery());//her initilize da kuyruğu temizle
                        }
                        this._isInitialized = true;
                    }
                }
            }

            return new Transaction<T>(ionixFactory.CreateTransactionalDbClient());
        }

        public sealed class Transaction<T> : IDisposable
        {
            private TransactionalDbClient Client { get; }

            internal Transaction(TransactionalDbClient client)
            {
                this.Client = client;
            }

            public T Dequeue()
            {
                try
                {
                    SqlQuery q = "select Id, Json from Model t order by t.Id asc limit 1".ToQuery();

                    Model model = this.Client.Cmd.QuerySingle<Model>(q);
                    if (null != model)
                    {
                        T ret = JsonConvert.DeserializeObject<T>(model.Json);
                        this.Client.Cmd.Delete(model);
                        return ret;
                    }
                }
                catch { }

                return default(T);
            }

            public bool Enqueue(T obj)
            {
                if (null != obj)
                {
                    try
                    {
                        string json = JsonConvert.SerializeObject(obj);
                        Model model = new Model { Json = json };

                        return this.Client.Cmd.Insert(model) > 0;
                    }
                    catch { }
                }

                return false;
            }


            public void Dispose() => this.Client?.Dispose();

            public void Commit() => this.Client?.Commit();

            public void Rollback() => this.Client?.Rollback();
        }
    }
}
