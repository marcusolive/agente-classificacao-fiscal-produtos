CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE IF NOT EXISTS documento (
    documento_id uuid PRIMARY KEY,
    origem text NOT NULL,
    identificacao text NOT NULL,
    versao text NOT NULL,
    faixa text NOT NULL,
    vigencia_inicial date NOT NULL,
    vigencia_final date NULL,
    formato text NOT NULL,
    hash text NOT NULL,
    estado text NOT NULL,
    fonte text NOT NULL,
    consultavel boolean NOT NULL,
    conteudo jsonb NOT NULL,
    CONSTRAINT documento_identidade UNIQUE (origem, identificacao, versao)
);

CREATE TABLE IF NOT EXISTS chunk (
    chunk_id uuid PRIMARY KEY,
    documento_id uuid NOT NULL REFERENCES documento (documento_id),
    posicao integer NOT NULL,
    texto text NOT NULL,
    localizacao text NULL,
    hash_documento text NOT NULL
);
