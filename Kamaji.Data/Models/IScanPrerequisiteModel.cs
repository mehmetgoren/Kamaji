namespace Kamaji.Data.Models
{
    using System;

    public interface IScanPrerequisiteModel
    {
        object ScanPrerequisiteId { get; set; }

        DateTime CreatedDate { get; set; }

        string Name { get; set; }

        string Version { get; set; }//Versiyon değişince node' lşar günceleyecek ondan sonraki dosyaları.

        byte[] Resources { get; set; }//i.e. node_modules
    }
}
