using System.Xml.Serialization;
using System;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            //  app.MapHub<PresenceHub>("hubs/presence");
            try
            {
                var context = services.GetRequiredService<DataContext>();
                var userManger = services.GetRequiredService<UserManager<AppUser>>();
                var roleManger = services.GetRequiredService<RoleManager<AppRole>>();
                await context.Database.MigrateAsync();
                //context.Connections.RemoveRange(context.Connections);await context.Database.ExecuteSqlRawAsync("Truncate TABLE [Connections]"); not is working in sqlit                   
               // await context.Database.ExecuteSqlRawAsync("Truncate TABLE [Connections]");
                await Seed.ClearConnections(context);
                await Seed.SeedUsers(userManger, roleManger);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occured during migration");
            }

            await host.RunAsync();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });

    }
}
