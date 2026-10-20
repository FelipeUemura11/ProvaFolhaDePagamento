using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDataContext>();
var app = builder.Build();

app.MapPost("/api/folha/cadastrar", (AppDataContext ctx, Folha folhaNova) =>
{
    var folhaAntiga = ctx.Folhas.FirstOrDefault(f => f.Cpf == folhaNova.Cpf);

    if (folhaAntiga is not null)
    {
        return Results.NotFound("Folha com cpf ja existe");
    }
   
    folhaNova.SalarioBruto = folhaNova.HorasTrabalhadas * folhaNova.ValorHora;

    if(folhaNova.SalarioBruto <= 1903.98)
    {
        folhaNova.ImpostoRenda = 0;
    }
    if(folhaNova.SalarioBruto <= 2826.65)
    {
        folhaNova.ImpostoRenda = (folhaNova.SalarioBruto * 0.075) - 142.80;
    }
    if(folhaNova.SalarioBruto <= 3751.05)
    {
        folhaNova.ImpostoRenda = (folhaNova.SalarioBruto * 0.15) - 354.80;
    }
    if(folhaNova.SalarioBruto <= 4664.68)
    {
        folhaNova.ImpostoRenda = (folhaNova.SalarioBruto * 0.255) - 636.13;
    }
    else
    {
        folhaNova.ImpostoRenda = (folhaNova.SalarioBruto * 0.275) - 869.36;
    }

    if(folhaNova.SalarioBruto <= 1693.72)
    {
        folhaNova.Inss =folhaNova.SalarioBruto * 0.08;
    }
    if(folhaNova.SalarioBruto <= 2822.90)
    {
        folhaNova.Inss =folhaNova.SalarioBruto * 0.09;
    }
    if(folhaNova.SalarioBruto <= 5645.80)
    {
        folhaNova.Inss =folhaNova.SalarioBruto * 0.11;
    }
    else
    {
        folhaNova.Inss = 621.03;
    }

    folhaNova.Fgts = folhaNova.SalarioBruto * 0.08;

    folhaNova.SalarioLiquido = folhaNova.SalarioBruto - folhaNova.ImpostoRenda - folhaNova.Inss;

    ctx.Folhas.Add(folhaNova);
    ctx.SaveChanges();
    return Results.Created($"/api/folha/cadastrar/{folhaNova.Id}", folhaNova);
});

app.MapGet("/api/folha/listar", (AppDataContext ctx) =>
{
    var lista = ctx.Folhas.ToList();

    if(lista is null)
    {
        return Results.NotFound("Lista vazia...");
    }
    return Results.Ok(lista);
    
});

app.MapGet("/api/folha/buscar/{cpf}/{mes}/{ano}", (AppDataContext ctx, int mes, int ano, string cpf) =>
{
    var folhaBuscar = ctx.Folhas.FirstOrDefault(f => f.Cpf == cpf && f.Mes == mes && f.Ano == ano);

    if(folhaBuscar is null)
    {
        return Results.NotFound("Folha nao encontrada");
    }
    return Results.Ok(folhaBuscar);
});

app.MapDelete("/api/folha/remover/{cpf}/{mes}/{ano}", (AppDataContext ctx, int mes, int ano, string cpf) =>
{
    var folhaBuscar = ctx.Folhas.FirstOrDefault(f => f.Cpf == cpf && f.Mes == mes && f.Ano == ano);

    if(folhaBuscar is null)
    {
        return Results.NotFound("Folha nao encontrada");
    }
    ctx.Folhas.Remove(folhaBuscar);
    ctx.SaveChanges();
    return Results.NoContent();
});

app.MapGet("/api/folha/total-liquido", (AppDataContext ctx) =>
{
    if (!ctx.Folhas.Any())
    {
        return Results.NotFound("Nenhuma folha cadastrada");
    }

    var total = ctx.Folhas.Sum(f => f.SalarioLiquido);
    return Results.Ok(new { totalLiquido = Math.Round(total, 2) });
});

app.Run();
