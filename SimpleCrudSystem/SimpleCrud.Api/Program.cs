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
app.MapPost("/api/ambulatorios", async (Ambulatorio a, AppDbContext db) => { db.Ambulatorios.Add(a); await db.SaveChangesAsync(); return Results.Created($"/api/ambulatorios/{a.nroa}", a); });
app.MapPut("/api/ambulatorios/{nroa}", async (int nroa, Ambulatorio input, AppDbContext db) => {
    var a = await db.Ambulatorios.FindAsync(nroa);
    if (a is null) return Results.NotFound();
    a.andar = input.andar; a.capacidade = input.capacidade;
    await db.SaveChangesAsync(); return Results.NoContent();
});
app.MapDelete("/api/ambulatorios/{nroa}", async (int nroa, AppDbContext db) => {
    if (await db.Ambulatorios.FindAsync(nroa) is Ambulatorio a) { db.Ambulatorios.Remove(a); await db.SaveChangesAsync(); return Results.Ok(a); }
    return Results.NotFound();
});

// --- Medicos Endpoints ---
app.MapGet("/api/medicos", async (AppDbContext db) => await db.Medicos.ToListAsync());
app.MapGet("/api/medicos/{codm}", async (int codm, AppDbContext db) => await db.Medicos.FindAsync(codm) is Medico m ? Results.Ok(m) : Results.NotFound());
app.MapPost("/api/medicos", async (Medico m, AppDbContext db) => { db.Medicos.Add(m); await db.SaveChangesAsync(); return Results.Created($"/api/medicos/{m.codm}", m); });
app.MapPut("/api/medicos/{codm}", async (int codm, Medico input, AppDbContext db) => {
    var m = await db.Medicos.FindAsync(codm);
    if (m is null) return Results.NotFound();
    m.nome = input.nome; m.idade = input.idade; m.especialidade = input.especialidade; m.RG = input.RG; m.cidade = input.cidade; m.nroa = input.nroa;
    await db.SaveChangesAsync(); return Results.NoContent();
});
app.MapDelete("/api/medicos/{codm}", async (int codm, AppDbContext db) => {
    if (await db.Medicos.FindAsync(codm) is Medico m) { db.Medicos.Remove(m); await db.SaveChangesAsync(); return Results.Ok(m); }
    return Results.NotFound();
});

// --- Pacientes Endpoints ---
app.MapGet("/api/pacientes", async (AppDbContext db) => await db.Pacientes.ToListAsync());
app.MapGet("/api/pacientes/{codp}", async (int codp, AppDbContext db) => await db.Pacientes.FindAsync(codp) is Paciente p ? Results.Ok(p) : Results.NotFound());
app.MapPost("/api/pacientes", async (Paciente p, AppDbContext db) => { db.Pacientes.Add(p); await db.SaveChangesAsync(); return Results.Created($"/api/pacientes/{p.codp}", p); });
app.MapPut("/api/pacientes/{codp}", async (int codp, Paciente input, AppDbContext db) => {
    var p = await db.Pacientes.FindAsync(codp);
    if (p is null) return Results.NotFound();
    p.nome = input.nome; p.idade = input.idade; p.cidade = input.cidade; p.RG = input.RG; p.problema = input.problema;
    await db.SaveChangesAsync(); return Results.NoContent();
});
app.MapDelete("/api/pacientes/{codp}", async (int codp, AppDbContext db) => {
    if (await db.Pacientes.FindAsync(codp) is Paciente p) { db.Pacientes.Remove(p); await db.SaveChangesAsync(); return Results.Ok(p); }
    return Results.NotFound();
});

// --- Consultas Endpoints ---
app.MapGet("/api/consultas", async (AppDbContext db) => await db.Consultas.ToListAsync());
app.MapPost("/api/consultas", async (Consulta c, AppDbContext db) => { db.Consultas.Add(c); await db.SaveChangesAsync(); return Results.Ok(c); });
app.MapDelete("/api/consultas", async (int codm, int codp, DateTime data, TimeSpan hora, AppDbContext db) => {
    var c = await db.Consultas.FindAsync(codm, codp, data, hora);
    if (c is null) return Results.NotFound();
    db.Consultas.Remove(c); await db.SaveChangesAsync(); return Results.Ok(c);
});

app.Run();
