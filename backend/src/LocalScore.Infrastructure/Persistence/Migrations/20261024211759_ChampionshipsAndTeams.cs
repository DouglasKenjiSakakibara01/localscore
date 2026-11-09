using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LocalScore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChampionshipsAndTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "championship_statuses",
                columns: table => new
                {
                    code = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_championship_statuses", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    short_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    acronym = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    primary_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    secondary_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teams", x => x.id);
                    table.ForeignKey(
                        name: "fk_teams_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "championships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status_code = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_championships", x => x.id);
                    table.ForeignKey(
                        name: "fk_championships_championship_statuses_status_code",
                        column: x => x.status_code,
                        principalTable: "championship_statuses",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_championships_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "championship_teams",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    championship_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    removed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_championship_teams", x => x.id);
                    table.ForeignKey(
                        name: "fk_championship_teams_championships_championship_id",
                        column: x => x.championship_id,
                        principalTable: "championships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_championship_teams_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "championship_statuses",
                columns: new[] { "code", "description" },
                values: new object[,]
                {
                    { 1, "Rascunho" },
                    { 2, "Publicado" },
                    { 3, "Finalizado" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_championship_teams_championship_id",
                table: "championship_teams",
                column: "championship_id");

            migrationBuilder.CreateIndex(
                name: "ix_championship_teams_team_id",
                table: "championship_teams",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "ux_championship_teams_championship_id_team_id",
                table: "championship_teams",
                columns: new[] { "championship_id", "team_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_championships_owner_user_id",
                table: "championships",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_championships_status_code",
                table: "championships",
                column: "status_code");

            migrationBuilder.CreateIndex(
                name: "ux_championships_slug",
                table: "championships",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teams_owner_user_id",
                table: "teams",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "ux_teams_owner_user_id_normalized_name",
                table: "teams",
                columns: new[] { "owner_user_id", "normalized_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "championship_teams");

            migrationBuilder.DropTable(
                name: "championships");

            migrationBuilder.DropTable(
                name: "teams");

            migrationBuilder.DropTable(
                name: "championship_statuses");
        }
    }
}
