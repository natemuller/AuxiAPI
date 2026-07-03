using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuxiAPI.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoTabelaCondominios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "condominios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigoDoCondominio = table.Column<string>(type: "text", nullable: false),
                    cnpjDoCondominio = table.Column<string>(type: "text", nullable: false),
                    nomeDoCondominio = table.Column<string>(type: "text", nullable: false),
                    endereco = table.Column<string>(type: "text", nullable: false),
                    numeroDoEndereco = table.Column<string>(type: "text", nullable: false),
                    estadoDoEndereco = table.Column<string>(type: "text", nullable: false),
                    cidadeDoEndereco = table.Column<string>(type: "text", nullable: false),
                    bairroDoEndereco = table.Column<string>(type: "text", nullable: false),
                    cepDoEndereco = table.Column<string>(type: "text", nullable: false),
                    numeroDeTorres = table.Column<int>(type: "integer", nullable: false),
                    numeroDeUnidades = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    dataInicial_Administracao = table.Column<string>(type: "text", nullable: false),
                    dataFinal_Administracao = table.Column<string>(type: "text", nullable: false),
                    nomeGerenteDeContas = table.Column<string>(type: "text", nullable: false),
                    nomeSindico = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_condominios", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "condominios",
                columns: new[] { "id", "bairroDoEndereco", "cepDoEndereco", "cnpjDoCondominio", "cidadeDoEndereco", "codigoDoCondominio", "dataFinal_Administracao", "dataInicial_Administracao", "endereco", "estadoDoEndereco", "nomeDoCondominio", "nomeGerenteDeContas", "nomeSindico", "numeroDeTorres", "numeroDeUnidades", "numeroDoEndereco", "status" },
                values: new object[,]
                {
                    { 1, "Auxiliadora", "98567213", "12345678000101", "Porto Alegre", "0001", "", "20/08/2021", "Rua do Cambraia", "RS", "Residencial Brasil-Hexa", "Antonio Banderas", "Alfonso Herrera", 3, 90, "5010", "Ativo" },
                    { 2, "Tristeza", "91183730", "12543867000101", "Porto Alegre", "0002", "01/06/2026", "10/04/2019", "Rua do Marcel", "RS", "Residencial Vivendas do Pelé", "Christopher Uckermann", "Lucio Flavio", 2, 60, "103", "Inativo" },
                    { 3, "Moinhos de Vento", "90450200", "98765432000110", "Porto Alegre", "0003", "", "15/01/2023", "Avenida das Orquídeas", "RS", "Residencial Jardim das Flores", "Mariana Santos", "Pedro Almeida", 4, 120, "1200", "Ativo" },
                    { 4, "Centro", "92700120", "11223344000155", "Canoas", "0004", "", "05/09/2020", "Rua das Acácias", "RS", "Condomínio Bela Vista", "Yur Teixeira", "Clara Mendes", 2, 48, "250", "Ativo" },
                    { 5, "Santo Antônio", "93010300", "55667788000199", "São Leopoldo", "0005", "20/02/2024", "22/11/2018", "Rua dos Eucaliptos", "RS", "Parque dos Pinheiros", "Juliana Pereira", "Sharon Garcia", 3, 72, "89", "Inativo" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "condominios");
        }
    }
}
