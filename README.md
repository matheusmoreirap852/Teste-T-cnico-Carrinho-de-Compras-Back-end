# Carrinho de Compras — API

Estrutura inicial em C# / .NET 10, ASP.NET Core e PostgreSQL via Entity Framework Core.
Esta etapa cria a base arquitetural; ainda não é a implementação completa do desafio.

## Camadas

- `Carrinho.API`: controllers, configuração HTTP, OpenAPI e erros em Problem Details.
- `Carrinho.Application`: casos de uso, contratos de repositórios e DTOs.
- `Carrinho.Core`: entidades e regras de domínio, sem dependências de infraestrutura.
- `Carrinho.Infrastructure`: EF Core, mapeamentos, migrations e repositórios PostgreSQL.
- `Carrinho.UnitTests`: testes das regras do domínio.

Dependências: Application → Core; Infrastructure → Application/Core; API → Application/Infrastructure.
A API referencia Infrastructure para compor a injeção de dependência. Controllers usam casos de uso.

## Abrir no Visual Studio

Abra `Carrinho.sln`, com suporte ao SDK .NET 10 instalado, e selecione `Carrinho.API` como projeto de inicialização.
No VS Code, abra a pasta raiz deste repositório.

## Executar com Docker

Requer Docker com suporte a containers Linux e Docker Compose v2.

```powershell
Copy-Item .env.example .env
# Edite a senha em .env antes de continuar.
docker compose up --build -d
```

O serviço `migrate` aplica as migrations e termina antes da API iniciar.
A API fica em `http://localhost:8080`; o PostgreSQL possui volume persistente.
Portas publicadas ficam restritas ao localhost. `docker compose down` mantém os dados;
não use `down -v` se precisar preservá-los.

## Executar pelo Visual Studio ou terminal

```powershell
docker compose up -d db
dotnet user-secrets set "ConnectionStrings:Carrinho" "Host=localhost;Port=5432;Database=carrinho;Username=carrinho;Password=SUA_SENHA" --project src/Carrinho.API
dotnet restore
dotnet run --project src/Carrinho.API -- --migrate
dotnet run --project src/Carrinho.API --launch-profile http
```

Use a mesma senha, usuário e banco configurados no `.env`. Senhas não são versionadas.
No perfil `http`, a API fica em `http://localhost:5269`.

## Endpoints desta etapa

| Método | Rota | Objetivo |
|---|---|---|
| GET | `/api/produtos` | Lista produtos persistidos, com preço e estoque |
| GET | `/health/live` | Verifica se o processo responde |
| GET | `/health/ready` | Verifica acesso à tabela Produto; retorna 503 se indisponível |
| GET | `/openapi/v1.json` | Documento OpenAPI, somente em Development |
| GET | `/swagger` | Interface interativa Swagger UI, somente em Development |

Exemplos no arquivo `src/Carrinho.API/Carrinho.API.http`.
Swagger UI está disponível em `http://localhost:8080/swagger` pelo Docker ou `http://localhost:5269/swagger` pelo perfil HTTP. Use **Try it out** e **Execute** para consultar os endpoints. A interface utiliza o documento `/openapi/v1.json`.
O catálogo retorna uma lista vazia até a importação dos dados oficiais.

## Testes e migrations

```powershell
dotnet test Carrinho.sln
dotnet tool restore
dotnet ef migrations add NomeDaMigration --project src/Carrinho.Infrastructure --startup-project src/Carrinho.API --output-dir Persistence/Migrations
```

Para criar migrations, configure a connection string por user-secrets conforme acima.
Valores monetários usam `decimal` e a coluna de preço usa `numeric(18,2)`.

## Próximas etapas

- Importar `produtos.json` e `cupons.json` originais, ainda não fornecidos; nenhum catálogo fictício foi criado.
- Implementar agregado Carrinho, itens, cupons, estoque, cálculos e checkout, com migrations e testes.
- Implementar autenticação. Os endpoints atuais ainda não exigem credenciais.
- Integrar o front-end e configurar a política de origem se necessária.
- Preparar a implantação EC2 com Nginx, HTTPS, ambiente Production, segredos e backups.

O Compose atual é para desenvolvimento e verificação local; não representa uma implantação EC2 concluída.
O front-end será mantido no repositório separado informado pelo usuário.

## Validação desta etapa

- Compilação da solução e três testes unitários aprovados.
- API iniciada localmente: saúde e OpenAPI responderam 200.
- Sem banco disponível: readiness respondeu 503 e catálogo respondeu 500 em Problem Details, sem expor detalhes internos.
- Sintaxe do Compose validada e migration inicial gerada.
- Containers compilados e iniciados com Docker Compose; PostgreSQL saudável e migration inicial aplicada com sucesso (serviço migrate encerrado com código 0).
- Com banco real, `/health/live`, `/health/ready`, `/api/produtos` e `/openapi/v1.json` responderam 200 em `http://localhost:8080`.
- O catálogo retornou `[]`, conforme esperado antes da importação dos dados oficiais.
