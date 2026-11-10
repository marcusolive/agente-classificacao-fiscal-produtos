CREATE TABLE IF NOT EXISTS execucao (
    id uuid PRIMARY KEY,
    resultado jsonb NOT NULL
);

CREATE TABLE IF NOT EXISTS item_analise (
    execucao_id uuid NOT NULL REFERENCES execucao (id),
    item_id text NOT NULL,
    situacao text NOT NULL,
    ncm_sugerida text NULL,
    PRIMARY KEY (execucao_id, item_id)
);

CREATE TABLE IF NOT EXISTS evidencia_citada (
    execucao_id uuid NOT NULL,
    item_id text NOT NULL,
    fonte text NOT NULL,
    faixa text NOT NULL,
    trecho text NULL,
    vigencia date NOT NULL
);

CREATE TABLE IF NOT EXISTS achado (
    execucao_id uuid NOT NULL,
    item_id text NULL,
    classe text NOT NULL,
    severidade text NOT NULL,
    descricao text NOT NULL
);
