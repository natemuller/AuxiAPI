create table if not exists public.atlas_condominios (
    codcondom integer primary key,
    nomecondom text,
    ativo text,
    cnpj text,
    cei text,
    inscrmunicip text,
    qtdblocos integer,
    qtd_unidades integer,
    totalfracao numeric(18, 8),
    diavencdoc integer,
    datainicioadm integer,
    datadistrato integer,
    motivodistrato text,
    assessor text,
    filial text,
    agencia text,
    sindico text,
    subsindico text,
    conselheiro text,
    gestor text,
    conselho_fiscal text,
    conselho_consultivo text,
    conselho_suplente text,
    tipocondominio text,
    tipocategoria text,
    dtalteracao timestamp,
    tipolograd text,
    lograd text,
    numero text,
    bairro text,
    cidade text,
    cep8_log text,
    uf text,
    codpessoa_sindico text,
    nome_sindico text,
    cpfdocnpj text,
    condgarantido text,
    tipoconta text,
    obscobranca text,
    garantidora text
);

create table if not exists public.atlas_blocos (
    codcondom integer not null,
    codbloco text not null,
    codblocobase text,
    descricao text,
    qtdeconomias integer,
    tipolograd text,
    lograd text,
    numero text,
    bairro text,
    cidade text,
    uf text,
    cep8_log text,
    ativo text,
    tipo_bloco text,

    constraint pk_atlas_blocos
        primary key (codcondom, codbloco),

    constraint fk_atlas_blocos_condominio
        foreign key (codcondom)
        references public.atlas_condominios (codcondom)
        on delete restrict
);

create table if not exists public.atlas_unidades (
    ideconomia integer primary key,
    codcondom integer not null,
    codbloco text not null,
    codeconom text,
    fracao numeric(18, 8),
    ativa text,
    datadesativa integer,
    dtalteracao timestamp,
    tipo_unidade text,
    cod_condomino text,
    nome_condomino text,
    endereco_principal text,
    endereco_correspondencia text,
    endereco_cobranca text,
    codpesdebconta text,
    nome_debconta text,
    codfornec text,
    codnaadmdest text,
    codfornecescrit text,

    constraint fk_atlas_unidades_condominio
        foreign key (codcondom)
        references public.atlas_condominios (codcondom)
        on delete restrict,

    constraint fk_atlas_unidades_bloco
        foreign key (codcondom, codbloco)
        references public.atlas_blocos (codcondom, codbloco)
        on delete restrict
);

create index if not exists ix_atlas_condominios_cnpj
    on public.atlas_condominios (cnpj);

create index if not exists ix_atlas_condominios_nomecondom
    on public.atlas_condominios (nomecondom);

create index if not exists ix_atlas_condominios_ativo
    on public.atlas_condominios (ativo);

create index if not exists ix_atlas_blocos_codcondom
    on public.atlas_blocos (codcondom);

create index if not exists ix_atlas_blocos_tipo_bloco
    on public.atlas_blocos (tipo_bloco);

create index if not exists ix_atlas_blocos_ativo
    on public.atlas_blocos (ativo);

create index if not exists ix_atlas_unidades_codcondom
    on public.atlas_unidades (codcondom);

create index if not exists ix_atlas_unidades_codbloco
    on public.atlas_unidades (codbloco);

create index if not exists ix_atlas_unidades_ativa
    on public.atlas_unidades (ativa);