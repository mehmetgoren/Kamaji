namespace Kamaji.Data.Models
{
    using System;

    [Flags]
    public enum ScanState
    {
        NotStarted = 1,
        Assigned = 2,//Kamaji ScanQueueService ile atadığında 
        //AssignFailed
        Running = 4,//WorkerBase' de Staret da.
        Stopped = 8,//kullanıcı tarafından durdurduğunda. Örneğin arayüzden
        Cancelled = 16,//node showdown edilince.
        Completed = 32,//Once' da tamamlanmdığında. Örneğin linkler veya taramalar birince.
        Failed = 64// Hata yüzünden veya max error dan  durdurulunca.
    }

    public enum ScanType
    {
        Simple = 0,
        NoDelay = 1,
        Once = 2
    }

    public interface IScanModel
    {
        object ScanId { get; set; }

        //MongoDB ' de DateTime.Now, Postgres' de default value
        DateTime CreatedDate { get; set; }

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
    }
}
