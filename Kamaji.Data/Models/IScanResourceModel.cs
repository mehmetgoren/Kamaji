namespace Kamaji.Data.Models
{
    using System;


    public interface IScanResourceModel
    {
        object ScanResourceId { get; set; }

        object ScanPrerequisiteId { get; set; }

        DateTime CreatedDate { get; set; }
        DateTime LastModifiedDate { get; set; }

        string Name { get; set; }

        string Version { get; set; }//Versiyon değişince node' lşar günceleyecek ondan sonraki dosyaları.

        /// <summary>
        /// A .Net assembly which is stored in db.
        /// </summary>
        byte[] Resources { get; set; }//i.e. node_modules
    }
}
