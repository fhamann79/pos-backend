using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pos.Backend.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEmissionPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmissionPointId",
                table: "Users",
                type: "integer",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "EmissionPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstablishmentId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmissionPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmissionPoints_Establishments_EstablishmentId",
                        column: x => x.EstablishmentId,
                        principalTable: "Establishments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmissionPoints_EstablishmentId",
                table: "EmissionPoints",
                column: "EstablishmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmissionPointId",
                table: "Users",
                column: "EmissionPointId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_EmissionPoints_EmissionPointId",
                table: "Users",
                column: "EmissionPointId",
                principalTable: "EmissionPoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_EmissionPoints_EmissionPointId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "EmissionPoints");

            migrationBuilder.DropIndex(
                name: "IX_Users_EmissionPointId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmissionPointId",
                table: "Users");
        }
    }
}
