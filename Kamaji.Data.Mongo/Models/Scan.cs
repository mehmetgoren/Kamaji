namespace Kamaji.Data.Mongo.Models
{
    using ionix.Data.Mongo;
    using Kamaji.Data.Models;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;

    [MongoCollection(Name = "Scan")]
    [MigrationVersion(Migration100.VersionNo)]
    [MongoIndex("Asset","ScanResourceId", Unique = true)]
    [MongoIndex("Asset")]
    [MongoIndex("ParentId")]
    public sealed class Scan : IScanModel
    {
        [BsonId]
        public ObjectId ScanId { get; set; }


        public DateTime CreatedDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string Asset { get; set; }

        public ScanType Type { get; set; }

        public bool Enabled { get; set; }

        public ScanState State { get; set; }

        public int Period { get; set; }

        public ObjectId? ParentId { get; set; }

        public ObjectId? ScanResourceId { get; set; }


        public int MaxOperationLimit { get; set; }

        public int MaxErrorLimit { get; set; } = 10;

        public ObjectId? LastAssignedNodeId { get; set; }

        public ObjectId? SelectedNodeId { get; set; }

        public ScanSaveType SaveType { get; set; }


        public BsonDocument ArgsBson { get; set; }

        public bool SaveNullResult { get; set; }


        public int MaxInstance { get; set; }


        object IScanModel.ScanId { get => this.ScanId; set => this.ScanId = (ObjectId)value; }
        object IScanModel.ParentId { get => this.ParentId; set => this.ParentId = (ObjectId)value; }
        object IScanModel.ScanResourceId { get => this.ScanResourceId; set => this.ScanResourceId = (ObjectId?)value; }
        object IScanModel.LastAssignedNodeId { get => this.LastAssignedNodeId; set => this.LastAssignedNodeId = (ObjectId?)value; }
        object IScanModel.SelectedNodeId { get => this.SelectedNodeId; set => this.SelectedNodeId = (ObjectId?)value; }

        ExpandoObject IScanModel.Args
        {
            get
            {
                BsonDocument document = this.ArgsBson;
                if (null != document)
                {
                    IDictionary<string, object> dic = new ExpandoObject();
                    foreach (BsonElement elm in document)
                        dic.Add(elm.Name, elm.Value.AsString);

                    return (ExpandoObject)dic;
                }

                return null;
            }
            set
            {
                BsonDocument document = null;
                if (null != value)
                    document = new BsonDocument(value);

                this.ArgsBson = document;
            }
        }

        public ObjectId? ScanScheduleId { get; set; }
        object IScanModel.ScanScheduleId { get => this.ScanScheduleId; set => this.ScanScheduleId = (ObjectId?)value; }
    }
}
