﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Jasper.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing.AspNetCoreIntegration
{
    public class can_bootstrap_a_bus_plus_aspnetcore_app_through_jasper_registry : IDisposable
    {
        private readonly JasperRuntime theRuntime = JasperRuntime.For<JasperServerApp>();

        public void Dispose()
        {
            theRuntime.Dispose();
        }

        [Fact]
        public async Task can_handle_an_http_request_through_Kestrel()
        {
            using (var client = new HttpClient())
            {
                var text = await client.GetStringAsync("http://localhost:3002");
                text.ShouldContain("Hello from a hybrid Jasper application");

            }
        }

        [Fact]
        public void has_the_bus()
        {
            theRuntime.Get<IMessageContext>().ShouldNotBeNull();
        }

        [Fact]
        public void captures_registrations_from_configure_registry()
        {
            theRuntime.Get<IFoo>().ShouldBeOfType<Foo>();
        }
    }

    public class SomeHandler
    {
        public void Handle(SomeMessage message)
        {

        }
    }

    public class SomeMessage
    {

    }

    // SAMPLE: ConfiguringAspNetCoreWithinJasperRegistry
    public class JasperServerApp : JasperHttpRegistry
    {
        public JasperServerApp()
        {
            Handlers.Discovery(x => x.DisableConventionalDiscovery());

            Http
                .UseKestrel()
                .UseUrls("http://localhost:3002")
                .UseStartup<Startup>();

        }
    }
    // ENDSAMPLE

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.Run(c => c.Response.WriteAsync("Hello from a hybrid Jasper application"));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IFoo, Foo>();
        }
    }

    public interface IFoo{}
    public class Foo : IFoo{}
}