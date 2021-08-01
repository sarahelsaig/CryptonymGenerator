// Decompiled with JetBrains decompiler
// Type: Lexicala.NET.LexicalaClient
// Assembly: Lexicala.NET, Version=1.5.0.0, Culture=neutral, PublicKeyToken=null

// All I did here was to remove the "SearchText cannot be empty" because it totally can and I need it that way.
#nullable disable

using Lexicala.NET.Request;
using Lexicala.NET.Response;
using Lexicala.NET.Response.Entries.JsonConverters;
using Lexicala.NET.Response.Languages;
using Lexicala.NET.Response.Me;
using Lexicala.NET.Response.Search;
using Lexicala.NET.Response.Test;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Lexicala.NET;

namespace CryptonymGenerator.Services
{
    public class LexicalaClientAlt : ILexicalaClient
    {
        private readonly HttpClient _httpClient;

        public LexicalaClientAlt(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<TestResponse> TestAsync() =>
            JsonConvert.DeserializeObject<TestResponse>(await _httpClient.GetStringAsync("/test"));

        public async Task<MeResponse> MeAsync() =>
            JsonConvert.DeserializeObject<MeResponse>(await _httpClient.GetStringAsync("/users/me"));

        public async Task<LanguagesResponse> LanguagesAsync() =>
            JsonConvert.DeserializeObject<LanguagesResponse>(await _httpClient.GetStringAsync("/languages"));

        public Task<SearchResponse> BasicSearchAsync(
            string searchText,
            string sourceLanguage,
            string etag = null)
        {
            if (sourceLanguage.Length != 2)
            {
                throw new ArgumentException(
                    "Invalid language code provided (" + sourceLanguage + "), a valid language code is two characters",
                    nameof(sourceLanguage));
            }

            return ExecuteSearch("/search?language=" + sourceLanguage + "&text=" + searchText, etag);
        }

        public Task<SearchResponse> AdvancedSearchAsync(
            AdvancedSearchRequest searchRequest)
        {
            string language = searchRequest.Language;
            if ((language != null ? (language.Length != 2 ? 1 : 0) : 1) != 0)
            {
                throw new ArgumentException("Invalid language code provided (" + searchRequest.Language +
                                            "), a valid language code is two characters");
            }

            StringBuilder stringBuilder1 =
                new StringBuilder("/search?language=" + searchRequest.Language + "&text=" + searchRequest.SearchText);
            stringBuilder1.Append("&source=" + searchRequest.Source);
            if (searchRequest.Analyzed)
            {
                stringBuilder1.Append("&analyzed=true");
            }

            if (searchRequest.Monosemous)
            {
                stringBuilder1.Append("&monosemous=true");
            }

            if (searchRequest.Polysemous)
            {
                stringBuilder1.Append("&polysemous=true");
            }

            if (searchRequest.Morph)
            {
                stringBuilder1.Append("&morph=true");
            }

            if (!string.IsNullOrEmpty(searchRequest.Pos))
            {
                stringBuilder1.Append("&pos=" + searchRequest.Pos);
            }

            if (!string.IsNullOrEmpty(searchRequest.Number))
            {
                stringBuilder1.Append("&number=" + searchRequest.Number);
            }

            if (!string.IsNullOrEmpty(searchRequest.Gender))
            {
                stringBuilder1.Append("&gender=" + searchRequest.Gender);
            }

            if (!string.IsNullOrEmpty(searchRequest.Subcategorization))
            {
                stringBuilder1.Append("&subcategorization=" + searchRequest.Subcategorization);
            }

            int num;
            if (searchRequest.Page > 1)
            {
                StringBuilder stringBuilder2 = stringBuilder1;
                num = searchRequest.Page;
                string str = "&page=" + num.ToString();
                stringBuilder2.Append(str);
            }

            if (searchRequest.PageLength != 10 && searchRequest.PageLength > 0 && searchRequest.PageLength <= 30)
            {
                StringBuilder stringBuilder2 = stringBuilder1;
                num = searchRequest.PageLength;
                string str = "&page-length=" + num.ToString();
                stringBuilder2.Append(str);
            }

            if (searchRequest.Sample > 0)
            {
                StringBuilder stringBuilder2 = stringBuilder1;
                num = searchRequest.Sample;
                string str = "&sample=" + num.ToString();
                stringBuilder2.Append(str);
            }

            return ExecuteSearch(stringBuilder1.ToString(), searchRequest.ETag);
        }

        private async Task<SearchResponse> ExecuteSearch(
            string querystring,
            string etag)
        {
            HttpResponseMessage response;
            SearchResponse searchResponse1;
            using (HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, querystring))
            {
                if (etag != null)
                {
                    httpRequest.Headers.Add("If-None-Match", etag);
                }

                response = await _httpClient.SendAsync(httpRequest);
                try
                {
                    response.EnsureSuccessStatusCode();
                    SearchResponse searchResponse2 =
                        JsonConvert.DeserializeObject<SearchResponse>(await response.Content.ReadAsStringAsync(),
                            SearchResponseJsonConverter.Settings);
                    searchResponse2!.Metadata = LexicalaClientAlt.GetResponseMetadata(response.Headers);
                    searchResponse1 = searchResponse2;
                }
                finally
                {
                    response.Dispose();
                }
            }

            // ReSharper disable once RedundantAssignment
            response = null;
            return searchResponse1;
        }

        public async Task<Lexicala.NET.Response.Entries.Entry> GetEntryAsync(
            string entryId,
            string etag = null)
        {
            HttpResponseMessage response;
            Lexicala.NET.Response.Entries.Entry entry1;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/entries/" + entryId))
            {
                if (etag != null)
                {
                    request.Headers.Add("If-None-Match", etag);
                }

                response = await _httpClient.SendAsync(request);
                try
                {
                    response.EnsureSuccessStatusCode();
                    Lexicala.NET.Response.Entries.Entry entry2 =
                        JsonConvert.DeserializeObject<Lexicala.NET.Response.Entries.Entry>(
                            await response.Content.ReadAsStringAsync(), EntryResponseJsonConverter.Settings);
                    entry2!.Metadata = LexicalaClientAlt.GetResponseMetadata(response.Headers);
                    entry1 = entry2;
                }
                finally
                {
                    response.Dispose();
                }
            }

            // ReSharper disable once RedundantAssignment
            response = null;
            return entry1;
        }

        private static ResponseMetadata GetResponseMetadata(HttpResponseHeaders headers)
        {
            return new ResponseMetadata()
            {
                ETag = headers.ETag?.Tag,
                RateLimits = new RateLimits()
                {
                    DailyLimitRemaining = ParseRateLimitHeader("X-RateLimit-DailyLimit-Remaining"),
                    DailyLimit = ParseRateLimitHeader("X-RateLimit-DailyLimit"),
                    Limit = ParseRateLimitHeader("X-RateLimit-Limit"),
                    Remaining = ParseRateLimitHeader("X-RateLimit-Remaining"),
                },
            };

            int ParseRateLimitHeader(string header)
            {
                IEnumerable<string> values;
                int result;
                // ReSharper disable PossibleMultipleEnumeration
                return headers.TryGetValues(header, out values) && values.Count() == 1 &&
                       int.TryParse(values.First(), out result)
                    ? result
                    : -1;
            }
        }
    }
}