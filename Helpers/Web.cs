﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GaleForceCore.Helpers
{
    public static class Web
    {
        /// <summary>
        /// Gets the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="timeoutSeconds">The timeout in seconds.</param>
        /// <returns>System.String.</returns>
        public static async Task<string> Get(string url, int timeoutSeconds = 5)
        {
            var uri = new Uri(url);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000 * timeoutSeconds);

            string result;

            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(uri, cts.Token))
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">Bearer token if needed</param>
        /// <param name="timeoutSeconds">The timeout in seconds.</param>
        /// <returns>System.String.</returns>
        public static async Task<string> Get(
            string url,
            Dictionary<string, string> headers = null,
            string token = null,
            int timeoutSeconds = 5,
            bool addAPIKey = true)
        {
            var uri = new Uri(url);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000 * timeoutSeconds);

            string result;

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                if (headers != null && headers.Count > 0)
                {
                    if (headers.ContainsKey("Content-Type"))
                    {
                        request.Headers.Accept
                            .Add(new MediaTypeWithQualityHeaderValue(headers["Content-Type"]));
                    }

                    if (token != null)
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue(
                            "Bearer",
                            token);
                    }

                    if (headers.ContainsKey("Authentication") && addAPIKey)
                    {
                        request.Headers.Add("Authentication", "APIKey " + headers["Authentication"]);
                    }

                    foreach (var header in headers)
                    {
                        if (header.Key != "Content-Type" &&
                            ((header.Key != "Authentication") || !addAPIKey))
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }
                    }
                }

                using (var response = await client.SendAsync(request, cts.Token))
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }

            return result;
        }

        /// <summary>
        /// Puts the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="body">The body.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="token">The bearer token.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="method">The method.</param>
        /// <param name="timeoutSeconds">The timeout in seconds.</param>
        /// <returns>System.String.</returns>
        public static async Task<string> Put(
            string url,
            string body,
            Dictionary<string, string> headers = null,
            string token = null,
            string contentType = "application/x-www-form-urlencoded",
            HttpMethod method = null,
            int timeoutSeconds = 15)
        {
            return await Post(
                url,
                body,
                headers,
                token,
                contentType,
                HttpMethod.Put,
                timeoutSeconds: timeoutSeconds);
        }

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

            using (var client = new HttpClient())
            {
                method = method ?? HttpMethod.Post;
                var request = new HttpRequestMessage(method, url);

                if (headers != null && headers.Count > 0)
                {
                    if (headers.ContainsKey("Content-Type"))
                    {
                        request.Headers.Accept
                            .Add(new MediaTypeWithQualityHeaderValue(headers["Content-Type"]));
                    }

                    if (headers.ContainsKey("Authentication"))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue(
                            "Bearer",
                            token);
                        request.Headers.Add("Authentication", "APIKey " + headers["Authentication"]);
                    }
                }

                request.Content = new StringContent(body, Encoding.UTF8, contentType);

                using (var response = await client.SendAsync(request, cts.Token))
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }

            return result;
        }

        public static async Task<Tuple<string, HttpStatusCode>> GetResult(
            string url,
            Dictionary<string, string> headers = null,
            int timeoutSeconds = 15)
        {
            return await PostResult(url, null, headers, null, HttpMethod.Get, timeoutSeconds);
        }

        public static async Task<Tuple<string, HttpStatusCode>> PostResult(
            string url,
            string body,
            Dictionary<string, string> headers = null,
            string contentType = "application/x-www-form-urlencoded",
            HttpMethod method = null,
            int timeoutSeconds = 15)
        {
            var result = new ContentResult();

            var uri = new Uri(url); // validates url
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000 * timeoutSeconds);

            using (var client = new HttpClient())
            {
                method = method ?? HttpMethod.Post;
                var request = new HttpRequestMessage(method, url);

                if (headers != null && headers.Count > 0)
                {
                    if (headers.ContainsKey("Content-Type"))
                    {
                        request.Headers.Accept
                            .Add(new MediaTypeWithQualityHeaderValue(headers["Content-Type"]));
                    }
                }

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                if (body != null)
                {
                    byte[] dataStream = Encoding.UTF8.GetBytes(body);
                    request.Content = new ByteArrayContent(dataStream);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    request.Content.Headers.ContentEncoding.Add(Encoding.UTF8.WebName);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));

                    // new StringContent(body, Encoding.UTF8, contentType);
                }

                using (var response = await client.SendAsync(request, cts.Token))
                {
                    result.Content = await response.Content.ReadAsStringAsync();
                    result.Status = response.StatusCode;
                }
            }

            return new Tuple<string, HttpStatusCode>(result.Content, result.Status);
        }
    }

    public class ContentResult
    {
        public string Content { get; set; }

        public HttpStatusCode Status { get; set; }
    }
}
