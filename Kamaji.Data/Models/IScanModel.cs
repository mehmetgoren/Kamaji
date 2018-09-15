namespace Kamaji.Data.Models
{
    using System;
    using System.Dynamic;

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

    public interface IScanModel
    {
        object ScanId { get; set; }

        //MongoDB ' de DateTime.Now, Postgres' de default value
        DateTime CreatedDate { get; set; }
        DateTime LastModifiedDate { get; set; }

        string Asset { get; set; }

        ScanType Type { get; set; }

        bool Enabled { get; set; }

        ScanState State { get; set; }

        /// <summary>
        ///Milliseconds
        /// </summary>
        int Period { get; set; }

        object ParentId { get; set; }

        /// <summary>
        /// local zipped datasource Id. i.e. specific (puppeteer 1.1.1, & express 4.16.3) node_modules
        /// </summary>
        object ScanResourceId { get; set; }
        //

        int MaxOperationLimit { get; set; }

        int MaxErrorLimit { get; set; }

        object LastAssignedNodeId { get; set; }

        /// <summary>
        /// If a user forces the scan to run on a specific node. 
        /// </summary>
        object SelectedNodeId { get; set; }

        ScanSaveType SaveType { get; set; }

        ExpandoObject Args { get; set; }

        bool SaveNullResult { get; set; }

        /// <summary>
        /// Max result number
        /// </summary>
        int MaxInstance { get; set; }


        object ScanScheduleId { get; set; }
    }
}
