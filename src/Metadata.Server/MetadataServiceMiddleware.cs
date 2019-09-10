using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zhichkin.Hermes.Model;
using Zhichkin.Hermes.Services;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Services;

namespace Zhichkin.Metadata.Server
{
    public static class MetadataServiceMiddlewareExtentions
    {
        public static IApplicationBuilder UseMetadataServiceMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MetadataServiceMiddleware>();
        }
    }

    public sealed class MetadataServiceMiddleware
    {
        private readonly ILogger<MetadataServiceMiddleware> _logger;
        private Dictionary<string, Request> routes = new Dictionary<string, Request>();
        private readonly IHermesService Hermes = new HermesService();
        private readonly IMetadataService Metadata = new MetadataService();
        private readonly SerializationService Serializer = new SerializationService();

        private readonly RequestDelegate _next;
        public MetadataServiceMiddleware(RequestDelegate next, ILogger<MetadataServiceMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method == "GET" || context.Request.Method == "POST")
            {
                //if (routes.Count == 0)
                //{
                routes = Metadata.GetRequests();
                //}

                Request request = null;
                string json = string.Empty;

                _logger.LogInformation("Request path: {0}", context.Request.Path);

                if (routes.TryGetValue(context.Request.Path, out request))
                {
                    json = request.ParseTree;
                    QueryExpression query = Serializer.FromJson(json);

                    JObject parameters = this.ReadParameters(context);
                    if (parameters != null && query.Parameters != null && query.Parameters.Count > 0)
                    {
                        this.SetParameters(parameters, query);
                    }

                    QueryExecutor executor = new QueryExecutor(query);
                    try
                    {
                        var result = executor.Build().ExecuteAsRowData();
                        json = JsonConvert.SerializeObject(result);
                    }
                    catch (Exception ex)
                    {
                        json = Program.GetErrorText(ex);
                    }
                }
                else
                {
                    _logger.LogInformation("Requested path not found.");
                    _logger.LogInformation("Available paths are:");
                    foreach (string route in routes.Keys)
                    {
                        _logger.LogInformation(route);
                    }
                }

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(json);
            }
            else
            {
                await _next(context);
            }
        }

        private JObject ReadParameters(HttpContext context)
        {
            JObject parameters = null;

            using (var reader = new StreamReader(context.Request.Body))
            {
                string body = reader.ReadToEnd();

                if (!string.IsNullOrWhiteSpace(body))
                {
                    parameters = JsonConvert.DeserializeObject<JObject>(body);
                }
            }
            return parameters;
        }
        private void SetParameters(JObject parameters, QueryExpression query)
        {
            //Dictionary<string, object> bag = new Dictionary<string, object>();
            //{
            //	"Булево":true,
            //  "Строка":"Тест",
            //	"ЦелоеЧисло":123,
            //	"ДробноеЧисло":123.45,
            //	"Дата":"2019-08-01T19:15:00",
            //	"Неопределено":null
            //}

            JsonSerializer serializer = JsonSerializer.Create();
            foreach (JProperty property in parameters.Properties())
            {
                //bag.Add(property.Name, serializer.Deserialize(property.Value.CreateReader()));

                ParameterExpression parameter = query.Parameters.Where(p => p.Name == property.Name).FirstOrDefault();
                if (parameter != null)
                {
                    parameter.Value = serializer.Deserialize(property.Value.CreateReader());
                }
            }
        }
    }
}
