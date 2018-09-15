namespace Kamaji.Data.Mongo.Models
{
    using ionix.Data.Mongo;
    using Kamaji.Data.Models;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using System;

    [MongoCollection(Name = nameof(ScanSchedule))]
    [MigrationVersion(Migration100.VersionNo)]
    [MongoIndex("Time")]
    public class ScanSchedule : ScanScheduleModelBase<ObjectId>
    {
        [BsonId]
        public override ObjectId ScanScheduleId { get; set; }

        public override TimeSpan? Time { get; set; }

        public override DaysOfWeek? Days { get; set; }

        public override byte? Day { get; set; }

        public override DateTime? ExactDateTime { get; set; }

    }
}
