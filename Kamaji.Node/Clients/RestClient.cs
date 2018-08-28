namespace Kamaji.Node
{
    using System;
    using System.Net.Http.Headers;
    using Kamaji.Common;

    public class RestClient : RestClientBase
    {
        public static readonly RestClient Instance = new RestClient();

        private RestClient()
            : base(DataSources.Jsons.AppSettings.Config.BrokerAddress)
        {

        }

        protected override AuthenticationHeaderValue CreateAuthenticationHeader() => AuthToken == default ? null : new AuthenticationHeaderValue("Token", Serializer.ToBaseb64(AuthToken.ToString()));

        public static Guid AuthToken { get; set; }
    }
}
