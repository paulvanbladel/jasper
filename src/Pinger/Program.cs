﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper;
using Jasper.CommandLine;
using Jasper.Diagnostics;
using Jasper.Http;
using Jasper.Messaging;
using Jasper.Messaging.Transports.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oakton;
using TestMessages;

namespace Pinger
{
    class Program
    {
        static int Main(string[] args)
        {
            return JasperAgent.Run<JasperRegistry>(args, _ =>
            {
                _.Transports.LightweightListenerAt(2600);

                // Using static routing rules to start
                _.Publish.Message<PingMessage>().To("tcp://localhost:2601");

                _.Services.AddSingleton<IHostedService, PingSender>();

                _.Hosting
                    .UseUrls("http://localhost:5000")
                    .UseKestrel();
            });
        }
    }

    public class PongHandler
    {
        public void Handle(PongMessage message)
        {
            ConsoleWriter.Write(ConsoleColor.Cyan, "Got a pong back with name: " + message.Name);

        }
    }

    public class HomeEndpoint
    {
        public string Index()
        {
            return "Hello!";
        }

        public string get_hello()
        {
            return "hello.";
        }
    }

    public class PingSender : BackgroundService
    {
        private readonly IMessageContext _bus;

        public PingSender(IMessageContext bus)
        {
            _bus = bus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int count = 1;

            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    Thread.Sleep(1000);

                    await _bus.Send(new PingMessage
                    {
                        Name = "Message" + count++
                    });
                }
            }, stoppingToken);
        }
    }
}
