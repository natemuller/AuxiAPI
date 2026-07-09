using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuxiAPI.WebApi.Migrations
{
    public partial class AddCacheInvalidationTrigger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE EXTENSION IF NOT EXISTS unaccent;

                CREATE OR REPLACE FUNCTION public.invalidar_cache_condominios()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    condominio_id integer;
                BEGIN
                    IF (TG_OP = 'DELETE') THEN
                        condominio_id := OLD.id;
                    ELSE
                        condominio_id := NEW.id;
                    END IF;

                    UPDATE public.cache
                    SET
                        invalidado_em = now(),
                        motivo_invalidacao = 'Registro da tabela condominios alterado'
                    WHERE
                        entidade = 'condominios'
                        AND invalidado_em IS NULL
                        AND expirado_em > now()
                        AND (
                            tipo_consulta = 'CONDOMINIO_NOME'
                            OR (
                                tipo_consulta = 'CONDOMINIO_ID'
                                AND entidade_id = condominio_id
                            )
                        );

                    IF (TG_OP = 'UPDATE' AND OLD.id <> NEW.id) THEN
                        UPDATE public.cache
                        SET
                            invalidado_em = now(),
                            motivo_invalidacao = 'Registro da tabela condominios alterado'
                        WHERE
                            entidade = 'condominios'
                            AND invalidado_em IS NULL
                            AND expirado_em > now()
                            AND tipo_consulta = 'CONDOMINIO_ID'
                            AND entidade_id = OLD.id;
                    END IF;

                    IF (TG_OP = 'DELETE') THEN
                        RETURN OLD;
                    END IF;

                    RETURN NEW;
                END;
                $$;

                DROP TRIGGER IF EXISTS trg_invalidar_cache_condominios ON public.condominios;

                CREATE TRIGGER trg_invalidar_cache_condominios
                AFTER INSERT OR UPDATE OR DELETE
                ON public.condominios
                FOR EACH ROW
                EXECUTE FUNCTION public.invalidar_cache_condominios();
            """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS trg_invalidar_cache_condominios ON public.condominios;

                DROP FUNCTION IF EXISTS public.invalidar_cache_condominios();
            """);
        }
    }
}