namespace ReliableQueue.SQLite
{
    using ionix.Data;
    using ionix.Migration;
    using System.ComponentModel.DataAnnotations.Schema;


    [MigrationVersion(Migration100.VersionNo)]
    [Table("Model")]
    internal sealed class Model
    {
        [DbSchema(IsKey = true, DatabaseGeneratedOption = StoreGeneratedPattern.Identity)]
        public int Id { get; set; }

        [DbSchema(IsNullable = false)]
        public string Json { get; set; }
    }
}
