using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Netfox.Web.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var root = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                var file = Path.Combine(root, $"{new AssemblyName(e.Name).Name}.dll");
                if (File.Exists(file))
                    return Assembly.LoadFile(file);
                
                return null;
            };
            
            AppDomain.CurrentDomain.AssemblyLoad += (_, e) =>
            {
                // if (e.LoadedAssembly.FullName?.Contains("Netfox") ?? false)
                //     Console.WriteLine($">>> Loaded `{e.LoadedAssembly.GetName()}`");
            };
            
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((_, app) => app.AddUserSecrets<Program>())
                .ConfigureWebHostDefaults(app => app.UseStartup<AspStartup>())
                .Build()
                .Run();
        }
    }
}