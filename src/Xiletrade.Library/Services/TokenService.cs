using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services;

/// <summary>
/// Service used to load and save poe OAuth token.
/// </summary>
internal class TokenService
{
    private static IServiceProvider _serviceProvider;

    public TokenService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static readonly string MachineIdHash = Common.GetMachineIdHash();
    private static readonly string AppFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Common.GetAppHash());
    private static readonly string TokenPath = Path.Combine(AppFolder, $"{MachineIdHash}.bin");
    private static readonly string KeyPath = Path.Combine(AppFolder, $"{MachineIdHash}.key");

    internal bool TryParseQueryToken(ReadOnlySpan<char> query)
    {
        var queryParams = HttpUtility.ParseQueryString(query.ToString());
        if (queryParams.Count > 1
            && query.Contains("access_token", StringComparison.Ordinal)
            && query.Contains("expires_in", StringComparison.Ordinal))
        {
            SaveToken(new(queryParams["access_token"], queryParams["expires_in"]));
            return true;
        }
        return false;
    }

    internal OAuthToken LoadToken()
    {
        try
        {
            if (!File.Exists(TokenPath)) return null;

            var key = GetOrCreateKey();
            var allBytes = File.ReadAllBytes(TokenPath);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = allBytes[..16];

            var decryptor = aes.CreateDecryptor();
            var cipherBytes = allBytes[16..];
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            var json = Encoding.UTF8.GetString(plainBytes);
            var token = JsonSerializer.Deserialize<OAuthToken>(json);
            if (token is not null && token.IsExpired())
            {
                DeleteToken();
            }
            return token;
        }
        catch (Exception ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "LoadToken error", MessageStatus.Error);
        }
        return null;
    }

    private static void DeleteToken()
    {
        if (File.Exists(TokenPath))
            File.Delete(TokenPath);
    }

    private static void SaveToken(OAuthToken token)
    {
        try
        {
            EnsureAppFolder();

            var json = JsonSerializer.Serialize(token);
            var key = GetOrCreateKey();
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(json);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            using var fs = new FileStream(TokenPath, FileMode.Create, FileAccess.Write);
            fs.Write(aes.IV, 0, aes.IV.Length);
            fs.Write(cipherBytes, 0, cipherBytes.Length);
        }
        catch (Exception ex) 
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n", ex.Source, ex.Message, ex.StackTrace), "SaveToken error", MessageStatus.Error);
        }
    }

    private static void EnsureAppFolder()
    {
        if (!Directory.Exists(AppFolder))
            Directory.CreateDirectory(AppFolder);
    }

    private static byte[] GetOrCreateKey()
    {
        if (File.Exists(KeyPath))
            return File.ReadAllBytes(KeyPath);

        using var rng = RandomNumberGenerator.Create();
        var key = new byte[32]; // 256 bits
        rng.GetBytes(key);

        File.WriteAllBytes(KeyPath, key);
        return key;
    }
}
