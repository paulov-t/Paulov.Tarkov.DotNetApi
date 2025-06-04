using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using System;
using System.Threading.Tasks;

namespace BSGHelperLibrary.ResponseModels
{
    /// <summary>
    /// Represents an HTTP response that formats a specified body value as JSON, typically used to return error-related
    /// data.
    /// </summary>
    /// <remarks>This class provides functionality for formatting the response body in a consistent JSON
    /// structure, including error codes and messages. It supports customization of the HTTP status code, content type,
    /// and serializer settings.</remarks>
    public class BSGErrorBodyResult : ActionResult, IStatusCodeActionResult
    {
        /// <summary>
        /// Creates a new <see cref="BSGSuccessBodyResult"/> with the given <paramref name="bodyValue"/>.
        /// </summary>
        /// <param name="bodyValue">The value to format as JSON.</param>
        public BSGErrorBodyResult(int errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
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
        /// Gets or sets the error code associated with the current operation.
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <inheritdoc />
        public override Task ExecuteResultAsync(ActionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var responseText = "{ \"err\": " + ErrorCode + ", \"errmsg\": \"" + ErrorMessage + "\", \"data\": null }";

            return HttpBodyConverters.CompressStringIntoResponseBody(responseText, context.HttpContext.Request, context.HttpContext.Response);
        }
    }
}