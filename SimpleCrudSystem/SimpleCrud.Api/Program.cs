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
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

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
app.MapGet("/api/items", async (AppDbContext db) =>
    await db.Items.ToListAsync())
    .WithName("GetItems");

app.MapGet("/api/items/{id}", async (int id, AppDbContext db) =>
    await db.Items.FindAsync(id)
        is Item item
            ? Results.Ok(item)
            : Results.NotFound())
    .WithName("GetItemById");

app.MapPost("/api/items", async (Item item, AppDbContext db) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/api/items/{item.Id}", item);
})
.WithName("CreateItem");

app.MapPut("/api/items/{id}", async (int id, Item inputItem, AppDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    item.Name = inputItem.Name;
    item.Description = inputItem.Description;
    
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("UpdateItem");

app.MapDelete("/api/items/{id}", async (int id, AppDbContext db) =>
{
    if (await db.Items.FindAsync(id) is Item item)
    {
        db.Items.Remove(item);
        await db.SaveChangesAsync();
        return Results.Ok(item);
    }

    return Results.NotFound();
})
.WithName("DeleteItem");

app.Run();
