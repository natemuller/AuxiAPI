-- =====================================================
-- Validação das tabelas Atlas importadas no Supabase
-- =====================================================

-- 1. Total de registros importados
select 'atlas_condominios' as tabela, count(*) as total
from public.atlas_condominios

union all

select 'atlas_blocos' as tabela, count(*) as total
from public.atlas_blocos

union all

select 'atlas_unidades' as tabela, count(*) as total
from public.atlas_unidades;


-- 2. Unidades sem condomínio correspondente
select 
    u.codcondom, 
    count(*) as total
from public.atlas_unidades u
left join public.atlas_condominios c
    on c.codcondom = u.codcondom
where c.codcondom is null
group by u.codcondom;


-- 3. Unidades sem bloco correspondente
select 
    u.codcondom, 
    u.codbloco, 
    count(*) as total
from public.atlas_unidades u
left join public.atlas_blocos b
    on b.codcondom = u.codcondom
   and b.codbloco = u.codbloco
where b.codbloco is null
group by u.codcondom, u.codbloco
order by u.codcondom, u.codbloco;


-- 4. Comparativo entre totais da tabela de condomínios e registros importados
select
    c.codcondom,
    c.nomecondom,
    c.ativo,
    c.cnpj,
    c.qtdblocos,
    c.qtd_unidades,
    count(distinct b.codbloco) as blocos_importados,
    count(distinct u.ideconomia) as unidades_importadas
from public.atlas_condominios c
left join public.atlas_blocos b
    on b.codcondom = c.codcondom
left join public.atlas_unidades u
    on u.codcondom = c.codcondom
group by
    c.codcondom,
    c.nomecondom,
    c.ativo,
    c.cnpj,
    c.qtdblocos,
    c.qtd_unidades
order by c.codcondom;