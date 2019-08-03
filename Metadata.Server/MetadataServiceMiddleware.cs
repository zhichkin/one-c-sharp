using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        private Dictionary<string, Request> routes = new Dictionary<string, Request>();
        private readonly IHermesService Hermes = new HermesService();
        private readonly IMetadataService Metadata = new MetadataService();
        private readonly SerializationService Serializer = new SerializationService();

        private readonly RequestDelegate _next;
        public MetadataServiceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method == "POST")
            {
                if (routes.Count == 0)
                {
                    routes = Metadata.GetRequests();
                }

                Request request = null;
                string json = string.Empty;

                /* http://localhost:5000/TestRequest */
                if (routes.TryGetValue(context.Request.Path, out request))
                {
                    json = request.ParseTree;
                    QueryExpression query = Serializer.FromJson(json);
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

                //string json = string.Empty;
                //QueryExpression query = null;

                //Request request = Hermes.GetTestRequest();
                //json = request.ParseTree;

                //query = Serializer.FromJson(json);
                //QueryExecutor executor = new QueryExecutor(query);
                
                //try
                //{
                //    var result = executor.Build().ExecuteAsRowData();

                //    json = JsonConvert.SerializeObject(result);
                //}
                //catch (Exception ex)
                //{
                //    json = Program.GetErrorText(ex);
                //    //context.Response.StatusCode = 500;
                //}

                await context.Response.WriteAsync(json);
            }
            //context.Response.StatusCode = 200;
            await _next(context);
        }
    }
}
