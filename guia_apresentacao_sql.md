# Guia de Apresentação de SQL do Projeto Hospitalar

Este documento serve como um mapa para mostrar ao seu professor onde e como todos os conceitos de Banco de Dados e SQL foram aplicados no seu projeto.

## 1. Criação do Banco e Tabelas (DDL)
**O que mostrar:** Onde as tabelas, Chaves Primárias (PK) e Chaves Estrangeiras (FK) são criadas.
**Onde mostrar no código:** Arquivo raiz `init_tables.sql`

O projeto possui um script puro em SQL que roda assim que o Docker sobe o banco de dados. Nele estão definidos todos os comandos de criação de tabelas:
*   **CREATE TABLE:** Usado para criar `Ambulatorios`, `Medicos`, `Pacientes`, etc.
*   **PRIMARY KEY:** Usado para garantir que não haja dois registros com o mesmo identificador (ex: `nroa` em Ambulatórios).
*   **FOREIGN KEY (Relacionamentos):** Mostre a tabela `Consultas`, que possui duas chaves estrangeiras (`codm` e `codp`), ou a tabela `Medicos` que se relaciona com `Ambulatorios` através do `nroa`.

## 2. Inserção Inicial de Dados (DML - INSERT)
**O que mostrar:** Como os dados base foram injetados no sistema antes mesmo da tela abrir (População inicial).
**Onde mostrar no código:** Final do arquivo `init_tables.sql`

*   **INSERT INTO ... VALUES:** Lá no seu arquivo `init_tables.sql`, existem múltiplos comandos inserindo os médicos "João", "Maria", os pacientes, etc., para que o banco não comece totalmente vazio.

## 3. Consultas e Relatórios (DQL - SELECT)
**O que mostrar:** Onde as "Quereys" de SQL bruto explícito estão rodando em tempo real.
**Onde mostrar no código:** `api/Program.cs` na seção Endpoints

*   **Listagem Completa (Endpoints GET):** O arquivo `Program.cs` utiliza o comando `.FromSqlRaw("SELECT * FROM ...")` explícito dentro do C# para ir buscar os dados. Você pode mostrar para ele exatamente as strings de SQL como essa:
    ```csharp
    db.Medicos.FromSqlRaw("SELECT * FROM Medicos").ToListAsync();
    ```
*   **Busca por ID:** Quando se clica em Buscar, ele executa um SQL injetando de maneira segura o parâmetro:
    ```csharp
    FromSqlRaw("SELECT * FROM Medicos WHERE codm = {0}", codm)
    ```

## 4. O Relatório SQL "Crú" que a Interface Gera
**O que mostrar:** A tela onde o professor pode baixar os dados do banco como um backup gigante em SQL.
**Onde mostrar no código:** `api/Program.cs` na rota `app.MapGet("/api/relatorio/sql", ...)`

*   No seu Frontend (React), existe o botão amarelo/verde de **"💾 Gerar Relatório SQL"**. 
*   No Backend, esse endpoint faz um laço de repetição (`foreach`) lendo os dados da memória e montando `strings` com a sintaxe de `INSERT INTO` novamente, linha a linha, reconstruindo todo o estado atual do banco.

## 5. Controle de Transações Nativas (Transaction, Commit e Rollback)
**O que mostrar:** Como o sistema lida com falhas para garantir que o banco de dados não fique pela metade (Garantia ACID e Consistência).
**Onde mostrar no código:** Qualquer Endpoint de `POST`, `PUT` ou `DELETE` no `api/Program.cs`.

**Passo a passo da Transação no Código (ex: Endpoint POST de Ambulatorios):**
1.  **START TRANSACTION:** A linha `using var transaction = await db.Database.BeginTransactionAsync();` manda o MySQL abrir uma nova janela de transação segura.
2.  **TRY & EXECUÇÃO DO SQL NO BANCO:** O sistema atira nativamente o SQL no banco usando a função:  
    `await db.Database.ExecuteSqlRawAsync("INSERT INTO Ambulatorios (nroa, andar, capacidade) VALUES ({0}, {1}, {2})", a.nroa, a.andar, a.capacidade);`
3.  **SE DER TUDO CERTO (COMMIT):** O código prossegue e atinge a linha `await transaction.CommitAsync();`, efetivando os dados inseridos definitivamente no disco.
4.  **CATCH - SE DER ERRO (ROLLBACK):** Se qualquer regra falhar ou você marcar a caixinha "Forçar Erro p/ Ver Rollback" no frontend, o sistema cai no bloco `catch` e roda `await transaction.RollbackAsync();`. Isso desfaz a inserção SQL bruta rodada no Passo 2 e joga o erro na tela.


