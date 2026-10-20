using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDataContext>();
var app = builder.Build();

app.MapPost("/api/folha/cadastrar", (AppDataContext ctx, Folha folhaNova) =>
{
    if (folhaNova.Mes < 1 || folhaNova.Mes > 12)
    {
        return Results.BadRequest("Mes deve estar entre 1 e 12");
    }

    if (folhaNova.Ano < 2000)
    {
        return Results.BadRequest("Ano deve ser maior ou igual a 2000");
    }

    var folhaAntiga = ctx.Folhas.FirstOrDefault(f => f.Cpf == folhaNova.Cpf && f.Mes == folhaNova.Mes && f.Ano == folhaNova.Ano);

    if (folhaAntiga is not null)
    {
        return Results.Conflict("Ja existe folha para esse cpf, mes e ano");
    }

    folhaNova.SalarioBruto = folhaNova.HorasTrabalhadas * folhaNova.ValorHora;

    if(folhaNova.SalarioBruto <= 1903.98)
    {
        folhaNova.ImpostoRenda = 0;
    }
    else if(folhaNova.SalarioBruto <= 2826.65)
    {
        folhaNova.ImpostoRenda = (folhaNova.SalarioBruto * 0.075) - 142.80;
    }
    else if(folhaNova.SalarioBruto <= 3751.05)
    {
        folhaNova.ImpostoRenda = (folhaNova.SalarioBruto * 0.15) - 354.80;
    }
    else if(folhaNova.SalarioBruto <= 4664.68)
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
    else if(folhaNova.SalarioBruto <= 2822.90)
    {
        folhaNova.Inss =folhaNova.SalarioBruto * 0.09;
    }
    else if(folhaNova.SalarioBruto <= 5645.80)
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
