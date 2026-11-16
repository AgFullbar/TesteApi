using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiTeste.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoTabelaCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Compras_DataCadastro",
                table: "Compras",
                column: "DataCadastro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Compras_DataCadastro",
                table: "Compras");
        }
    }
}