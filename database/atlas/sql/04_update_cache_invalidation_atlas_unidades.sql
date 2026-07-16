create or replace function public.invalidar_cache_atlas_unidades()
returns trigger
language plpgsql
as $$
declare
    id_economia_atual integer;
    id_economia_antigo integer;
begin
    if (TG_OP = 'DELETE') then
        id_economia_atual := OLD.ideconomia;
        id_economia_antigo := OLD.ideconomia;
    elsif (TG_OP = 'UPDATE') then
        id_economia_atual := NEW.ideconomia;
        id_economia_antigo := OLD.ideconomia;
    else
        id_economia_atual := NEW.ideconomia;
        id_economia_antigo := NEW.ideconomia;
    end if;

    update public.cache
    set
        invalidado_em = now(),
        motivo_invalidacao = 'Registro da tabela atlas_unidades alterado'
    where
        entidade = 'atlas_unidades'
        and invalidado_em is null
        and expirado_em > now()
        and tipo_consulta = 'UNIDADE_IDECONOMIA'
        and entidade_id = id_economia_atual;

    if (TG_OP = 'UPDATE' and id_economia_antigo <> id_economia_atual) then
        update public.cache
        set
            invalidado_em = now(),
            motivo_invalidacao = 'Registro da tabela atlas_unidades alterado'
        where
            entidade = 'atlas_unidades'
            and invalidado_em is null
            and expirado_em > now()
            and tipo_consulta = 'UNIDADE_IDECONOMIA'
            and entidade_id = id_economia_antigo;
    end if;

    update public.cache
    set
        invalidado_em = now(),
        motivo_invalidacao = 'Registro da tabela atlas_unidades alterado'
    where
        entidade = 'atlas_unidades'
        and invalidado_em is null
        and expirado_em > now()
        and tipo_consulta in (
            'UNIDADE_CODCONDOM',
            'UNIDADE_NOME_CONDOMINO'
        );

    if (TG_OP = 'DELETE') then
        return OLD;
    end if;

    return NEW;
end;
$$;

drop trigger if exists trg_invalidar_cache_atlas_unidades
on public.atlas_unidades;

create trigger trg_invalidar_cache_atlas_unidades
after insert or update or delete
on public.atlas_unidades
for each row
execute function public.invalidar_cache_atlas_unidades();