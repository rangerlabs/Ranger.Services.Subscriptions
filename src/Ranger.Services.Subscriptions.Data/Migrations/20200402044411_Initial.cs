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
                    pgsql_database_username = table.Column<string>(nullable: false),
                    subscription_id = table.Column<string>(nullable: false),
                    plan_id = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_subscriptions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "utilization_details",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_subscription_id = table.Column<int>(nullable: false),
                    geofence_count = table.Column<int>(nullable: false),
                    project_count = table.Column<int>(nullable: false),
                    integration_count = table.Column<int>(nullable: false),
                    account_count = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_utilization_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_utilization_details_tenant_subscriptions_tenant_subscriptio~",
                        column: x => x.tenant_subscription_id,
                        principalTable: "tenant_subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_subscriptions_pgsql_database_username",
                table: "tenant_subscriptions",
                column: "pgsql_database_username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_subscriptions_subscription_id",
                table: "tenant_subscriptions",
                column: "subscription_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_utilization_details_tenant_subscription_id",
                table: "utilization_details",
                column: "tenant_subscription_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_protection_keys");

            migrationBuilder.DropTable(
                name: "utilization_details");

            migrationBuilder.DropTable(
                name: "tenant_subscriptions");
        }
    }
}
