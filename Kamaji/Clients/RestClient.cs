namespace Kamaji
{
    using System.Net.Http.Headers;
    using Kamaji.Common;

    public class RestClient : RestClientBase
    {
        public RestClient(string host) 
            : base(host) { }


        private static readonly AuthenticationHeaderValue _authenticationHeader =  new AuthenticationHeaderValue("Token", Serializer.ToBaseb64("IamYourBoss"));
        protected override AuthenticationHeaderValue CreateAuthenticationHeader() => _authenticationHeader;
    }
}
