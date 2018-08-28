namespace KamajiTests
{
    using System;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Kamaji.Common;
    using Kamaji.Common.Models;

    public class RestClient : RestClientBase
    {
        public static readonly RestClient Instance = new RestClient();

        private RestClient()
            : base("http://192.168.0.21:1000/api")
        {

        }

        public async Task SignIn()
        {
            TakeATokenModel model = new TakeATokenModel() { Address = "UnitTests", Password = "You are right boss" };
            Guid token = await this.PostAsync<Guid>("Auth/TakeAToken", model);
            AuthToken = token;
        }

        protected override AuthenticationHeaderValue CreateAuthenticationHeader() => AuthToken == default ? null : new AuthenticationHeaderValue("Token", Serializer.ToBaseb64(AuthToken.ToString()));

        public static Guid AuthToken { get; set; }
    }
}
