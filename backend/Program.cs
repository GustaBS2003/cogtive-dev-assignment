using System;
using Microsoft.EntityFrameworkCore;
using Cogtive.DevAssignment.Api.Data;
using Cogtive.DevAssignment.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp",
        policy => policy
            .WithOrigins("http://localhost:3000", "http://web", "http://web:80")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Configure DbContext with SQLite by default or PostgreSQL via env variable
var dbProvider = builder.Configuration["DATABASE_PROVIDER"] ?? "Sqlite";
if (dbProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));
}

var app = builder.Build();

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try 
    {
        logger.LogInformation($"Using database provider: {dbProvider}");
        
        if (context.Database.CanConnect())
        {
            try
            {
                // This will throw exception if tables don't exist
                var machinesExist = context.Machines.Any();
                logger.LogInformation("Database connection successful. Tables already exist.");
            }
            catch
            {
                // Tables don't exist, recreate the database
                logger.LogWarning("Database exists but tables are missing. Recreating database...");
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                InitSeedData(context);
                logger.LogInformation("Database recreated with seed data.");
            }
        }
        else
        {
            // Database doesn't exist, create it
            logger.LogInformation("Creating new database with initial schema...");
            
            // For PostgreSQL, it's better to use migrations
            if (dbProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
            {
                context.Database.Migrate();
            }
            else
            {
                context.Database.EnsureCreated();
            }
            
            InitSeedData(context);
            logger.LogInformation("New database created with seed data.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowWebApp");

// Define API endpoints
app.MapGet("/api/machines", async (AppDbContext context) =>
    await context.Machines.ToListAsync())
    .WithName("GetMachines")
    .Produces<List<Machine>>(StatusCodes.Status200OK);

app.MapGet("/api/machines/{id}", async (int id, AppDbContext context) =>
{
    var machine = await context.Machines.FindAsync(id);
    return machine is null ? Results.NotFound() : Results.Ok(machine);
})
.WithName("GetMachine")
.Produces<Machine>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapGet("/api/production-data", async (AppDbContext context) =>
    await context.ProductionData.ToListAsync())
    .WithName("GetProductionData")
    .Produces<List<ProductionData>>(StatusCodes.Status200OK);

app.MapGet("/api/machines/{id}/production-data", async (int id, AppDbContext context) =>
{
    var data = await context.ProductionData
        .Where(p => p.MachineId == id)
        .OrderByDescending(p => p.Timestamp)
        .ToListAsync();

    return data.Count == 0 ? Results.NotFound() : Results.Ok(data);
})
.WithName("GetMachineProductionData")
.Produces<List<ProductionData>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

// Optional endpoint for seniors to implement IoT data reception
app.MapPost("/api/production-data", async (ProductionData data, AppDbContext context) =>
{
    context.ProductionData.Add(data);
    await context.SaveChangesAsync();
    return Results.Created($"/api/production-data/{data.Id}", data);
})
.WithName("AddProductionData")
.Produces<ProductionData>(StatusCodes.Status201Created);

app.Run();

// Helper method to seed initial data
void InitSeedData(AppDbContext context)
{
    // Seed machines
    context.Machines.AddRange(
        new Machine { 
            Id = 1, 
            Name = "CNC Machine Alpha", 
            SerialNumber = "CNC-2023-001", 
            Type = "CNC", 
            IsActive = true 
        },
        new Machine { 
            Id = 2, 
            Name = "Injection Molder Beta", 
            SerialNumber = "INJ-2022-042", 
            Type = "Injection", 
            IsActive = true 
        },
        new Machine { 
            Id = 3, 
            Name = "Assembly Line Gamma", 
            SerialNumber = "ASM-2021-007", 
            Type = "Assembly", 
            IsActive = false 
        }
    );
    context.SaveChanges();
    
    // Seed production data
    var now = DateTime.UtcNow;
    context.ProductionData.AddRange(
        new ProductionData { 
            MachineId = 1, 
            Timestamp = now.AddHours(-4), 
            Efficiency = 92.7m, // Intentional error: string instead of decimal
            UnitsProduced = 427, 
            Downtime = 24 
        },
        new ProductionData { 
            MachineId = 2, 
            Timestamp = now.AddHours(-3), 
            Efficiency = 88.3m, 
            UnitsProduced = 195, 
            Downtime = 32 
        },
        new ProductionData { 
            MachineId = 1, 
            Timestamp = now.AddHours(-2), 
            Efficiency = 95.1m, 
            UnitsProduced = 512, 
            Downtime = 15 
        }
    );
    context.SaveChanges();
}