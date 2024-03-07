using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// Lägg till tjänster för Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Skapar EncryptionService som en singleton-tjänst
builder.Services.AddSingleton<EncryptionService>();

var app = builder.Build();

// Använder Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// Krypterar endpoint
app.MapPost("/encrypt", (EncryptionService encryptionService, Klass klassToEncrypt) =>
{
    var klassJson = JsonSerializer.Serialize(klassToEncrypt);
    return Results.Ok(new { encryptedText = encryptionService.Encrypt(klassJson) });
});

// Avkrypterar endpoint
app.MapPost("/decrypt", (EncryptionService encryptionService, string encryptedText) =>
{
    var decryptedText = encryptionService.Decrypt(encryptedText);
    try
    {
        var klass = JsonSerializer.Deserialize<Klass>(decryptedText);
        return Results.Ok(klass);
    }
    catch
    {
        return Results.BadRequest("Decryption failed or the decrypted text is not a valid Klass object.");
    }
});

app.Run();

public class Klass
{
    public int id { get; set; }
    public string namn { get; set; } = "";
    public List<Elev> elever { get; set; } = new();
}

public class Elev
{
    public int id { get; set; }
    public string namn { get; set; } = "";
    public int ålder { get; set; }
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
