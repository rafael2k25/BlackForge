INSERT INTO dbo.ProcessosProducao
(
    OrdemServicoId,
    MaquinaId,
    DataInicio,
    DataFim,
    QuantidadePlanejada,
    QuantidadeProduzida,
    ProducaoPorMinuto,
    ConsumoPorUnidade,
    MaterialConsumido,
    Status,
    Observacoes
)
VALUES
(
    6,
    1,
    DATEADD(MINUTE, -40, GETDATE()),
    NULL,
    500,
    320,
    8.00,
    0.8000,
    256.0000,
    'EM_EXECUCAO',
    'Produção fictícia para teste do monitor'
),
(
    7,
    2,
    DATEADD(MINUTE, -25, GETDATE()),
    NULL,
    300,
    120,
    6.00,
    1.2000,
    144.0000,
    'EM_EXECUCAO',
    'Produção fictícia para teste do monitor'
);