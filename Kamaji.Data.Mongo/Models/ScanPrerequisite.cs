namespace Kamaji.Data.Mongo.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using Kamaji.Data.Models;
    using System;
    using System.IO;
    using System.Reflection;
    using ionix.Data.Mongo;


    [MongoCollection(Name = "ScanPrerequisite")]
    [MigrationVersion(Migration100.VersionNo)]
    [MongoIndex("Name", Unique = true)]
    public sealed class ScanPrerequisite : IScanPrerequisiteModel
    {
        [BsonId]
        public ObjectId ScanPrerequisiteId { get; set; }

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
                    string filePath = executionPath + $"\\{nameof(ScanPrerequisite)}s";
                    if (!Directory.Exists(filePath))
                        Directory.CreateDirectory(filePath);

                    string fileName = this.Name + "_" + this.Version;//if it has illegal file name char, fix it later.
                    this.ResourcePath = $@"{filePath}\{fileName}.zip";

                    File.WriteAllBytes(this.ResourcePath, value);
                }
            }

        }


        object IScanPrerequisiteModel.ScanPrerequisiteId { get => this.ScanPrerequisiteId; set => this.ScanPrerequisiteId = (ObjectId)value; }
    }
}
