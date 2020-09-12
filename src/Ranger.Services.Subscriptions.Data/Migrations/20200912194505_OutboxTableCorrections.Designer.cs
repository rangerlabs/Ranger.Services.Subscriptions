﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Ranger.Services.Subscriptions.Data;

namespace Ranger.Services.Subscriptions.Data.Migrations
{
    [DbContext(typeof(SubscriptionsDbContext))]
    [Migration("20200912194505_OutboxTableCorrections")]
    partial class OutboxTableCorrections
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.7")
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

            modelBuilder.Entity("Ranger.RabbitMQ.OutboxMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("InsertedAt")
                        .HasColumnName("inserted_at")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("MessageId")
                        .HasColumnName("message_id")
                        .HasColumnType("integer");

                    b.Property<bool>("Nacked")
                        .HasColumnName("nacked")
                        .HasColumnType("boolean");

                    b.HasKey("Id")
                        .HasName("pk_outbox_messages");

                    b.HasIndex("MessageId")
                        .IsUnique()
                        .HasName("ix_outbox_messages_message_id");

                    b.ToTable("outbox_messages");
                });

            modelBuilder.Entity("Ranger.RabbitMQ.RangerRabbitMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Body")
                        .IsRequired()
                        .HasColumnName("body")
                        .HasColumnType("text");

                    b.Property<string>("Headers")
                        .IsRequired()
                        .HasColumnName("headers")
                        .HasColumnType("text");

                    b.Property<float>("MessageVersion")
                        .HasColumnName("message_version")
                        .HasColumnType("real");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnName("type")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_ranger_rabbit_messages");

                    b.ToTable("ranger_rabbit_messages");
                });

            modelBuilder.Entity("Ranger.Services.Subscriptions.Data.PlanLimits", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("Accounts")
                        .HasColumnName("accounts")
                        .HasColumnType("integer");

                    b.Property<int>("Geofences")
                        .HasColumnName("geofences")
                        .HasColumnType("integer");

                    b.Property<int>("Integrations")
                        .HasColumnName("integrations")
                        .HasColumnType("integer");

                    b.Property<int>("Projects")
                        .HasColumnName("projects")
                        .HasColumnType("integer");

                    b.Property<int>("TenantSubscriptionId")
                        .HasColumnName("tenant_subscription_id")
                        .HasColumnType("integer");

                    b.HasKey("Id")
                        .HasName("pk_plan_limits");

                    b.HasIndex("TenantSubscriptionId")
                        .IsUnique()
                        .HasName("ix_plan_limits_tenant_subscription_id");

                    b.ToTable("plan_limits");
                });

            modelBuilder.Entity("Ranger.Services.Subscriptions.Data.TenantSubscription", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<bool>("Active")
                        .HasColumnName("active")
                        .HasColumnType("boolean");

                    b.Property<string>("CustomerId")
                        .IsRequired()
                        .HasColumnName("customer_id")
                        .HasColumnType("text");

                    b.Property<DateTime>("OccurredAt")
                        .HasColumnName("occurred_at")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("PlanId")
                        .IsRequired()
                        .HasColumnName("plan_id")
                        .HasColumnType("text");

                    b.Property<DateTime?>("ScheduledCancellationDate")
                        .HasColumnName("scheduled_cancellation_date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("SubscriptionId")
                        .IsRequired()
                        .HasColumnName("subscription_id")
                        .HasColumnType("text");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnName("tenant_id")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_tenant_subscriptions");

                    b.HasIndex("CustomerId")
                        .IsUnique();

                    b.HasIndex("SubscriptionId")
                        .IsUnique();

                    b.HasIndex("TenantId")
                        .IsUnique();

                    b.ToTable("tenant_subscriptions");
                });

            modelBuilder.Entity("Ranger.RabbitMQ.OutboxMessage", b =>
                {
                    b.HasOne("Ranger.RabbitMQ.RangerRabbitMessage", "Message")
                        .WithOne("OutboxMessage")
                        .HasForeignKey("Ranger.RabbitMQ.OutboxMessage", "MessageId")
                        .HasConstraintName("fk_outbox_messages_ranger_rabbit_messages_message_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Ranger.Services.Subscriptions.Data.PlanLimits", b =>
                {
                    b.HasOne("Ranger.Services.Subscriptions.Data.TenantSubscription", "TenantSubscription")
                        .WithOne("PlanLimits")
                        .HasForeignKey("Ranger.Services.Subscriptions.Data.PlanLimits", "TenantSubscriptionId")
                        .HasConstraintName("fk_plan_limits_tenant_subscriptions_tenant_subscription_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
