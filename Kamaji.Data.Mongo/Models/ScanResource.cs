namespace Kamaji.Data.Mongo.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using ionix.Data.Mongo;
    using Kamaji.Data.Models;


    [MongoCollection(Name = "ScanResource")]
    [MigrationVersion(Migration100.VersionNo)]
    [MongoIndex("ScanPrerequisiteId")]
    [MongoIndex("Name", Unique = true)]
    public sealed class ScanResource : IScanResourceModel
    {
        [BsonId]
        public ObjectId ScanResourceId { get; set; }

        public ObjectId? ScanPrerequisiteId { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }


        //  Burada mongodb 16 mb per row' u desteklediğinden dosya path' ını kaydetmeli ve bu kısmı manupile edebilmeli
        public byte[] Resources { get; set; }

        object IScanResourceModel.ScanPrerequisiteId { get => this.ScanPrerequisiteId; set => this.ScanPrerequisiteId = (ObjectId)value; }

        object IScanResourceModel.ScanResourceId { get => this.ScanResourceId; set => this.ScanResourceId = (ObjectId)value; }
    }
}
