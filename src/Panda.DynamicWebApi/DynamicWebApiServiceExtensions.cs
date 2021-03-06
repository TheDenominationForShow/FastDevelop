﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Com.Ctrip.Framework.Apollo;
using GW.ApiLoader.Utils.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Panda.DynamicWebApi.Attributes;
using Panda.DynamicWebApi.Helpers;

namespace Panda.DynamicWebApi
{
    /// <summary>
    /// Add Dynamic WebApi
    /// </summary>
    public static class DynamicWebApiServiceExtensions
    {
        /// <summary>
        /// Add Dynamic WebApi to Container
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options">configuration</param>
        /// <returns></returns>
        public static IServiceCollection AddDynamicWebApi(this IServiceCollection services, IConfiguration configuration, DynamicWebApiOptions options)
        {
            if (options == null)
            {
                throw new ArgumentException(nameof(options));
            }

            options.Valid();

            AppConsts.DefaultAreaName = options.DefaultAreaName;
            AppConsts.DefaultHttpVerb = options.DefaultHttpVerb;
            AppConsts.DefaultApiPreFix = options.DefaultApiPrefix;
            AppConsts.ControllerPostfixes = options.RemoveControllerPostfixes;
            AppConsts.ActionPostfixes = options.RemoveActionPostfixes;
            AppConsts.FormBodyBindingIgnoredTypes = options.FormBodyBindingIgnoredTypes;
            AppConsts.GetRestFulActionName = options.GetRestFulActionName;
            AppConsts.AssemblyDynamicWebApiOptions = options.AssemblyDynamicWebApiOptions;

            var partManager = services.GetSingletonInstanceOrNull<ApplicationPartManager>();

            if (partManager == null)
            {
                throw new InvalidOperationException("\"AddDynamicWebApi\" must be after \"AddMvc\".");
            }

            // Add a custom controller checker
            partManager.FeatureProviders.Add(new DynamicWebApiControllerFeatureProvider());

            services.Configure<MvcOptions>(o =>
            {
                // Register Controller Routing Information Converter
                o.Conventions.Add(new DynamicWebApiConvention());
            });
            services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), LoadConfig(configuration)));
            return services;
        }
        public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration, DynamicWebApiOptions options)
        {
            services.AddSwaggerGen(c =>
            {
                //to do xml 
                c.SwaggerDoc("v1", new OpenApiInfo { Title =  "接口", Version = "1" });
                c.IncludeXmlComments(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Directory.GetCurrentDirectory()}.xml"), true);
                c.DocumentFilter<SwaggerIgnoreFilter>();
                c.UseInlineDefinitionsForEnums();
            });

            services.AddSwaggerGenNewtonsoftSupport();
            return services;
        }
        public static IConfiguration LoadConfig( IConfiguration configuration)
        {
            var tmpconfigurationBuilder = new ConfigurationBuilder();
            tmpconfigurationBuilder.AddConfiguration(configuration);
            #region 判断有无apollo配置，如有则加载apollo配置
            var apolloSection = tmpconfigurationBuilder.Build().GetSection("apollo");
            var id = apolloSection["AppId"];
            //支持动态改变apollo
            if (id != null && id != "")
            {
                var tmpBuilder = new ConfigurationBuilder().AddApollo(apolloSection);
                var apolloNamespace = apolloSection["Namespace"] ?? string.Empty;
                foreach (var space in apolloNamespace.Split('|'))
                {
                    //获取apollo不同命名空间配置
                    tmpBuilder.AddNamespace(space);
                }
                //命令行配置 > appsetting配置 > Apollo 配置
                tmpBuilder.AddConfiguration(configuration);
                //加载serlog配置
                tmpBuilder.AddJsonFile("Serilog.json", true, true);
                configuration = tmpBuilder.Build();       
            }
            else
            {
                //加载serlog配置
                tmpconfigurationBuilder.AddJsonFile("Serilog.json", true, true);
                configuration = tmpconfigurationBuilder.Build();
            }
            return configuration;
            #endregion
        }
        public static IServiceCollection AddDynamicWebApi(this IServiceCollection services, IConfiguration configuration)
        {
            return AddDynamicWebApi(services, configuration, new DynamicWebApiOptions() );
        }

        public static IServiceCollection AddDynamicWebApi(this IServiceCollection services, IConfiguration configuration, Action<DynamicWebApiOptions> optionsAction )
        {
            var dynamicWebApiOptions = new DynamicWebApiOptions();

            optionsAction?.Invoke(dynamicWebApiOptions);

            return AddDynamicWebApi(services,configuration,dynamicWebApiOptions);
        }
        /// <summary>
        /// 注入基础中间件（带Swagger）
        /// </summary>
        /// <param name="app"></param>
        /// <param name="routePrefix"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json",  "接口");
            });
            return app;
        }
        public static void AddAutowired(this ContainerBuilder builder)
        {
            builder.RegisterBllModule();
        }
        /// <summary>
        /// 注册BaseFoo 和 Autowried 相关服务
        /// </summary>
        /// <param name="containerBuilder"></param>
        /// <param name="configuration"></param>
        public static void RegisterBllModule(this ContainerBuilder containerBuilder)
        {
            Assembly moduleAssembly = null;
            foreach (var module in AppDomain.CurrentDomain.GetAssemblies())
            {
                moduleAssembly = null;
                moduleAssembly = module;

                //autowired  class实现
                var propertySelector = new AutowiredPropertySelector();

                var typeAutowired = moduleAssembly.GetTypes().Where(t => t.GetCustomAttribute<AutowiredAttribute>() != null && !t.GetTypeInfo().IsAbstract);
                foreach (var t in typeAutowired)
                {
                    var interceptor = t.GetCustomAttribute<AutowiredAttribute>().InterceptorType;
                    var builder = containerBuilder.RegisterType(t);
                    builder.PropertiesAutowired(propertySelector).InstancePerLifetimeScope();
                    //if (t.GetInterfaces().Count() > 0)
                    //{
                    //    builder.AsImplementedInterfaces();
                    //}

                    //if (interceptor != null)
                    //{
                    //    builder.InterceptedBy(interceptor);
                    //    if (t.GetInterfaces().Count() > 0)
                    //    {
                    //        builder.EnableInterfaceInterceptors();
                    //    }
                    //    else
                    //    {
                    //        builder.EnableClassInterceptors();
                    //    }
                    //}
                }
            }
        }

    }
}