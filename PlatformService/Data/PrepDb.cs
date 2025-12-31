using PlatformService.Models;
using Microsoft.EntityFrameworkCore;


namespace PlatformService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd)
        {
            using (var serviceScope = app.ApplicationServices.CreateAsyncScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<ApplicationDbContext>(), isProd);
            }
        }

        private static void SeedData(ApplicationDbContext context, bool isProd)
        {
            if (isProd)
            {
                Console.WriteLine("--> Attempting to apply Migrations...");
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not run migrations: {ex.Message}");
                }
            }

            if (!context.Platforms.Any())
            {
                
                Console.WriteLine("--> Seeding Data.....");
                context.Platforms.AddRange(
                    new Platform() { Name = "Dot Net", Cost = "Free", Publisher = "Microsoft" },
                    new Platform() { Name = "Angular", Cost = "Free", Publisher = "Google" },
                    new Platform() { Name = "SQL Server", Cost = "Free", Publisher = "Microsoft" },
                    new Platform() { Name = "ReactJS", Cost = "Free", Publisher = "Meta" }
                );

                context.SaveChanges();
            }
            else
                Console.WriteLine("==> We Already have data");
            
        }
    }
}