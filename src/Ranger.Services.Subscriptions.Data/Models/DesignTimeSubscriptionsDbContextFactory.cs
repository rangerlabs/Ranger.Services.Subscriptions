using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Ranger.Services.Subscriptions.Data
{
    public class DesignTimeSubscriptionsDbContextFactory : IDesignTimeDbContextFactory<SubscriptionsDbContext>
    {
        public SubscriptionsDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var options = new DbContextOptionsBuilder<SubscriptionsDbContext>();
            options.UseNpgsql(config["cloudSql:ConnectionString"]);

            return new SubscriptionsDbContext(options.Options);
        }
    }
}