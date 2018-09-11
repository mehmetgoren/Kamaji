namespace Kamaji.Data.Mongo.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using ionix.Data.Mongo;
    using Kamaji.Data.Models;
    using System;
    using System.IO;
    using System.Reflection;


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


        public string ResourcePath { get; set; }

        //Burada mongodb 16 mb per row' u desteklediğinden dosya path' ını kaydetmeli ve bu kısmı manupile edebilmeli
        [BsonIgnore]
        public byte[] Resources //Burası Kamaji' nin çalıştığı dosyada çalışıyor.
        {
            get
            {
                if (!String.IsNullOrEmpty(this.ResourcePath))
                {
                    try
                    {
                        return File.ReadAllBytes(this.ResourcePath);
                    }
                    catch { }
                }

                return null;
            }
            set
            {
                if (null != value)
                {
                    string executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);//Eğer Common' u eklersen bunu Utility' ye getir.
                    string filePath = executionPath + $"\\{nameof(ScanResource)}s";
                    if (!Directory.Exists(filePath))
                        Directory.CreateDirectory(filePath);

                    string fileName = this.Name + "_" + this.Version;//if it has illegal file name char, fix it later.
                    this.ResourcePath = $@"{filePath}\{fileName}.zip";

                    File.WriteAllBytes(this.ResourcePath, value);
                }
            }

        }

        object IScanResourceModel.ScanPrerequisiteId { get => this.ScanPrerequisiteId; set => this.ScanPrerequisiteId = (ObjectId?)value; }

        object IScanResourceModel.ScanResourceId { get => this.ScanResourceId; set => this.ScanResourceId = (ObjectId)value; }
    }
}
