namespace Kamaji.Data.Models
{
    using System;
     
    public interface IAuthModel
    {
        object AuthId { get; set; }

        string Address { get; set; }

        Guid Token { get; set; }

        uint LoginCount { get; set; }
    }
}
