namespace Kamaji
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Kamaji.Common;

    public class RestClient : RestClientBase
    {
        public RestClient(string host) 
            : base(host) { }


        protected override void OnHttpClientCreated(HttpClient httpClient) => httpClient.Timeout = TimeSpan.FromSeconds(1);

        private static readonly AuthenticationHeaderValue _authenticationHeader =  new AuthenticationHeaderValue("Token", Serializer.ToBaseb64("IamYourBoss"));
        protected override AuthenticationHeaderValue CreateAuthenticationHeader() => _authenticationHeader;
    }
}
