# ADR 0002 — Confiança pela faixa de autoridade

- Status: aceita
- Data: 2026-10-29
- Specification: `specs/001-execucao-analise-fiscal/spec.md`

## Decisão

A confiança de 0 a 100 é calculada pela faixa da evidência vencedora, não por uma nota livre do modelo.

- Legislação vigente: 90
- Notas e Regras Gerais de Interpretação: 80
- Caso corporativo confirmado: 65
- Texto do fornecedor: 40

Empate na mesma faixa, evidência sem trecho ou falha de cálculo deixam a confiança desconhecida. Valor desconhecido não é gravado como zero. Menor que 70 pede revisão humana e, havendo candidato sustentado, a sugestão continua na resposta.

## Contexto

O corte de 70 já está decidido. O método precisava ser o mesmo em toda execução.

## Consequências

Caso corporativo ou texto de fornecedor, quando são a melhor fonte, voltam com sugestão e revisão. Legislação ou notas, sem empate, ficam em 80 ou 90 e não pedem revisão só pela confiança.
