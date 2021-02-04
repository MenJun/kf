using Common.Filter;
using Microsoft.Web.Http.Routing;
using Swashbuckle.Application;
using System;
using System.Configuration;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Routing;

namespace Api
{
    public static class WebApiConfig
    {
        private static readonly string swaggerRootUrl = ConfigurationManager.AppSettings["swaggerRootUrl"];
        public static void Register(HttpConfiguration config)
        {
            // Web API 配置和服务
            config.Filters.Add(new ControllerExceptionFilterAttribute());
            config.Services.Replace(typeof(IExceptionLogger), new GlobalExceptionLogger());
            config.Services.Replace(typeof(IExceptionHandler), new GlobalExceptionHandler());

            AutoMapper.Mapper.Initialize(cfg => cfg.AddProfile<AutoMapperProfile>());

            config.Filters.Add(new LogFilterAttribute());

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            // Web API 路由
            var constraintResolver = new DefaultInlineConstraintResolver()
            {
                ConstraintMap =
                {
                     ["apiVersion"] = typeof( ApiVersionRouteConstraint )
                }
            };
            config.MapHttpAttributeRoutes(constraintResolver);
            config.AddApiVersioning();

            //var cors = new EnableCorsAttribute("http://znpz.net", "*", "*");
            //config.EnableCors(cors);

            var apiExplorer = config.AddVersionedApiExplorer(o => o.GroupNameFormat = "'v'VVV");
            var xmlPath = GetXmlCommentsPath();
            config.EnableSwagger(
                "{apiVersion}/swagger",
                swagger =>
                {
                    //swagger.RootUrl(req => swaggerRootUrl);
                    swagger.CustomProvider((x) => new SwaggerProvider(x, xmlPath));
                    swagger.IncludeXmlComments(xmlPath);
                    swagger.MultipleApiVersions(
                        (apiDescription, version) => apiDescription.GetGroupName() == version,
                        info =>
                        {
                            foreach (var group in apiExplorer.ApiDescriptions)
                            {
                                info.Version(group.Name, $"NEB_DH API {group.ApiVersion}");
                            }
                        });
                })
             .EnableSwaggerUi(swagger => {
                 swagger.EnableDiscoveryUrlSelector();
                 swagger.InjectJavaScript(Assembly.GetExecutingAssembly(), "Api.Scripts.swagger_lang.js");
             });

            config.Routes.MapHttpRoute(
            name: "Swagger UI",
            routeTemplate: "",
            defaults: null,
            constraints: null,
            handler: new RedirectHandler(SwaggerDocsConfig.DefaultRootUrlResolver, "swagger/ui/index"));

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }

        private static string GetXmlCommentsPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory + $"bin/{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        }
    }
}
