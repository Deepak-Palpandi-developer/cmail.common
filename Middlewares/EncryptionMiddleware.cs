using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Text;

namespace Cgmail.Common.Middlewares
{
    public class EncryptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;


        public EncryptionMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context, IServiceProvider serviceProvider)
        {

            if (context.Request.Path.StartsWithSegments("/emailHub"))
            {
                await _next(context);
                return;
            }

            bool enableEncryption = _configuration.GetValue<bool>("EnableEncryption");

            if (!enableEncryption)
            {
                await _next(context);

                return;
            }
            var hmacService = serviceProvider.GetRequiredService<IHmacService>();

            if (context.Request.ContentType == "application/json")
            {
                context.Request.Body = await DecryptRequest(context.Request, hmacService);
            }

            var originalResponseBody = context.Response.Body;

            using (var newResponseBody = new MemoryStream())
            {
                context.Response.Body = newResponseBody;

                await _next(context);

                await EncryptResponse(context, hmacService, originalResponseBody);
            }
        }

        private async Task<Stream> DecryptRequest(HttpRequest request, IHmacService hmacService)
        {
            request.EnableBuffering();

            var body = await new StreamReader(request.Body).ReadToEndAsync();

            request.Body.Position = 0;

            var decodedBody = JsonConvert.DeserializeObject<string>(body) ?? string.Empty;

            string key = _configuration.GetValue<string>("SecretKey") ?? string.Empty;

            var data = hmacService.ParseEncryptedRequest(decodedBody);

            var decryptedData = hmacService.DecryptAndDeserialize<object>(
                data.EncryptedData,
                data.Hmac,
                key,
                data.Iv);

            var decryptedJson = JsonConvert.SerializeObject(decryptedData);

            return new MemoryStream(Encoding.UTF8.GetBytes(decryptedJson));
        }

        private async Task EncryptResponse(HttpContext context, IHmacService hmacService, Stream originalResponseBody)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string key = _configuration.GetValue<string>("SecretKey") ?? string.Empty;

            if (!string.IsNullOrEmpty(responseBody))
            {
                var secureResponse = hmacService.EncryptData(responseBody, key);

                var encryptedPayload = $"{secureResponse.EncryptedData}(/=/){secureResponse.Iv}(/=/){secureResponse.Hmac}";

                string jsonResponse = System.Text.Json.JsonSerializer.Serialize(encryptedPayload);

                context.Response.ContentType = "application/json";
                await originalResponseBody.WriteAsync(Encoding.UTF8.GetBytes(jsonResponse));
            }
            else
            {
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                await context.Response.Body.CopyToAsync(originalResponseBody);
            }

            context.Response.Body = originalResponseBody;
        }

    }
}
