using Autofac;
using Konbi.WindowServices.QRPayment.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;

namespace Konbi.WindowServices.QRPayment
{
    public class Program
    {

        static void Main(string[] args)
        {
            var container = AutofacConfig.Config();
            using (var scope = container.BeginLifetimeScope())
            {

                var startupType = string.Empty;
                var app = scope.Resolve<Application>();
                app.Run(args);
            }
        }

    

    }
}
