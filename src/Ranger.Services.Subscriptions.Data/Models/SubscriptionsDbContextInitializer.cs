using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ranger.Common;

namespace Ranger.Services.Subscriptions.Data
{
    public class SubscriptionsDbContextInitializer : ISubscriptionsDbContextInitializer
    {
        private readonly SubscriptionsDbContext context;

        public SubscriptionsDbContextInitializer(SubscriptionsDbContext context)
        {
            this.context = context;
        }

        public bool EnsureCreated()
        {
            return context.Database.EnsureCreated();
        }

        public void Migrate()
        {
            context.Database.Migrate();
        }

        public void Seed()
        {
        }
    }

    public interface ISubscriptionsDbContextInitializer
    {
        bool EnsureCreated();
        void Migrate();
        void Seed();
    }
}