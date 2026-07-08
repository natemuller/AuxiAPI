using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuxiAPI.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class CreateCacheTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cache",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave_cache = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    url_da_consulta = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    metodo_http = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    tipo_consulta = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entidade_id = table.Column<int>(type: "integer", nullable: true),
                    resposta = table.Column<string>(type: "jsonb", nullable: false),
                    status_code = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expirado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    invalidado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo_invalidacao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cache", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cache_chave_cache",
                table: "cache",
                column: "chave_cache");

            migrationBuilder.CreateIndex(
                name: "IX_cache_chave_cache_expirado_em_invalidado_em",
                table: "cache",
                columns: new[] { "chave_cache", "expirado_em", "invalidado_em" });

            migrationBuilder.CreateIndex(
                name: "IX_cache_entidade_entidade_id",
                table: "cache",
                columns: new[] { "entidade", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "IX_cache_tipo_consulta",
                table: "cache",
                column: "tipo_consulta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cache");
        }
    }
}
