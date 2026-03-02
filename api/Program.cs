using Microsoft.EntityFrameworkCore;
using SimpleCrud.Api.Data;
using SimpleCrud.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB Context (MySQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? "server=mysql;user=root;password=root;database=simplecrud";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 31))));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensure DB is created with Retry Logic
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    var retryCount = 0;
    while (retryCount < 10)
    {
        try
        {
            logger.LogInformation("Attempting to connect to database... ({Attempt})", retryCount + 1);
            db.Database.EnsureCreated();
            logger.LogInformation("Database connected and created.");
            break;
        }
        catch (Exception ex)
        {
            retryCount++;
            logger.LogWarning(ex, "Failed to connect to database. Retrying in 3 seconds...");
            Thread.Sleep(3000);
        }
    }
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Endpoints
// --- Ambulatorios Endpoints ---
app.MapGet("/api/ambulatorios", async (AppDbContext db) => await db.Ambulatorios.FromSqlRaw("SELECT * FROM Ambulatorios").ToListAsync());
app.MapGet("/api/ambulatorios/{nroa}", async (int nroa, AppDbContext db) => await db.Ambulatorios.FromSqlRaw("SELECT * FROM Ambulatorios WHERE nroa = {0}", nroa).FirstOrDefaultAsync() is Ambulatorio a ? Results.Ok(a) : Results.NotFound());
app.MapPost("/api/ambulatorios", async (Ambulatorio a, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => { 
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        await db.Database.ExecuteSqlRawAsync("INSERT INTO Ambulatorios (nroa, andar, capacidade) VALUES ({0}, {1}, {2})", a.nroa, a.andar, a.capacidade);
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.Created($"/api/ambulatorios/{a.nroa}", a); 
    } catch(Exception ex) { 
        await transaction.RollbackAsync(); return Results.Problem(ex.Message); 
    }
});
app.MapPut("/api/ambulatorios/{nroa}", async (int nroa, Ambulatorio input, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        var execs = await db.Database.ExecuteSqlRawAsync("UPDATE Ambulatorios SET andar = {0}, capacidade = {1} WHERE nroa = {2}", input.andar, input.capacidade, nroa);
        if (execs == 0) return Results.NotFound();
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.NoContent();
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});
app.MapDelete("/api/ambulatorios/{nroa}", async (int nroa, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        var a = await db.Ambulatorios.FromSqlRaw("SELECT * FROM Ambulatorios WHERE nroa = {0}", nroa).FirstOrDefaultAsync();
        if (a == null) return Results.NotFound();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Ambulatorios WHERE nroa = {0}", nroa);
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.Ok(a); 
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});

