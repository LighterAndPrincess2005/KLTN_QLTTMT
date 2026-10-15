using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace MyThuat.Api.Services;
public static class RequestKeys
{
    public static string Fingerprint<T>(T request)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request))));
    public static string Key(long actor,string key)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(actor+":"+key)));
}
