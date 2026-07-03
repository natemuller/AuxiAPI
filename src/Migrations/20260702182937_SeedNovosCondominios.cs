using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuxiAPI.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedNovosCondominios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "condominios",
                columns: new[] { "id", "bairroDoEndereco", "cepDoEndereco", "cnpjDoCondominio", "cidadeDoEndereco", "codigoDoCondominio", "dataFinal_Administracao", "dataInicial_Administracao", "endereco", "estadoDoEndereco", "nomeDoCondominio", "nomeGerenteDeContas", "nomeSindico", "numeroDeTorres", "numeroDeUnidades", "numeroDoEndereco", "status" },
                values: new object[,]
                {
                    { 6, "Jardins", "01234567", "22111333000166", "São Paulo", "0006", "", "01/03/2022", "Avenida do Sol", "SP", "Condomínio Plaza Colucci", "Mia Colucci", "Miguel Arango", 5, 180, "144", "Ativo" },
                    { 7, "Barra da Tijuca", "22345678", "33222444000177", "Rio de Janeiro", "0007", "", "10/06/2023", "Rua da Harmonia", "RJ", "Residencial RBD", "Roberta Pardo", "Diego Bustamante", 4, 120, "200", "Ativo" },
                    { 8, "Savassi", "30456789", "44333555000188", "Belo Horizonte", "0008", "", "15/09/2021", "Travessa das Estrelas", "MG", "Parque Celina", "Celina Ferrer", "Lupita Fernández", 3, 90, "77", "Ativo" },
                    { 9, "Meireles", "60123456", "55444666000199", "Fortaleza", "0009", "", "20/02/2024", "Boulevard das Artes", "CE", "Vista Altos", "Josy Luján", "Giovanni Méndez", 6, 240, "310", "Ativo" },
                    { 10, "Cambuí", "13234567", "66555777000111", "Campinas", "0010", "", "05/11/2022", "Rua dos Sonhos", "SP", "Condomínio Esperança", "Victoria Díaz", "Lupita Fernández", 2, 64, "510", "Ativo" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "condominios",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "condominios",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "condominios",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "condominios",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "condominios",
                keyColumn: "id",
                keyValue: 10);
        }
    }
}
