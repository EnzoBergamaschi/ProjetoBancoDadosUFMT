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
app.MapGet("/api/ambulatorios", async (AppDbContext db) => await db.Ambulatorios.ToListAsync());
app.MapGet("/api/ambulatorios/{nroa}", async (int nroa, AppDbContext db) => await db.Ambulatorios.FindAsync(nroa) is Ambulatorio a ? Results.Ok(a) : Results.NotFound());
app.MapPost("/api/ambulatorios", async (Ambulatorio a, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => { 
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        db.Ambulatorios.Add(a); await db.SaveChangesAsync(); 
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.Created($"/api/ambulatorios/{a.nroa}", a); 
    } catch(Exception ex) { 
        await transaction.RollbackAsync(); return Results.Problem(ex.Message); 
    }
});
app.MapPut("/api/ambulatorios/{nroa}", async (int nroa, Ambulatorio input, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        var a = await db.Ambulatorios.FindAsync(nroa);
        if (a is null) return Results.NotFound();
        a.andar = input.andar; a.capacidade = input.capacidade;
        await db.SaveChangesAsync(); 
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.NoContent();
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});
app.MapDelete("/api/ambulatorios/{nroa}", async (int nroa, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        if (await db.Ambulatorios.FindAsync(nroa) is Ambulatorio a) { 
            db.Ambulatorios.Remove(a); await db.SaveChangesAsync(); 
            if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
            await transaction.CommitAsync(); return Results.Ok(a); 
        }
        return Results.NotFound();
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});

// --- Medicos Endpoints ---
app.MapGet("/api/medicos", async (AppDbContext db) => await db.Medicos.ToListAsync());
app.MapGet("/api/medicos/{codm}", async (int codm, AppDbContext db) => await db.Medicos.FindAsync(codm) is Medico m ? Results.Ok(m) : Results.NotFound());
app.MapPost("/api/medicos", async (Medico m, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => { 
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        db.Medicos.Add(m); await db.SaveChangesAsync(); 
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.Created($"/api/medicos/{m.codm}", m); 
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});
app.MapPut("/api/medicos/{codm}", async (int codm, Medico input, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        var m = await db.Medicos.FindAsync(codm);
        if (m is null) return Results.NotFound();
        m.nome = input.nome; m.idade = input.idade; m.especialidade = input.especialidade; m.RG = input.RG; m.cidade = input.cidade; m.nroa = input.nroa;
        await db.SaveChangesAsync(); 
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.NoContent();
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});
app.MapDelete("/api/medicos/{codm}", async (int codm, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        if (await db.Medicos.FindAsync(codm) is Medico m) { 
            db.Medicos.Remove(m); await db.SaveChangesAsync(); 
            if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
            await transaction.CommitAsync(); return Results.Ok(m); 
        }
        return Results.NotFound();
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});

// --- Pacientes Endpoints ---
app.MapGet("/api/pacientes", async (AppDbContext db) => await db.Pacientes.ToListAsync());
app.MapGet("/api/pacientes/{codp}", async (int codp, AppDbContext db) => await db.Pacientes.FindAsync(codp) is Paciente p ? Results.Ok(p) : Results.NotFound());
app.MapPost("/api/pacientes", async (Paciente p, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        db.Pacientes.Add(p); await db.SaveChangesAsync(); 
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.Created($"/api/pacientes/{p.codp}", p); 
    } catch (Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});
app.MapPut("/api/pacientes/{codp}", async (int codp, Paciente input, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        var p = await db.Pacientes.FindAsync(codp);
        if (p is null) return Results.NotFound();
        p.nome = input.nome; p.idade = input.idade; p.cidade = input.cidade; p.RG = input.RG; p.problema = input.problema;
        await db.SaveChangesAsync(); 
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.NoContent();
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});
app.MapDelete("/api/pacientes/{codp}", async (int codp, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        if (await db.Pacientes.FindAsync(codp) is Paciente p) { 
            db.Pacientes.Remove(p); await db.SaveChangesAsync(); 
            if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
            await transaction.CommitAsync(); return Results.Ok(p); 
        }
        return Results.NotFound();
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});

// --- Consultas Endpoints ---
app.MapGet("/api/consultas", async (AppDbContext db) => await db.Consultas.ToListAsync());
app.MapPost("/api/consultas", async (Consulta c, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => { 
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        db.Consultas.Add(c); await db.SaveChangesAsync(); 
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.Ok(c); 
    } catch (Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});
app.MapDelete("/api/consultas", async (int codm, int codp, DateTime data, TimeSpan hora, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        var c = await db.Consultas.FindAsync(codm, codp, data, hora);
        if (c is null) return Results.NotFound();
        db.Consultas.Remove(c); await db.SaveChangesAsync(); 
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.Ok(c);
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});

// --- Funcionarios Endpoints ---
app.MapGet("/api/funcionarios", async (AppDbContext db) => await db.Funcionarios.ToListAsync());
app.MapGet("/api/funcionarios/{codf}", async (int codf, AppDbContext db) => await db.Funcionarios.FindAsync(codf) is Funcionario f ? Results.Ok(f) : Results.NotFound());
app.MapPost("/api/funcionarios", async (Funcionario f, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => { 
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        db.Funcionarios.Add(f); await db.SaveChangesAsync(); 
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.Created($"/api/funcionarios/{f.codf}", f); 
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});
app.MapPut("/api/funcionarios/{codf}", async (int codf, Funcionario input, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        var f = await db.Funcionarios.FindAsync(codf);
        if (f is null) return Results.NotFound();
        f.nome = input.nome; f.idade = input.idade; f.RG = input.RG; f.salario = input.salario; f.depto = input.depto; f.tempoServico = input.tempoServico;
        await db.SaveChangesAsync(); 
        if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
        await transaction.CommitAsync(); return Results.NoContent();
    } catch(Exception ex) {
        await transaction.RollbackAsync(); return Results.Problem(ex.Message);
    }
});
app.MapDelete("/api/funcionarios/{codf}", async (int codf, AppDbContext db, [Microsoft.AspNetCore.Mvc.FromQuery] bool? rollback) => {
    using var transaction = await db.Database.BeginTransactionAsync();
    try {
        if (await db.Funcionarios.FindAsync(codf) is Funcionario f) { 
            db.Funcionarios.Remove(f); await db.SaveChangesAsync(); 
            if (rollback == true) throw new Exception("ROLLBACK FORÇADO MANUALMENTE!");
            await transaction.CommitAsync(); return Results.Ok(f); 
        }
        return Results.NotFound();
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
