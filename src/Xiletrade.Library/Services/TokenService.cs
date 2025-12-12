using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
    public OAuthToken CustomToken { get; private set; }

    public TokenService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void ClearTokens()
    {
        TokenStorage.DeleteTokens();
        CacheToken = null;
        CustomToken = null;
    }

    public void LoadTokens()
    {
        try
        {
            CacheToken = TokenStorage.LoadToken();
            CustomToken = TokenStorage.LoadToken(useCustom: true);
        }
        catch (Exception ex)
        {
            var service = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            service.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n"
                , ex.Source, ex.Message, ex.StackTrace)
                , "LoadToken error", MessageStatus.Error);
        }
    }

    public bool TryInitToken(ReadOnlySpan<char> query, bool useCustom = false)
    {
        var token = useCustom ? GetTokenFromString(query) : GetTokenFromQuery(query);
        if (token is null)
        {
            return false;
        }
        try
        {
            TokenStorage.SaveToken(token, useCustom);
            if (useCustom)
            {
                CustomToken = token;
            }
            else
            {
                CacheToken = token;
            }
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

    public bool TryGetToken(out string token, bool useCustom = false)
    {
        // TODO add missing logic
        // An HTTP 401 error will occur when the token has expired or has been revoked.
        var tokenObject = useCustom ? CustomToken : CacheToken;
        if (tokenObject is not null)
        {
            token = tokenObject.AccessToken; 
            return true;
        }
        token = string.Empty;
        return false;
    }

    private static OAuthToken GetTokenFromQuery(ReadOnlySpan<char> query)
    {
        var queryParams = HttpUtility.ParseQueryString(query.ToString());
        if (queryParams.Count > 1
            && query.Contain("access_token") && query.Contain("expires_in"))
        {
            var token = queryParams["access_token"];
            if (int.TryParse(queryParams["expires_in"], out var expireInDays))
            {
                return new(token, expireInDays);
            }
            return new(token, 2592000);
        }
        return null;
    }

    private static OAuthToken GetTokenFromString(ReadOnlySpan<char> valString)
    {
        if (!RegexUtil.MD5().IsMatch(valString))
        {
            return null;
        }
        return new(valString, 2592000);
    }

    private static class TokenStorage
    {
        private static readonly string AppFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            , Common.GetAppName() , Common.GetHash(Environment.UserName));
        private static readonly string TokenPath = Path.Combine(AppFolder, Common.GetHash(Environment.UserName));
        private static readonly string CustomTokenPath = Path.Combine(AppFolder, Common.GetHash(Environment.UserName + Environment.MachineName));
        private static readonly string KeyPath = Path.Combine(AppFolder, Common.GetHash(Environment.MachineName));
        
        private static string GetPath(bool useCustom) => useCustom ? CustomTokenPath : TokenPath;

        internal static OAuthToken LoadToken(bool useCustom = false)
        {
            try
            {
                var path = GetPath(useCustom);
                if (!File.Exists(path)) return null;

                var key = GetOrCreateKey();
                var allBytes = File.ReadAllBytes(path);

                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = allBytes[..16];

                var decryptor = aes.CreateDecryptor();
                var cipherBytes = allBytes[16..];
                var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                
                var json = Encoding.UTF8.GetString(plainBytes);
                var dm = _serviceProvider.GetRequiredService<DataManagerService>();
                var token = dm.Json.Deserialize<OAuthToken>(json);
                if (token is not null && token.IsExpired())
                {
                    DeleteToken(useCustom);
                    return null;
                }
                return token;
            }
            catch (Exception)
            {
                throw; //no new
            }
        }

        internal static void SaveToken(OAuthToken token, bool useCustom = false)
        {
            try
            {
                EnsureAppFolder();

                var dm = _serviceProvider.GetRequiredService<DataManagerService>();
                var json = dm.Json.Serialize<OAuthToken>(token);

                var key = GetOrCreateKey();
                using var aes = Aes.Create();
                aes.Key = key;
                aes.GenerateIV();

                var encryptor = aes.CreateEncryptor();
                var plainBytes = Encoding.UTF8.GetBytes(json);
                var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                var path = GetPath(useCustom);
                using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                fs.Write(aes.IV, 0, aes.IV.Length);
                fs.Write(cipherBytes, 0, cipherBytes.Length);
            }
            catch (Exception)
            {
                throw; //no new
            }
        }

        internal static void DeleteTokens()
        {
            if (Directory.Exists(AppFolder))
                Directory.Delete(AppFolder, recursive: true);
        }

        internal static void DeleteToken(bool isCustomToken = false)
        {
            if (!Directory.Exists(AppFolder))
            {
                return;
            }
            var cleanAll = File.Exists(GetPath(isCustomToken)) && !File.Exists(GetPath(!isCustomToken));
            if (cleanAll)
            {
                Directory.Delete(AppFolder, recursive: true);
                return;
            }

            var path = GetPath(isCustomToken);
            if (File.Exists(path))
            {
                File.Delete(path);
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
}
