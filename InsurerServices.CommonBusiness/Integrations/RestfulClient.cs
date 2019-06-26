using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Ituran.Framework.Comum.Excecao;
using Ituran.Framework.Comum.Response;
using Newtonsoft.Json;

namespace InsurerServices.CommonBusiness.Integrations
{
    public sealed class RestfulClient<TEntity> : IDisposable
    {
        private const string JsonMediaType = "application/json";
        private readonly string _addressSuffix;
        private System.Net.Http.HttpClient _httpClient;
        private readonly string _accessToken;

        public RestfulClient(string serviceBaseAddress, string addressSuffix, string accessToken = null, long timeOut = 60000)
        {
            _addressSuffix = addressSuffix;
            _accessToken = accessToken;
            _httpClient = MakeHttpClient(serviceBaseAddress, timeOut);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private System.Net.Http.HttpClient MakeHttpClient(string serviceBaseAddress, long timeOut)
        {
            _httpClient = new System.Net.Http.HttpClient
            {
                BaseAddress = new Uri(serviceBaseAddress),
                Timeout = TimeSpan.FromMilliseconds(timeOut)
            };

            if (!string.IsNullOrEmpty(_accessToken))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{_accessToken}");

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(JsonMediaType));
            return _httpClient;
        }

        public TEntity Get()
        {
            var responseMessage = GetSync();
            var result = responseMessage.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<TEntity>(result);
        }

        public TEntity Save(object model)
        {
            var objectContent = CreateJsonObjectContent(model);
            var responseMessage = PostSync(objectContent);
            var result = responseMessage.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<TEntity>(result);
        }

        public TEntity Update(object model)
        {
            var objectContent = CreateJsonObjectContent(model);
            var responseMessage = PutSync(objectContent);
            var result = responseMessage.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<TEntity>(result);
        }

        public TEntity Delete()
        {
            var responseMessage = DeleteSync();
            var result = responseMessage.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<TEntity>(result);
        }

        private HttpResponseMessage GetSync()
        {
            string result = string.Empty;

            try
            {
                var httpResponseMessage = _httpClient.GetAsync(_addressSuffix).Result;

                if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                    throw new ExcecaoAcessoApi();

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    return httpResponseMessage;

                result = httpResponseMessage.Content.ReadAsStringAsync().Result;
                var jsonErrorDataResponse = JsonConvert.DeserializeObject<JsonErrorDataResponse>(result);
                throw new ExcecaoRestful(jsonErrorDataResponse, httpResponseMessage.StatusCode);
            }
            catch (ExcecaoRestful)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception(result);
            }
        }

        private HttpResponseMessage PostSync(StringContent objectContent)
        {
            string result = string.Empty;

            try
            {
                var httpResponseMessage = _httpClient.PostAsync($"{_addressSuffix}", objectContent).Result;

                if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                    throw new ExcecaoAcessoApi();

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    return httpResponseMessage;

                result = httpResponseMessage.Content.ReadAsStringAsync().Result;
                var jsonErrorDataResponse = JsonConvert.DeserializeObject<JsonErrorDataResponse>(result);
                throw new ExcecaoRestful(jsonErrorDataResponse, httpResponseMessage.StatusCode);
            }
            catch (ExcecaoRestful ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                var e = ex;
                throw new Exception(result);
            }
        }

        private HttpResponseMessage PutSync(StringContent objectContent)
        {
            string result = string.Empty;

            try
            {
                var httpResponseMessage = _httpClient.PutAsync(_addressSuffix, objectContent).Result;

                if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                    throw new ExcecaoAcessoApi();

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    return httpResponseMessage;

                result = httpResponseMessage.Content.ReadAsStringAsync().Result;
                var jsonErrorDataResponse = JsonConvert.DeserializeObject<JsonErrorDataResponse>(result);
                throw new ExcecaoRestful(jsonErrorDataResponse, httpResponseMessage.StatusCode);
            }
            catch (ExcecaoRestful)
            {
                throw;
            }
            catch (Exception)
            {
                throw new Exception(result);
            }
        }

        private HttpResponseMessage DeleteSync()
        {
            string result = string.Empty;

            try
            {
                var httpResponseMessage = _httpClient.DeleteAsync(_addressSuffix).Result;

                if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                    throw new ExcecaoAcessoApi();

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    return httpResponseMessage;

                result = httpResponseMessage.Content.ReadAsStringAsync().Result;
                var jsonErrorDataResponse = JsonConvert.DeserializeObject<JsonErrorDataResponse>(result);
                throw new ExcecaoRestful(jsonErrorDataResponse, httpResponseMessage.StatusCode);
            }
            catch (ExcecaoRestful)
            {
                throw;
            }
            catch (Exception)
            {
                throw new Exception(result);
            }
        }

        private StringContent CreateJsonObjectContent(object model)
        {
            return new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, JsonMediaType);
        }
    }
}