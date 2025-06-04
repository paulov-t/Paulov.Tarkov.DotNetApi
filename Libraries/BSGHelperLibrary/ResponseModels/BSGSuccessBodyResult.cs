#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using System;
using System.Threading.Tasks;

namespace BSGHelperLibrary.ResponseModels
{
    public class BSGSuccessBodyResult : ActionResult, IStatusCodeActionResult
    {
        /// <summary>
        /// Creates a new <see cref="BSGSuccessBodyResult"/> with the given <paramref name="bodyValue"/>.
        /// </summary>
        /// <param name="bodyValue">The value to format as JSON.</param>
        public BSGSuccessBodyResult(object? bodyValue)
        {
            BodyValue = bodyValue;
        }

        /// <summary>
        /// <summary>
        /// Gets or sets the <see cref="Net.Http.Headers.MediaTypeHeaderValue"/> representing the Content-Type header of the response.
        /// </summary>
        public string? ContentType { get; set; } = "application/json";

        /// <summary>
        /// Gets or sets the serializer settings.
        /// <para>
        /// When using <c>System.Text.Json</c>, this should be an instance of <see cref="JsonSerializerOptions" />
        /// </para>
        /// <para>
        /// When using <c>Newtonsoft.Json</c>, this should be an instance of <c>JsonSerializerSettings</c>.
        /// </para>
        /// </summary>
        public object? SerializerSettings { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public int? StatusCode { get; set; } = 200;

        /// <summary>
        /// Gets or sets the value to be formatted.
        /// </summary>
        public object? BodyValue { get; set; }

        /// <inheritdoc />
        public override Task ExecuteResultAsync(ActionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var responseText = "";

            if (BodyValue != null)
            {
                if ((BodyValue is Array || BodyValue.GetType().FullName.Contains("List`1")))
                {
                    var data = BodyValue != null ? BodyValue?.ToJson() : null;
                    responseText = "{ \"err\": 0, \"errmsg\": null, \"data\": " + data + " }";
                }
                else if (BodyValue is string && !BodyValue.ToString().StartsWith("{") && !BodyValue.ToString().StartsWith("["))
                {
                    var data = BodyValue != null ? BodyValue?.ToString()?.Replace("\r", "").Replace("\n", "") : "";
                    responseText = "{ \"err\": 0, \"errmsg\": null, \"data\": \"" + data + "\" }";
                }
                else
                {
                    var data = BodyValue != null ? BodyValue?.ToString()?.Replace("\r", "").Replace("\n", "") : "";
                    responseText = "{ \"err\": 0, \"errmsg\": null, \"data\": " + data + " }";
                }
            }

            return HttpBodyConverters.CompressStringIntoResponseBody(responseText, context.HttpContext.Request, context.HttpContext.Response);
        }
    }
}

#nullable disable