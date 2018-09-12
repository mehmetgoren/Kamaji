namespace Kamaji.Common
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using ionix.Utils.Extensions;
    using Newtonsoft.Json;

    public abstract class RestClientBase
    {
        public string Host { get; }

        public RestClientBase(string host)
        {
            this.Host = host ?? throw new ArgumentNullException(nameof(host));
        }


        protected virtual void OnHttpClientCreated(HttpClient httpClient) { }

        public virtual async Task<T> GetAsync<T>(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = this.CreateAuthenticationHeader();
                    this.OnHttpClientCreated(client);

                    Uri uri = new Uri(this.Host + "/" + url);
                    using (HttpResponseMessage response = await client.GetAsync(uri, CancellationToken.None))
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var result =  JsonConvert.DeserializeObject<RestClientBase.ResponseModel<T>>(json);
                        if (result.Error != null)
                        {
                            await Utility.CreateLogger(nameof(RestClientBase), nameof(GetAsync)).Code(3).Error(result.Error + " on " + url).SaveAsync();
                        }
                        return result.Data;
                    }
                }
            }
            catch (Exception ex)
            {
                await Utility.CreateLogger(nameof(RestClientBase), nameof(GetAsync)).Code(2).Error(ex.FindRoot().Message + " on " + url).SaveAsync();
                throw;
            }
        }


        public virtual async Task<T> PostAsync<T>(string url, object data)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = this.CreateAuthenticationHeader();
                    this.OnHttpClientCreated(client);

                    Uri uri = new Uri(this.Host + "/" + url);

                    var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                    using (HttpResponseMessage response = await client.PostAsync(uri, content, CancellationToken.None))
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<RestClientBase.ResponseModel<T>>(json);
                        if (result.Error != null)
                        {
                            await Utility.CreateLogger(nameof(RestClientBase), nameof(PostAsync)).Code(3).Error(result.Error + " on " + url).SaveAsync();
                        }
                        return result.Data;
                    }
                }
            }
            catch (Exception ex)
            {
                await Utility.CreateLogger(nameof(RestClientBase), nameof(PostAsync)).Code(2).Error(ex.FindRoot().Message + " on " + url).SaveAsync();
                throw;
            }
        }


        protected abstract AuthenticationHeaderValue CreateAuthenticationHeader();


        //ResultModel Builder olduğuından ikinci bir type' a ihtiyaç olduğu için eklendi.
        private sealed class ResponseModel<T>
        {
            public T Data { get; set; }

            public string Message { get; set; }

            public Exception Error { get; set; }
        }

    }
}
