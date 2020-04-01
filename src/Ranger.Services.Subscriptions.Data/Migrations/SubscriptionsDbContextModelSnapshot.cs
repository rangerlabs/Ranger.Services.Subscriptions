﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Ranger.Services.Subscriptions.Data;

namespace Ranger.Services.Subscriptions.Data.Migrations
{
    [DbContext(typeof(SubscriptionsDbContext))]
    partial class SubscriptionsDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("FriendlyName")
                        .HasColumnName("friendly_name")
                        .HasColumnType("text");

                    b.Property<string>("Xml")
                        .HasColumnName("xml")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_data_protection_keys");

                    b.ToTable("data_protection_keys");
                });

            modelBuilder.Entity("TenantSubscription", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("PgsqlDatabaseUsername")
                        .IsRequired()
                        .HasColumnName("pgsql_database_username")
                        .HasColumnType("text");

                    b.Property<string>("PlanId")
                        .IsRequired()
                        .HasColumnName("plan_id")
                        .HasColumnType("text");

                    b.Property<string>("SubscriptionId")
                        .IsRequired()
                        .HasColumnName("subscription_id")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_tenant_subscriptions");

                    b.HasIndex("PgsqlDatabaseUsername")
                        .IsUnique();

                    b.HasIndex("SubscriptionId")
                        .IsUnique();

                    b.ToTable("tenant_subscriptions");
                });
#pragma warning restore 612, 618
        }
    }
}
