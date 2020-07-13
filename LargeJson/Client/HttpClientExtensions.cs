using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LargeJson.Client
{

    //Just some copies of extension methods so we can use our copied TranscodingReadStream for performance checks


	public static class HttpClientExtensions
    {
        public static Task<TValue> GetFromJsonAsync2<TValue>(this HttpClient client, string? requestUri, JsonSerializerOptions? options, CancellationToken cancellationToken = default)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            Task<HttpResponseMessage> taskResponse = client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return GetFromJsonAsyncCore<TValue>(taskResponse, options, cancellationToken);
        }

        private static async Task<T> GetFromJsonAsyncCore<T>(Task<HttpResponseMessage> taskResponse, JsonSerializerOptions? options, CancellationToken cancellationToken)
        {
            using (HttpResponseMessage response = await taskResponse.ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                // Nullable forgiving reason:
                // GetAsync will usually return Content as not-null.
                // If Content happens to be null, the extension will throw.
                return await response.Content!.ReadFromJsonAsync<T>(options, cancellationToken).ConfigureAwait(false);
            }
        }

        public static Task<T> ReadFromJsonAsync<T>(this HttpContent content, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            ValidateContent(content);
            Debug.Assert(content.Headers.ContentType != null);
            Encoding? sourceEncoding = GetEncoding(content.Headers.ContentType.CharSet);

            return ReadFromJsonAsyncCore<T>(content, sourceEncoding, options, cancellationToken);
        }


        private static async Task<T> ReadFromJsonAsyncCore<T>(HttpContent content, Encoding? sourceEncoding, JsonSerializerOptions? options, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            Stream contentStream = await content.ReadAsStreamAsync().ConfigureAwait(false);

            // Wrap content stream into a transcoding stream that buffers the data transcoded from the sourceEncoding to utf-8.
            if (sourceEncoding != null && sourceEncoding != Encoding.UTF8)
            {
#if NETCOREAPP
                contentStream = Encoding.CreateTranscodingStream(contentStream, innerStreamEncoding: sourceEncoding, outerStreamEncoding: Encoding.UTF8);
#else
                contentStream = new TranscodingReadStream(contentStream, sourceEncoding);
#endif
            }


            var l = contentStream.Length;


            var dur0 = sw.Elapsed;

            using (contentStream)
            {
                var data= await JsonSerializer.DeserializeAsync<T>(contentStream, options ?? s_defaultSerializerOptions, cancellationToken).ConfigureAwait(false);

                var dur1 = sw.Elapsed;
                Console.WriteLine(l);
                Console.WriteLine(dur0);
                Console.WriteLine(dur1);

                return data;
            }
        }

        internal static readonly JsonSerializerOptions s_defaultSerializerOptions
           = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        internal static Encoding? GetEncoding(string? charset)
        {
            Encoding? encoding = null;

            if (charset != null)
            {
                try
                {
                    // Remove at most a single set of quotes.
                    if (charset.Length > 2 && charset[0] == '\"' && charset[charset.Length - 1] == '\"')
                    {
                        encoding = Encoding.GetEncoding(charset.Substring(1, charset.Length - 2));
                    }
                    else
                    {
                        encoding = Encoding.GetEncoding(charset);
                    }
                }
                catch (ArgumentException e)
                {
                    throw new InvalidOperationException("SR.CharSetInvalid, e");
                }

                Debug.Assert(encoding != null);
            }

            return encoding;
        }

        private static void ValidateContent(HttpContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            string? mediaType = content.Headers.ContentType?.MediaType;

            if (mediaType == null ||
                !mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) &&
                !IsValidStructuredSyntaxJsonSuffix(mediaType.AsSpan()))
            {
                throw new NotSupportedException("SR.Format(SR.ContentTypeNotSupported, mediaType)");
            }
        }

        private static bool IsValidStructuredSyntaxJsonSuffix(ReadOnlySpan<char> mediaType)
        {
            int index = 0;
            int typeLength = mediaType.IndexOf('/');

            ReadOnlySpan<char> type = mediaType.Slice(index, typeLength);
            if (typeLength < 0 ||
                type.CompareTo("application".AsSpan(), StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }

            index += typeLength + 1;
            int suffixStart = mediaType.Slice(index).IndexOf('+');

            // Empty prefix subtype ("application/+json") not allowed.
            if (suffixStart <= 0)
            {
                return false;
            }

            index += suffixStart + 1;
            ReadOnlySpan<char> suffix = mediaType.Slice(index);
            if (suffix.CompareTo("json".AsSpan(), StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }

            return true;
        }

    }
}
