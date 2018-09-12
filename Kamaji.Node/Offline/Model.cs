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
    [Table(nameof(OfflineData))]
    internal sealed class OfflineData
    {
        internal static OfflineData From(object model, string operation, Exception ex)
        {
            OfflineData ret = new OfflineData();
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

        internal static async Task<IEnumerable<OfflineData>> GetOfflineData()
        {
            List<OfflineData> ret = new List<OfflineData>();
            using (var db = ionixFactory.CreateDbClient())
            {
                ret.AddRange(await db.Cmd.QueryAsync<OfflineData>("select * from OfflineData t order by t.Id".ToQuery()));
            }
            return ret;
        }

        internal static async Task<int> Delete(int id)
        {
            if (id > 0)
            {
                using (var db = ionixFactory.CreateDbClient())
                {
                    OfflineData entity = new OfflineData { Id = id };
                    await db.Cmd.DeleteAsync(entity);
                }
            }

            return 0;
        }
    }
}
