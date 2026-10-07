using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlackForge.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarStatusOS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "OrdensServico",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "pendente");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataConclusao",
                table: "OrdensServico",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataConclusao",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OrdensServico");
        }
    }
}
