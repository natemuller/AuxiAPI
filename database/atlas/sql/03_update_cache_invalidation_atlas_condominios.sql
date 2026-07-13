-- =====================================================
-- Atualiza invalidação de cache para origem Atlas
-- Tabela principal: public.atlas_condominios
-- =====================================================

drop trigger if exists trg_invalidar_cache_condominios
on public.condominios;

drop function if exists public.invalidar_cache_condominios();

create or replace function public.invalidar_cache_atlas_condominios()
returns trigger
language plpgsql
as $$
declare
    cod_condom_atual integer;
    cod_condom_antigo integer;
begin
    if (TG_OP = 'DELETE') then
        cod_condom_atual := OLD.codcondom;
        cod_condom_antigo := OLD.codcondom;
    elsif (TG_OP = 'UPDATE') then
        cod_condom_atual := NEW.codcondom;
        cod_condom_antigo := OLD.codcondom;
    else
        cod_condom_atual := NEW.codcondom;
        cod_condom_antigo := NEW.codcondom;
    end if;

    -- Invalida cache específico por codcondom
    update public.cache
    set
        invalidado_em = now(),
        motivo_invalidacao = 'Registro da tabela atlas_condominios alterado'
    where
        entidade = 'atlas_condominios'
        and invalidado_em is null
        and expirado_em > now()
        and tipo_consulta = 'CONDOMINIO_CODCONDOM'
        and entidade_id = cod_condom_atual;

    -- Se o codcondom foi alterado, invalida também o código antigo
    if (TG_OP = 'UPDATE' and cod_condom_antigo <> cod_condom_atual) then
        update public.cache
        set
            invalidado_em = now(),
            motivo_invalidacao = 'Registro da tabela atlas_condominios alterado'
        where
            entidade = 'atlas_condominios'
            and invalidado_em is null
            and expirado_em > now()
            and tipo_consulta = 'CONDOMINIO_CODCONDOM'
            and entidade_id = cod_condom_antigo;
    end if;

    -- Invalida buscas por nome e CNPJ
    -- Como esses caches guardam resultado de listagem, invalidamos todos dessa categoria.
    update public.cache
    set
        invalidado_em = now(),
        motivo_invalidacao = 'Registro da tabela atlas_condominios alterado'
    where
        entidade = 'atlas_condominios'
        and invalidado_em is null
        and expirado_em > now()
        and tipo_consulta in (
            'CONDOMINIO_NOME',
            'CONDOMINIO_CNPJ'
        );

    if (TG_OP = 'DELETE') then
        return OLD;
    end if;

    return NEW;
end;
$$;

drop trigger if exists trg_invalidar_cache_atlas_condominios
on public.atlas_condominios;

create trigger trg_invalidar_cache_atlas_condominios
after insert or update or delete
on public.atlas_condominios
for each row
execute function public.invalidar_cache_atlas_condominios();