namespace Kamaji.Common.Models
{
    using System.ComponentModel.DataAnnotations;

    public sealed class TakeATokenModel
    {
        [Required]
        /// <summary>
        /// request can only get ip address from client not the whore service full address.
        /// </summary>
        public string Address { get; set; }

        [Required]
        /// <summary>
        /// You are right boss
        /// </summary>
        public string Password {get; set; }
    }
}
