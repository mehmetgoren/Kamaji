﻿namespace Kamaji.Common.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Dynamic;

    public sealed class ScanPrerequisiteModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Version { get; set; }//Versiyon değişince node' lşar günceleyecek ondan sonraki dosyaları.

        [Required]
        public byte[] Resources { get; set; }
    }


    public sealed class ScanResourceModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Version { get; set; }//Versiyon değişince node' lşar günceleyecek ondan sonraki dosyaları.

        [Required]
        public byte[] Resources { get; set; }

        public string ScanPrerequisiteName { get; set; }// id ve version verilecek.
    }


    public sealed class ScanModel
    {
        [Flags]
        public enum ScanState
        {
            NotStarted = 1,
            Assigned = 2,//Kamaji ScanQueueService ile atadığında 
            AssignFailed = 4,
            Running = 8,//WorkerBase' de Staret da.
            Stopped = 16,//kullanıcı tarafından durdurduğunda. Örneğin arayüzden
            NodeShutdown = 32,//node showdown edilince.
            Completed = 64,//Once' da tamamlanmdığında. Örneğin linkler veya taramalar birince.
            Failed = 128// Hata yüzünden veya max error dan  durdurulunca.
        }

        public enum ScanType
        {
            Simple = 0,
            NoDelay = 1,
            Once = 2
        }

        public enum ScanSaveType
        {
            InsertNew = 0,
            Upsert
        }

        public string PrerequisiteName { get; set; }

        //[Required]
        public string ResourceName { get; set; }

        [Required]
        public string Asset { get; set; }

        public ScanType Type { get; set; }

        public ScanState? State { get; set; }

        public ScanSaveType SaveType { get; set; }

        [Range(0, int.MaxValue)]
        public int Period { get; set; }


        public int MaxOperationLimit { get; set; }

        public int MaxErrorLimit { get; set; } = 10;


        public string NodeAddress { get; set; }

        public ExpandoObject Args { get; set; }

        public bool SaveNullResult { get; set; }

        public int MaxInstance { get; set; }
    }


    public sealed class ScanInstanceModel
    {
        [Required]
        public string ResourceName { get; set; }

        [Required]
        public string Asset { get; set; }

        [Required]
        public string NodeAddress { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }


        public string Result { get; set; }//it' s stored in Db, so string is a good generic type for this purpose.

        public string FailedReason { get; set; }
    }


    public sealed class AddChildScansModel
    {
        [Required]
        public ScanModel Parent { get; set; }

        [Required]
        public IEnumerable<ScanModel> Childs { get; set; }
    }


    public sealed class OfflineDataModel
    {
        public int Id { get; set; }

        public DateTime OfflineDate { get; set; } = DateTime.Now;

        public string Error { get; set; }

        public string Operation { get; set; }

        public string Json { get; set; }
    }
}
