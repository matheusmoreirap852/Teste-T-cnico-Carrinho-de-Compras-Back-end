# Testes unitários do back-end

## Resultado da execução

89 testes aprovados, 0 falhas, 0 ignorados.

| Camada | Linhas cobertas | Ramificações cobertas |
|---|---:|---:|
| Carrinho.Core | 97,76% | 100% |
| Carrinho.Application | 98,43% | 100% |

Métricas obtidas com Coverlet. API e Infrastructure não estão incluídas nesses percentuais. Os construtores privados de materialização do EF Core não são exercitados pelos testes unitários. Cobertura indica código executado, não prova ausência de defeitos.

## Como executar

Na raiz do repositório:

```powershell
dotnet test Carrinho.sln
dotnet test Carrinho.sln --collect:"XPlat Code Coverage" --logger "trx;LogFileName=unitarios.trx" --results-directory TestResults
```

Para executar apenas um grupo:

```powershell
dotnet test Carrinho.sln --filter "FullyQualifiedName~CarrinhosServiceTests"
```

No Visual Studio: abra `Carrinho.sln`, use **Teste > Gerenciador de Testes** e **Executar Todos**. Os resultados TRX e o XML de cobertura são gerados em `TestResults/`, ignorado pelo Git.

## Cenários cobertos

- Produtos: dados obrigatórios, IDs inválidos, preço negativo, precisão e limite monetário, estoque negativo/zero, cadastro duplicado, atualização, consulta, listagem e exclusão.
- Cupons: código obrigatório, normalização, limite de comprimento, percentuais inválidos, limites 0,01% e 100%, duplicidade de ID e CRUD.
- Carrinho: primeira inclusão com uma unidade, soma posterior, substituição de quantidade, remoção, múltiplos produtos e recurso/item inexistente.
- Estoque: quantidade zero/negativa, excesso, soma superior ao disponível, estouro de inteiro, última unidade e revalidação no checkout.
- Cálculos: subtotal, troca/remoção de cupom, centavos e arredondamento, desconto integral, carrinho vazio e recálculo após remoção.
- Histórico: alterações do catálogo não modificam preços e desconto já aplicados.
- Checkout: bloqueio de carrinho vazio, baixa de estoque, prevenção de baixa parcial por validação e rejeição de todas as mutações após finalização.
- Aplicação: conversão para DTOs, persistência apenas nas operações válidas, falha de persistência propagada sem retorno de sucesso, paginação e cancelamento.

## Dados simulados

`Support/LojaRepositoryFake.cs` é um dublê manual de `ILojaRepository` e `IProdutoRepository`. Cada teste recebe uma instância independente. Os dicionários fornecem produtos, cupons e carrinhos fictícios; as listas registram inclusões/exclusões e o contador registra tentativas de salvar. `FalhaAoSalvar` permite simular um conflito de persistência.

O fake não é um banco de dados em memória e não implementa constraints, commits, rollback ou concorrência. Ele não grava dados reais, não altera seu catálogo e não requer bibliotecas adicionais de mocking.

## Limites e integração

Esses testes validam regras e orquestração dos casos de uso. Rotas HTTP, model binding, migrations, restrições do PostgreSQL, transações e disputa concorrente precisam da suíte de integração em `tests/integration.py`. A suíte existente possui 59 verificações HTTP e usa `compose.integration.yaml`, em um ambiente separado. Ela não foi reexecutada nesta ampliação de testes unitários, que não altera código de produção.
