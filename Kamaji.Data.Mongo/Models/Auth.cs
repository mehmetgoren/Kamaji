namespace Kamaji.Data.Mongo.Models
{
    using ionix.Data.Mongo;
    using Kamaji.Data.Models;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using System;


    [MongoCollection(Name = "Auth")]
    [MigrationVersion(Migration100.VersionNo)]
    [MongoIndex("Address", Unique = true)]
    [MongoIndex("Token", Unique = true)]
    public sealed class Auth : IAuthModel
    {
        [BsonId]
        public ObjectId AuthId { get; set; }

        public string Address { get; set; }

        public Guid Token { get; set; }

        public uint LoginCount { get; set; }

        object IAuthModel.AuthId { get => this.AuthId; set => this.AuthId = (ObjectId)value; }
    }
}
