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
/// Service used to handle PoE OAuth token.
/// </summary>
public class TokenService : ITokenService
{
    private static IServiceProvider _serviceProvider;

    public OAuthToken CacheToken { get; private set; }

    public TokenService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Load();
    }

    public void Clear()
    {
        TokenStorage.DeleteToken();
        CacheToken = null;
    }

    public void Load()
    {
        try
        {
            CacheToken = TokenStorage.LoadToken();
        }
        catch (Exception ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n"
                , ex.Source, ex.Message, ex.StackTrace)
                , "LoadToken error", MessageStatus.Error);
        }
    }

    public bool TryParseQuery(ReadOnlySpan<char> query)
    {
        var token = GetTokenFromQuery(query);
        if (token is null)
        {
            return false;
        }
        try
        {
            TokenStorage.SaveToken(token);
            CacheToken = token;
            return true;
        }
        catch (Exception ex) 
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n"
                , ex.Source, ex.Message, ex.StackTrace)
                , "SaveToken error", MessageStatus.Error);
        }
        return false;
    }

    private static OAuthToken GetTokenFromQuery(ReadOnlySpan<char> query)
    {
        var queryParams = HttpUtility.ParseQueryString(query.ToString());
        if (queryParams.Count > 1
            && query.Contains("access_token", StringComparison.Ordinal)
            && query.Contains("expires_in", StringComparison.Ordinal))
        {
            var token = queryParams["access_token"];
            if (int.TryParse(queryParams["expires_in"], out var expireInDays))
            {
                return new(token, expireInDays);
            }
        }
        return null;
    }

    private static class TokenStorage
    {
        private static readonly string AppFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            , Common.GetAppName() , Common.GetHash(Environment.UserName));
        private static readonly string TokenPath = Path.Combine(AppFolder, Common.GetHash(Environment.UserName));
        private static readonly string KeyPath = Path.Combine(AppFolder, Common.GetHash(Environment.MachineName));

        internal static OAuthToken LoadToken()
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
                    return null;
                }
                return token;
            }
            catch (Exception)
            {
                throw; //no new
            }
        }

        internal static void SaveToken(OAuthToken token)
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
            catch (Exception)
            {
                throw; //no new
            }
        }

        internal static void DeleteToken()
        {
            if (Directory.Exists(AppFolder))
                Directory.Delete(AppFolder, recursive: true);
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
}
