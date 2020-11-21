using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GaleForceCore.Helpers
{
    public static class Web
    {
        public static async Task<string> Get(string url, int timeoutSeconds = 5)
        {
            var uri = new Uri(url);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000 * timeoutSeconds);

            string result;

            using(var client = new HttpClient())
            {
                using(var response = await client.GetAsync(uri, cts.Token))
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }

            return result;
        }

        public static async Task<string> Get(
            string url,
            Dictionary<string, string> headers = null,
            string token = null,
            int timeoutSeconds = 5)
        {
            var uri = new Uri(url);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000 * timeoutSeconds);

            string result;

            using(var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                if(headers != null && headers.Count > 0)
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(headers["Content-Type"]));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    request.Headers.Add("Authentication", "APIKey " + headers["Authentication"]);
                }

                using(var response = await client.SendAsync(request, cts.Token))
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }

            return result;
        }

        public static async Task<string> Put(
            string url,
            string body,
            Dictionary<string, string> headers = null,
            string token = null,
            string contentType = "application/x-www-form-urlencoded",
            HttpMethod method = null,
            int timeoutSeconds = 15)
        { return await Post(url, body, headers, token, contentType, HttpMethod.Put, timeoutSeconds: timeoutSeconds); }

        public static Task<string> PostJson(string url, string body, int timeoutSeconds = 15)
        {
            return Post(
                url,
                body,
                new Dictionary<string, string>() { { "Content-Type", "application/json" } },
                null,
                "application/json",
                timeoutSeconds: timeoutSeconds);
        }

        public static async Task<string> Post(
            string url,
            string body,
            Dictionary<string, string> headers = null,
            string token = null,
            string contentType = "application/x-www-form-urlencoded",
            HttpMethod method = null,
            int timeoutSeconds = 15)
        {
            var uri = new Uri(url);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000 * timeoutSeconds);

            string result;

            using(var client = new HttpClient())
            {
                method = method ?? HttpMethod.Post;
                var request = new HttpRequestMessage(method, url);

                if(headers != null && headers.Count > 0)
                {
                    if(headers.ContainsKey("Content-Type"))
                    {
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(headers["Content-Type"]));
                    }

                    if(headers.ContainsKey("Authentication"))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        request.Headers.Add("Authentication", "APIKey " + headers["Authentication"]);
                    }
                }

                request.Content = new StringContent(body, Encoding.UTF8, contentType);

                using(var response = await client.SendAsync(request, cts.Token))
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }

            return result;
        }
    }
}
