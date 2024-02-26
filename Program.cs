using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

app.MapGet("/klass", (HttpContext httpContext) => Results.Ok(klass));

app.MapGet("/elever/{id}", (HttpContext httpContext, int id) =>
{
    var elev = klass.elever.FirstOrDefault(e => e.id == id);
    return elev != null ? Results.Ok(EncryptData(elev, encryptionKey)) : Results.NotFound();
});

app.MapPost("/elever", (HttpContext httpContext) =>
{
    var encryptedElev = httpContext.Request.ReadFromJsonAsync<string>().Result;
    var decryptedElev = DecryptData<Elev>(encryptedElev, encryptionKey);
    klass.elever.Add(decryptedElev);
    return Results.Created($"/elever/{decryptedElev.id}", decryptedElev);
});

app.Run();

static string EncryptData<T>(T data, string key)
{
    var jsonString = JsonSerializer.Serialize(data);
    byte[] jsonData = Encoding.UTF8.GetBytes(jsonString);
    using var aes = Aes.Create();
    aes.Key = Encoding.UTF8.GetBytes(key);
    aes.IV = new byte[16];
    using var ms = new MemoryStream();
    using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
    cs.Write(jsonData, 0, jsonData.Length);
    cs.FlushFinalBlock();
    return Convert.ToBase64String(ms.ToArray());
}

static T DecryptData<T>(string encryptedData, string key)
{
    byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
    using var aes = Aes.Create();
    aes.Key = Encoding.UTF8.GetBytes(key);
    aes.IV = new byte[16];
    using var ms = new MemoryStream();
    using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
    cs.Write(encryptedBytes, 0, encryptedBytes.Length);
    cs.FlushFinalBlock();
    byte[] decryptedBytes = ms.ToArray();
    string decryptedString = Encoding.UTF8.GetString(decryptedBytes);
    return JsonSerializer.Deserialize<T>(decryptedString);
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
    public string namn { get; set; } = "";
    public int ålder { get; set; }

    public Elev()
    {
        ålder = 0;
    }
}
