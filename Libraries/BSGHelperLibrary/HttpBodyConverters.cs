//using ComponentAce.Compression.Libs.zlib;
using ComponentAce.Compression.Libs.zlib;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SIT.BSGHelperLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Paulov.Tarkov.WebServer.DOTNET.Middleware
{
    public static class HttpBodyConverters
    {
        public static bool IsCompressed(byte[] Data)
        {
            // We need the first two bytes;
            // First byte:  Info (CM/CINFO) Header, should always be 0x78
            // Second byte: Flags (FLG) Header, should define our compression level.

            if (Data == null || Data.Length < 3 || Data[0] != 0x78)
            {
                return false;
            }

            switch (Data[1])
            {
                case 0x01:  // fastest
                case 0x5E:  // low
                case 0x9C:  // normal
                case 0xDA:  // max
                    return true;
            }

            return false;
        }

        public static async Task<byte[]> DecompressRequestBodyToBytes(HttpRequest request)
        {
            if (!request.Body.CanSeek)
                request.EnableBuffering();

            {

            }

            request.Body.Position = 0;
            var reader = new StreamReader(request.Body, Encoding.UTF8);
            var body = await reader.ReadToEndAsync().ConfigureAwait(false);
            request.Body.Position = 0;

            if (IsCompressed(((MemoryStream)request.Body).ToArray()))
            {

            }

            // This is the only way to handle Zlib versus Standard Json calls
            try
            {
                using ZLibStream zLibStream = new(request.Body, CompressionMode.Decompress);
                byte[] buffer = new byte[4096];
                await zLibStream.ReadAsync(buffer, 0, buffer.Length);
                return buffer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Debug.WriteLine(ex.ToString());
            }

            try
            {
                return Encoding.UTF8.GetBytes(body);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<Dictionary<string, object>> DecompressRequestBodyToDictionary(HttpRequest request)
        {
            if (!request.Body.CanSeek)
                request.EnableBuffering();

            request.Body.Position = 0;
            var reader = new StreamReader(request.Body, Encoding.UTF8);
            var body = await reader.ReadToEndAsync().ConfigureAwait(false);
            request.Body.Position = 0;

            // If we are Unity / Tarkov
            if (
                (request.Headers.ContainsKey("Content-Encoding") && request.Headers["Content-Encoding"] == "deflate")
                || (request.Headers.ContainsKey("user-agent") && request.Headers["user-agent"].ToString().StartsWith("Unity"))
                )
            {
                // This is the only way to handle Zlib versus Standard Json calls
                try
                {
                    using ZLibStream zLibStream = new(request.Body, CompressionMode.Decompress);
                    byte[] buffer = new byte[4096];
                    await zLibStream.ReadAsync(buffer, 0, buffer.Length);
                    var str = Encoding.UTF8.GetString(buffer);
                    var resultDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(str);
                    if (resultDict != null)
                        return resultDict;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Debug.WriteLine(ex.ToString());
                }
            }


            if (body.StartsWith('{') || body.StartsWith('['))
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(body);

            if (body.Contains('&'))
            {
                Dictionary<string, object> dictSplitItems = new Dictionary<string, object>();
                var splitBody = body.Split('&');
                foreach (var splitItem in splitBody)
                {
                    if (splitItem.Split('=').Length > 1)
                        dictSplitItems.Add(splitItem.Split('=')[0], splitItem.Split('=')[1]);
                }
                return dictSplitItems;
            }

            return null;

        }

        public static async Task<T> DecompressRequestBodyToType<T>(HttpRequest request)
        {
            if (!request.Body.CanSeek)
                request.EnableBuffering();

            request.Body.Position = 0;
            var reader = new StreamReader(request.Body, Encoding.UTF8);
            var body = await reader.ReadToEndAsync().ConfigureAwait(false);
            request.Body.Position = 0;

            // This is the only way to handle Zlib versus Standard Json calls
            try
            {
                using ZLibStream zLibStream = new(request.Body, CompressionMode.Decompress);
                byte[] buffer = new byte[4096];
                await zLibStream.ReadAsync(buffer, 0, buffer.Length);
                var str = Encoding.UTF8.GetString(buffer);
                if (BSGJsonHelpers.TrySITParseJson(str, out T result))
                    return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Debug.WriteLine(ex.ToString());
            }


            if ((body.StartsWith('{') || body.StartsWith('[')) && BSGJsonHelpers.TrySITParseJson(body, out T rBodyResult))
                return rBodyResult;

            return default(T);
        }

        public static async Task<(string, dynamic)> DecompressRequestBody(HttpRequest request)
        {
            if (!request.Body.CanSeek)
                request.EnableBuffering();

            request.Body.Position = 0;
            var reader = new StreamReader(request.Body, Encoding.UTF8);
            string resultString = await reader.ReadToEndAsync().ConfigureAwait(false);
            request.Body.Position = 0;

            dynamic resultDynamic = new ExpandoObject();

            // This is the only way to handle Zlib versus Standard Json calls
            try
            {
                using ZLibStream zLibStream = new(request.Body, CompressionMode.Decompress);
                byte[] buffer = new byte[4096];
                await zLibStream.ReadAsync(buffer, 0, buffer.Length);
                resultString = Encoding.UTF8.GetString(buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Debug.WriteLine(ex.ToString());
            }

            try
            {
                if (resultString.StartsWith("{"))
                    resultDynamic = JObject.Parse(resultString);
                else if (resultString.StartsWith("["))
                    resultDynamic = JArray.Parse(resultString);
            }
            catch
            {
            }

            return (resultString, resultDynamic);


        }

        public static async Task CompressDictionaryIntoResponseBody(Dictionary<string, object> dictionary, HttpRequest request, HttpResponse response)
        {
            await CompressStringIntoResponseBody(JsonConvert.SerializeObject(dictionary

                , new JsonSerializerSettings()
                {
                    Converters = new[] { new Newtonsoft.Json.Converters.StringEnumConverter() }
                    ,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                }

                ), request, response);
        }

        public static async Task CompressStringIntoResponseBody(string stringToConvert, HttpRequest request, HttpResponse response)
        {
            if (!string.IsNullOrEmpty(stringToConvert))
            {
                stringToConvert = stringToConvert.Trim();
            }

            if (!response.Headers.IsReadOnly)
            {
                // Must send application/json responses
                if (!response.Headers.TryGetValue("content-type", out StringValues contentType) || !contentType.Equals("application/json"))
                    response.Headers.ContentType = "application/json";

                // If we are not Unity / Tarkov, then instruct client to deflate
                if (request.Headers.TryGetValue("user-agent", out StringValues userAgent) && !userAgent.ToString().StartsWith("Unity"))
                    response.Headers.ContentEncoding = "deflate";
            }
            response.StatusCode = 200;

            // Zlib Compress the String
            if (!string.IsNullOrEmpty(stringToConvert))
            {
                //TODO: Look at streaming this instead of waiting for the entire compression before sending our response
                byte[] bytes = SimpleZlib.CompressToBytes(stringToConvert, 6);
                response.Headers.ContentLength = bytes.Length;
                await response.BodyWriter.WriteAsync(new ReadOnlyMemory<byte>(bytes));
            }

            GC.Collect();
        }

        public static async Task CompressNullIntoResponseBodyBSG(HttpRequest request, HttpResponse response)
        {
            Dictionary<string, object> BSGResponse = new();
            BSGResponse.Add("err", 0);
            BSGResponse.Add("errmsg", null);
            BSGResponse.Add("data", null);
            await CompressDictionaryIntoResponseBody(BSGResponse, request, response);
        }

        public static async Task CompressIntoResponseBodyBSG<T>(T model, HttpRequest request, HttpResponse response)
        {
            await CompressIntoResponseBodyBSG(model.SITToJson(), request, response);
        }

        public static async Task CompressIntoResponseBodyBSG(string data, HttpRequest request, HttpResponse response, int errorCode, string errorMessage)
        {
            var resp = "{ 'err': " + errorCode + ", 'errmsg': " + errorMessage + ", 'data': " + data + " }";
            await CompressStringIntoResponseBody(resp, request, response);
        }


        public static async Task CompressIntoResponseBodyBSG(string data, HttpRequest request, HttpResponse response)
        {
            data = SanitizeJson(data);
            var resp = "{ 'err': 0, 'errmsg':null, 'data': " + data + " }";
            await CompressStringIntoResponseBody(resp, request, response);
        }

        public static string SanitizeJson(string json)
        {
            return json.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
        }

        public static async Task CompressIntoResponseBodyBSG(Dictionary<string, object> dictionary, HttpRequest request, HttpResponse response)
        {
            Dictionary<string, object> BSGResponse = new();
            BSGResponse.Add("err", 0);
            BSGResponse.Add("errmsg", null);
            BSGResponse.Add("data", dictionary);
            await CompressDictionaryIntoResponseBody(BSGResponse, request, response);
        }

        public static void CompressIntoResponseBodyBSG(Dictionary<string, object> dictionary, ref HttpRequest request, ref HttpResponse response)
        {
            Dictionary<string, object> BSGResponse = new();
            BSGResponse.Add("err", 0);
            BSGResponse.Add("errmsg", null);
            BSGResponse.Add("data", dictionary);
            CompressDictionaryIntoResponseBody(BSGResponse, request, response).RunSynchronously();
        }

        public static async Task CompressIntoResponseBodyBSG(JObject obj, HttpRequest request, HttpResponse response)
        {
            Dictionary<string, object> BSGResponse = new();
            BSGResponse.Add("err", 0);
            BSGResponse.Add("errmsg", null);
            BSGResponse.Add("data", obj);
            await CompressDictionaryIntoResponseBody(BSGResponse, request, response);
        }

        public static async Task CompressDictionaryIntoResponseBodyBSG(Dictionary<string, object> dictionary, HttpRequest request, HttpResponse response)
        {
            await CompressIntoResponseBodyBSG(dictionary, request, response);
        }


    }
}
