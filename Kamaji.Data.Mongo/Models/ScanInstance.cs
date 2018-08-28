namespace Kamaji.Data.Mongo.Models
{
    using ionix.Data.Mongo;
    using Kamaji.Data.Models;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using System;


    [MongoCollection(Name = "ScanInstance")]
    [MigrationVersion(Migration100.VersionNo)]
    [MongoIndex("ScanId")]
    [MongoIndex("StartTime")]
    [MongoIndex("EndTime")]
    public sealed class ScanInstance : IScanInstanceModel
    {
        [BsonId]
        public ObjectId ScanInstanceId { get; set; }

        public ObjectId ScanId { get; set; }


        //newly (brand new) added
        public ObjectId NodeId { get; set; }


        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string Result { get; set; }//it' s stored in Db, so string is a good generic type for this purpose.

        public string FailedReason { get; set; }


        object IScanInstanceModel.ScanInstanceId { get => this.ScanInstanceId; set => this.ScanInstanceId = (ObjectId)value; }
        object IScanInstanceModel.ScanId { get => this.ScanId; set => this.ScanId = (ObjectId)value; }
        object IScanInstanceModel.NodeId { get => this.NodeId; set => this.NodeId = (ObjectId)value; }
    }

}
