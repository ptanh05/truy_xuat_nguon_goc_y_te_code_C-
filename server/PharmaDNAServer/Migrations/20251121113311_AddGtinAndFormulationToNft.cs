using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PharmaDNAServer.Migrations
{
    /// <inheritdoc />
    public partial class AddGtinAndFormulationToNft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Formulation",
                table: "SanPhamNFT",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gtin",
                table: "SanPhamNFT",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SensorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NftId = table.Column<int>(type: "integer", nullable: false),
                    DistributorAddress = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<byte[]>(type: "bytea", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedStatus = table.Column<string>(type: "text", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessingError = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensorLogs_SanPhamNFT_NftId",
                        column: x => x.NftId,
                        principalTable: "SanPhamNFT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SensorLogs_NftId",
                table: "SensorLogs",
                column: "NftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorLogs");

            migrationBuilder.DropColumn(
                name: "Formulation",
                table: "SanPhamNFT");

            migrationBuilder.DropColumn(
                name: "Gtin",
                table: "SanPhamNFT");
        }
    }
}
