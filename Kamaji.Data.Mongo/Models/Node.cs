namespace Kamaji.Data.Mongo.Models
{
    using ionix.Data.Mongo;
    using Kamaji.Data.Models;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using System;


    [MongoCollection(Name = "Node")]
    [MigrationVersion(Migration100.VersionNo)]
    [MongoIndex("Address", Unique = true)]
    public sealed class Node : INodeModel
    {
        [BsonId]
        public ObjectId NodeId { get; set; }


        public string Address { get; set; }

        public string IpAddress { get; set; }

        public int Port { get; set; }

        public string ComputerName { get; set; }

        public ushort ThreadCount { get; set; }

        public int TotalMemory { get; set; }// in MB

        public int TotalExecutingJobCount { get; set; }

        public int TotalQueuedJobCount { get; set; }


        public sbyte ConnectionStatus { get; set; }

        public DateTime LastConnectionTime { get; set; }


        //Ortalamalar alınıp ona göre değerlendirilecek.
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public ulong TotalBeatsCount { get; set; }
        //


        object INodeModel.NodeId { get => this.NodeId; set => this.NodeId = (ObjectId)value; }
        NodeConnectionStatus INodeModel.ConnectionStatus { get => (NodeConnectionStatus)this.ConnectionStatus; set => this.ConnectionStatus = (sbyte)value; }
    }
}
