using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string encryptionKey = "MyEncryptionKey123";

var klass = new Klass
{
    id = 1,
    namn = "KlassA",
    elever = new List<Elev>
    {
        new Elev { id = 1, namn = "Jessica", ålder = 15},
        new Elev { id = 2, namn = "Kevin", ålder = 14}
    }
};


var encryptionService = new EncryptionService();

app.MapGet("/klass", (HttpContext httpContext) => Results.Ok(klass));

app.MapGet("/elever/{id}", (HttpContext httpContext, int id) =>
{
    var elev = klass.elever.FirstOrDefault(e => e.id == id);
    return elev != null ? Results.Ok(encryptionService.Encrypt(JsonSerializer.Serialize(elev))) : Results.NotFound();
});

app.MapPost("/elever", (HttpContext httpContext) =>
{
    var encryptedElev = httpContext.Request.ReadFromJsonAsync<string>().Result;
    var decryptedElev = JsonSerializer.Deserialize<Elev>(encryptionService.Decrypt(encryptedElev));
    klass.elever.Add(decryptedElev);
    return Results.Created($"/elever/{decryptedElev.id}", decryptedElev);
});

// Endpoint för att kryptera och avkryptera elev id
app.MapGet("/elevid", (HttpContext httpContext) => Results.Ok(encryptionService.Encrypt(klass.id.ToString())));

app.MapPost("/elevid", (HttpContext httpContext) =>
{
    var encryptedId = httpContext.Request.ReadFromJsonAsync<string>().Result;
    var decryptedId = encryptionService.Decrypt(encryptedId);
    klass.id = int.Parse(decryptedId);
    return Results.Ok(klass.id);
});

app.Run();

public class Klass
{
    public int id { get; set; }
    public string namn { get; set; } = "";
    public List<Elev> elever { get; set; }

    public Klass()
    {
        elever = new List<Elev>();
    }
}

public class Elev
{
    public int id { get; set; }
    public string namn { get; set; } = "";
    public int ålder { get; set; }

    public Elev()
    {
        ålder = 0;
    }
}

public class EncryptionService
{
    public string Encrypt(string plaintext)
    {
        byte[] textAsBytes = Encoding.UTF8.GetBytes(plaintext);
        return Convert.ToBase64String(textAsBytes);
    }

    public string Decrypt(string encryptedText)
    {
        byte[] textAsBytes = Convert.FromBase64String(encryptedText);
        return Encoding.UTF8.GetString(textAsBytes);
    }
}
