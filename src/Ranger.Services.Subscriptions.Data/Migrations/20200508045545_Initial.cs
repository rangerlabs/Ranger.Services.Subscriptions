using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Ranger.Services.Subscriptions.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data_protection_keys",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    friendly_name = table.Column<string>(nullable: true),
                    xml = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_protection_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_subscriptions",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<string>(nullable: false),
                    subscription_id = table.Column<string>(nullable: false),
                    customer_id = table.Column<string>(nullable: false),
                    plan_id = table.Column<string>(nullable: false),
                    active = table.Column<bool>(nullable: false),
                    scheduled_cancellation_date = table.Column<DateTime>(nullable: true),
                    occurred_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_subscriptions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plan_limits",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_subscription_id = table.Column<int>(nullable: false),
                    geofences = table.Column<int>(nullable: false),
                    integrations = table.Column<int>(nullable: false),
                    projects = table.Column<int>(nullable: false),
                    accounts = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan_limits", x => x.id);
                    table.ForeignKey(
                        name: "fk_plan_limits_tenant_subscriptions_tenant_subscription_id",
                        column: x => x.tenant_subscription_id,
                        principalTable: "tenant_subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_plan_limits_tenant_subscription_id",
                table: "plan_limits",
                column: "tenant_subscription_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_subscriptions_customer_id",
                table: "tenant_subscriptions",
                column: "customer_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_subscriptions_subscription_id",
                table: "tenant_subscriptions",
                column: "subscription_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_subscriptions_tenant_id",
                table: "tenant_subscriptions",
                column: "tenant_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_protection_keys");

            migrationBuilder.DropTable(
                name: "plan_limits");

            migrationBuilder.DropTable(
                name: "tenant_subscriptions");
        }
    }
}
