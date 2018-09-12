namespace Kamaji.Node.Offline
{
    using ionix.Data;
    using ionix.Migration;
    using ionix.Utils.Extensions;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Threading.Tasks;


    [MigrationVersion(Migration100.VersionNo)]
    [Table(nameof(OfflineModel))]
    internal sealed class OfflineModel
    {
        internal static OfflineModel From(object model, string operation, Exception ex)
        {
            OfflineModel ret = new OfflineModel();
            ret.Json = JsonConvert.SerializeObject(model);
            ret.Operation = operation;
            ret.Error = ex.FindRoot()?.Message;
            return ret;
        }

        [DbSchema(IsKey = true, DatabaseGeneratedOption = StoreGeneratedPattern.Identity)]
        public int Id { get; set; }

        [DbSchema(IsNullable = false)]
        public DateTime OfflineDate { get; set; } = DateTime.Now;

        public string Error { get; set; }

        public string Operation { get; set; }

        public string  Json { get; set; }


        internal async Task<int> SaveAsync()
        {
            int ret = 0;
            using (var db = ionixFactory.CreateDbClient())
            {
                ret = await db.Cmd.InsertAsync(this);
            }
            return ret;
        }

        internal static async Task<IEnumerable<T>> GetOfflineData<T>(string operation)
        {
            List<T> ret = new List<T>();
            using (var db = ionixFactory.CreateDbClient())
            {
                var modelList = await db.Cmd.QueryAsync<OfflineModel>("select * from OfflineModel t where t.Operation=@0 order by t.Id".ToQuery(operation));
                foreach(OfflineModel model in modelList)
                {
                    if (!String.IsNullOrEmpty(model.Json))
                    {
                        ret.Add(JsonConvert.DeserializeObject<T>(model.Json));
                    }
                }
            }
            return ret;
        }
    }
}
