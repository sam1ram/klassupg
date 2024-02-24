using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string encryptionKey = "MyEncryptionKey123";
// Endpoint för att hämta klassinformation
app.MapGet("/klass", () =>
{
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

    return Results.Ok(klass);
});

// Endpoint för att hämta en specifik elev baserat på id
app.MapGet("/elever/{id}", (int id) =>
{
    var elev = new List<Elev>
    {
        new Elev { id = 1, namn = "Jessica", ålder = 15},
        new Elev { id = 2, namn = "Kevin", ålder = 14}
    };

    var Elev = elev.FirstOrDefault(e => e.id == id);

    if (elev != null)
    {
        return Results.Ok(elev);
    }
    else
    {
        return Results.NotFound();
    }
});

// Endpoint för att lägga till en elev
app.MapPost("/elever", (HttpContext httpContext) =>
{
    var elev = httpContext.Request.ReadFromJsonAsync<Elev>().Result;
    // Lägg till en elev
    // Spara elev
    return Results.Created("/elever", elev);
});

app.Run();


    public static class EncryptionHelper
{ 
public static string EncryptData(Elev elev, string key)
    {
    var jsonString = JsonSerializer.Serialize(elev);
        byte[] jsonData = Encoding.UTF8.GetBytes(jsonString);
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = new byte[16]; 
        using MemoryStream ms = new MemoryStream();
        using CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(jsonData, 0, jsonData.Length);
        cs.FlushFinalBlock();
        return Convert.ToBase64String(ms.ToArray());
    }
}
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
    public string namn { get; set; } ="";
    public int ålder { get; set; } 

    public Elev()
    {
        
        ålder = 0;
    }
}
