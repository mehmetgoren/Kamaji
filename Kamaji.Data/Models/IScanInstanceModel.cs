namespace Kamaji.Data.Models
{
    using System;


    public interface IScanInstanceModel
    {
        object ScanInstanceId { get; set; }

        object ScanId { get; set; }

        //Start timne da insert edeceğin için Node' ları da buraya koyalım ve extra Model' e gerek kalmasın. Sıkıntı çıkarsa değştirelişm.
        object NodeId { get; set; }

        DateTime StartTime { get; set; }

        DateTime? EndTime { get; set; }

        string Result { get; set; }//it' s stored in Db, so string is a good generic type for this purpose.

        string FailedReason { get; set; }
    }
}