// --- Medicos Endpoints ---
app.MapGet("/api/medicos", async (AppDbContext db) => await db.Medicos.FromSqlRaw("SELECT * FROM Medicos").ToListAsync());
app.MapGet("/api/medicos/{codm}", async (int codm, AppDbContext db) => await db.Medicos.FromSqlRaw("SELECT * FROM Medicos WHERE codm = {0}", codm).FirstOrDefaultAsync() is Medico m ? Results.Ok(m) : Results.NotFound());
app.MapPost("/api/medicos", async (Medico m, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => { 
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        await db.Database.ExecuteSqlRawAsync("INSERT INTO Medicos (codm, nome, idade, especialidade, RG, cidade, nroa) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})", m.codm, m.nome, m.idade, m.especialidade, m.RG, m.cidade, m.nroa);
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.Created($"/api/medicos/{m.codm}", m); 
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});
app.MapPut("/api/medicos/{codm}", async (int codm, Medico input, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        var execs = await db.Database.ExecuteSqlRawAsync("UPDATE Medicos SET nome = {0}, idade = {1}, especialidade = {2}, RG = {3}, cidade = {4}, nroa = {5} WHERE codm = {6}", input.nome, input.idade, input.especialidade, input.RG, input.cidade, input.nroa, codm);
        if (execs == 0) return Results.NotFound();
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.NoContent();
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});
app.MapDelete("/api/medicos/{codm}", async (int codm, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        var m = await db.Medicos.FromSqlRaw("SELECT * FROM Medicos WHERE codm = {0}", codm).FirstOrDefaultAsync();
        if (m == null) return Results.NotFound();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Medicos WHERE codm = {0}", codm);
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.Ok(m); 
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});

// --- Pacientes Endpoints ---
app.MapGet("/api/pacientes", async (AppDbContext db) => await db.Pacientes.FromSqlRaw("SELECT * FROM Pacientes").ToListAsync());
app.MapGet("/api/pacientes/{codp}", async (int codp, AppDbContext db) => await db.Pacientes.FromSqlRaw("SELECT * FROM Pacientes WHERE codp = {0}", codp).FirstOrDefaultAsync() is Paciente p ? Results.Ok(p) : Results.NotFound());
app.MapPost("/api/pacientes", async (Paciente p, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        await db.Database.ExecuteSqlRawAsync("INSERT INTO Pacientes (codp, nome, idade, cidade, RG, problema) VALUES ({0}, {1}, {2}, {3}, {4}, {5})", p.codp, p.nome, p.idade, p.cidade, p.RG, p.problema);
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.Created($"/api/pacientes/{p.codp}", p); 
    } catch (Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});
app.MapPut("/api/pacientes/{codp}", async (int codp, Paciente input, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        var execs = await db.Database.ExecuteSqlRawAsync("UPDATE Pacientes SET nome = {0}, idade = {1}, cidade = {2}, RG = {3}, problema = {4} WHERE codp = {5}", input.nome, input.idade, input.cidade, input.RG, input.problema, codp);
        if (execs == 0) return Results.NotFound();
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.NoContent();
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});
app.MapDelete("/api/pacientes/{codp}", async (int codp, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        var p = await db.Pacientes.FromSqlRaw("SELECT * FROM Pacientes WHERE codp = {0}", codp).FirstOrDefaultAsync();
        if (p == null) return Results.NotFound();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Pacientes WHERE codp = {0}", codp);
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.Ok(p); 
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});

// --- Consultas Endpoints ---
app.MapGet("/api/consultas", async (AppDbContext db) => await db.Consultas.FromSqlRaw("SELECT * FROM Consultas").ToListAsync());
app.MapPost("/api/consultas", async (Consulta c, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => { 
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        await db.Database.ExecuteSqlRawAsync("INSERT INTO Consultas (codm, codp, data, hora) VALUES ({0}, {1}, {2}, {3})", c.codm, c.codp, c.data, c.hora);
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.Ok(c); 
    } catch (Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});
app.MapDelete("/api/consultas", async (int codm, int codp, DateTime data, TimeSpan hora, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        var c = await db.Consultas.FromSqlRaw("SELECT * FROM Consultas WHERE codm = {0} AND codp = {1} AND data = {2} AND hora = {3}", codm, codp, data, hora).FirstOrDefaultAsync();
        if (c == null) return Results.NotFound();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Consultas WHERE codm = {0} AND codp = {1} AND data = {2} AND hora = {3}", codm, codp, data, hora);
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.Ok(c);
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});

// --- Funcionarios Endpoints ---
app.MapGet("/api/funcionarios", async (AppDbContext db) => await db.Funcionarios.FromSqlRaw("SELECT * FROM Funcionarios").ToListAsync());
app.MapGet("/api/funcionarios/{codf}", async (int codf, AppDbContext db) => await db.Funcionarios.FromSqlRaw("SELECT * FROM Funcionarios WHERE codf = {0}", codf).FirstOrDefaultAsync() is Funcionario f ? Results.Ok(f) : Results.NotFound());
app.MapPost("/api/funcionarios", async (Funcionario f, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => { 
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        await db.Database.ExecuteSqlRawAsync("INSERT INTO Funcionarios (codf, nome, idade, RG, salario, depto, tempoServico) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})", f.codf, f.nome, f.idade, f.RG, f.salario, f.depto, f.tempoServico);
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.Created($"/api/funcionarios/{f.codf}", f); 
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});
app.MapPut("/api/funcionarios/{codf}", async (int codf, Funcionario input, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        var execs = await db.Database.ExecuteSqlRawAsync("UPDATE Funcionarios SET nome = {0}, idade = {1}, RG = {2}, salario = {3}, depto = {4}, tempoServico = {5} WHERE codf = {6}", input.nome, input.idade, input.RG, input.salario, input.depto, input.tempoServico, codf);
        if (execs == 0) return Results.NotFound();
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.NoContent();
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});
app.MapDelete("/api/funcionarios/{codf}", async (int codf, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        var f = await db.Funcionarios.FromSqlRaw("SELECT * FROM Funcionarios WHERE codf = {0}", codf).FirstOrDefaultAsync();
        if (f == null) return Results.NotFound();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Funcionarios WHERE codf = {0}", codf);
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.Ok(f); 
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});

app.MapGet("/api/relatorio/sql", async (AppDbContext db) => {
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("-- Relatório de Banco de Dados: Sistema Hospitalar");
    sb.AppendLine($"-- Gerado em: {DateTime.Now}");
    sb.AppendLine();
    sb.AppendLine("-- ==========================================");
    sb.AppendLine("-- 1. DDL (CREATE TABLES)");
    sb.AppendLine("-- ==========================================\n");

    sb.AppendLine("CREATE TABLE IF NOT EXISTS Ambulatorios (\n    nroa INT PRIMARY KEY,\n    andar INT NOT NULL,\n    capacidade INT NOT NULL\n);\n");
    sb.AppendLine("CREATE TABLE IF NOT EXISTS Pacientes (\n    codp INT PRIMARY KEY,\n    nome VARCHAR(100) NOT NULL,\n    idade INT NOT NULL,\n    cidade VARCHAR(100) NOT NULL,\n    RG VARCHAR(20) NOT NULL,\n    problema VARCHAR(100) NOT NULL\n);\n");
    sb.AppendLine("CREATE TABLE IF NOT EXISTS Medicos (\n    codm INT PRIMARY KEY,\n    nome VARCHAR(100) NOT NULL,\n    idade INT NOT NULL,\n    especialidade VARCHAR(100) NOT NULL,\n    RG VARCHAR(20) NOT NULL,\n    cidade VARCHAR(100) NOT NULL,\n    nroa INT,\n    FOREIGN KEY (nroa) REFERENCES Ambulatorios(nroa)\n);\n");
    sb.AppendLine("CREATE TABLE IF NOT EXISTS Consultas (\n    codm INT,\n    codp INT,\n    data DATE NOT NULL,\n    hora TIME NOT NULL,\n    PRIMARY KEY (codm, codp, data, hora),\n    FOREIGN KEY (codm) REFERENCES Medicos(codm),\n    FOREIGN KEY (codp) REFERENCES Pacientes(codp)\n);\n");
    sb.AppendLine("CREATE TABLE IF NOT EXISTS Funcionarios (\n    codf INT PRIMARY KEY,\n    nome VARCHAR(100) NOT NULL,\n    idade INT NOT NULL,\n    RG VARCHAR(20) NOT NULL,\n    salario DECIMAL(10, 2) NOT NULL,\n    depto VARCHAR(100) NOT NULL,\n    tempoServico INT NOT NULL\n);\n");

    sb.AppendLine("-- ==========================================");
    sb.AppendLine("-- 2. DML (INSERTS) COM CONTROLE TRANSACIONAL");
    sb.AppendLine("-- ==========================================\n");
    sb.AppendLine("START TRANSACTION;\n");

    var ambulatorios = await db.Ambulatorios.ToListAsync();
    sb.AppendLine("-- Tabela: Ambulatorios");
    foreach (var a in ambulatorios)
        sb.AppendLine($"INSERT INTO ambulatorios (nroa, andar, capacidade) VALUES ({a.nroa}, {a.andar}, {a.capacidade});");
    
    var medicos = await db.Medicos.ToListAsync();
    sb.AppendLine("\n-- Tabela: Medicos");
    foreach (var m in medicos)
        sb.AppendLine($"INSERT INTO medicos (codm, nome, idade, especialidade, RG, cidade, nroa) VALUES ({m.codm}, '{m.nome}', {m.idade}, '{m.especialidade}', '{m.RG}', '{m.cidade}', {(m.nroa.HasValue ? m.nroa.Value.ToString() : "NULL")});");

    var pacientes = await db.Pacientes.ToListAsync();
    sb.AppendLine("\n-- Tabela: Pacientes");
    foreach (var p in pacientes)
        sb.AppendLine($"INSERT INTO pacientes (codp, nome, idade, cidade, RG, problema) VALUES ({p.codp}, '{p.nome}', {p.idade}, '{p.cidade}', '{p.RG}', '{p.problema}');");

    var consultas = await db.Consultas.ToListAsync();
    sb.AppendLine("\n-- Tabela: Consultas");
    foreach (var c in consultas)
        sb.AppendLine($"INSERT INTO consultas (codm, codp, data, hora) VALUES ({c.codm}, {c.codp}, '{c.data:yyyy-MM-dd}', '{c.hora}');");

    var funcionarios = await db.Funcionarios.ToListAsync();
    sb.AppendLine("\n-- Tabela: Funcionarios");
    foreach (var f in funcionarios)
        sb.AppendLine($"INSERT INTO funcionarios (codf, nome, idade, RG, salario, depto, tempoServico) VALUES ({f.codf}, '{f.nome}', {f.idade}, '{f.RG}', {f.salario.ToString(System.Globalization.CultureInfo.InvariantCulture)}, '{f.depto}', {f.tempoServico});");

    sb.AppendLine();
    sb.AppendLine("-- Confirma todas as inserções acima");
    sb.AppendLine("COMMIT;");
    
    return Results.Text(sb.ToString(), "text/plain");
});

app.MapPost("/api/transacao-manual/{acao}", async (string acao, Paciente p, AppDbContext db) => {
    // 1. Inicia Transação Explicitamente
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        db.Pacientes.Add(p);
        // 2. Salva no contexto local e gera o SQL no banco
        await db.SaveChangesAsync();
        
        // 3. Decide manualmente o futuro da Transação
        if (acao.ToUpper() == "COMMIT") {
            await transaction.CommitAsync();
            return Results.Ok(new { message = "✅ SUCESSO! COMMIT executado. O novo paciente foi gravado de forma definitiva no Banco de Dados." });
        } else {
            await transaction.RollbackAsync();
            return Results.Ok(new { message = "⚠️ ROLLBACK executado! A operação foi desfeita. O paciente foi descartado e NÃO foi salvo no banco." });
        }
    } catch(Exception ex) {
        await transaction.RollbackAsync();
        return Results.Problem("Erro na operação: " + ex.Message);
    }
});

app.Run();
