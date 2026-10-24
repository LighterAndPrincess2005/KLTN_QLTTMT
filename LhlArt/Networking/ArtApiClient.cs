using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LhlArt.Networking;

public sealed class ArtApiClient : IDisposable
{
    readonly HttpClient http;
    public string BaseUrl { get; }
    public string Username { get; private set; } = "";
    public string Role { get; private set; } = "";
    public long Permissions { get; private set; }
    public bool Has(long permission) => (Permissions & permission) == permission;
    public ArtApiClient(HttpMessageHandler? handler = null, string? baseUrl = null)
    {
        var file = Path.Combine(AppContext.BaseDirectory, "api.config.json");
        if (baseUrl is null && File.Exists(file))
        {
            using var config = JsonDocument.Parse(File.ReadAllText(file));
            baseUrl = config.RootElement.GetProperty("BaseUrl").GetString();
        }
        BaseUrl = (baseUrl ?? "http://127.0.0.1:5080").TrimEnd('/') + "/";
        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var address) || address.Scheme is not ("http" or "https"))
            throw new InvalidOperationException("Địa chỉ API trong api.config.json không hợp lệ.");
        http = handler is null ? new HttpClient() : new HttpClient(handler);
        http.BaseAddress = address;
        http.Timeout = TimeSpan.FromSeconds(20);
    }
    public async Task Login(string name, string password)
    {
        http.DefaultRequestHeaders.Authorization = null;
        Permissions = 0; Role = ""; Username = "";
        var result = await Send(HttpMethod.Post, "api/tai-khoan/dang-nhap", new { TenDangNhap = name, MatKhau = password });
        var role = Get(result,"vaiTroChinh").GetString() ?? "";
        if (role == "THANH_VIEN") throw new InvalidOperationException("Dùng tài khoản nhân viên để quản lý. Tài khoản học viên/phụ huynh sử dụng website.");
        var token = Get(result,"accessToken").GetString();
        if (string.IsNullOrEmpty(token)) throw new InvalidOperationException("API không trả về phiên đăng nhập hợp lệ.");
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Role = role; Username = Get(result,"tenDangNhap").GetString() ?? name;
        Permissions = Get(result,"quyen").GetInt64();
    }
    public Task<JsonElement> Read(string path) => Send(HttpMethod.Get, path);
    public Task<JsonElement> Post(string path, object body) => Send(HttpMethod.Post, path, body);
    public Task<JsonElement> Put(string path, object body, string? version = null) => Send(HttpMethod.Put,path,body,version);
    async Task<JsonElement> Send(HttpMethod method, string path, object? body = null, string? version = null)
    {
        using var request = new HttpRequestMessage(method,path);
        if (body is not null) request.Content = JsonContent.Create(body);
        if (version is not null) request.Headers.TryAddWithoutValidation("If-Match",version);
        HttpResponseMessage response;
        try { response = await http.SendAsync(request); }
        catch (HttpRequestException) { throw new InvalidOperationException($"Không kết nối được API tại {BaseUrl}. Hãy bật API hoặc kiểm tra api.config.json."); }
        catch (TaskCanceledException) { throw new InvalidOperationException("API phản hồi quá lâu. Hãy kiểm tra kết nối và tải lại trước khi gửi lại thao tác."); }
        using (response)
        {
            var text = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var message = $"API từ chối thao tác ({(int)response.StatusCode}).";
                try
                {
                    using var error = JsonDocument.Parse(text);
                    foreach (var key in new[] {"error","thongBao","message","detail","title"})
                        if (TryGet(error.RootElement,key,out var value) && value.ValueKind == JsonValueKind.String) { message=value.GetString()!; break; }
                    if (TryGet(error.RootElement,"errors",out var errors)) message += "\n"+string.Join("\n",errors.EnumerateObject().SelectMany(x=>x.Value.EnumerateArray().Select(y=>y.GetString())));
                }
                catch (JsonException) { }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    message += "\nĐăng nhập lại nếu phiên đã hết hạn.";
                    if(http.DefaultRequestHeaders.Authorization is not null)throw new ApiSessionExpiredException(message);
                    throw new ApiLoginRejectedException(message);
                }
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new ApiNotFoundException("Không tìm thấy dữ liệu hoặc chức năng trên API (404). Kiểm tra phiên bản API và tải lại.\n"+path);
                throw new InvalidOperationException(message);
            }
            if (string.IsNullOrWhiteSpace(text)) return JsonSerializer.SerializeToElement(new { });
            using var document = JsonDocument.Parse(text);
            return document.RootElement.Clone();
        }
    }
    public static bool TryGet(JsonElement item, string name, out JsonElement value)
    {
        if (item.ValueKind == JsonValueKind.Object)
            foreach (var field in item.EnumerateObject()) if (field.Name.Equals(name,StringComparison.OrdinalIgnoreCase)) { value=field.Value; return true; }
        value=default; return false;
    }
    public static JsonElement Get(JsonElement item,string name) => TryGet(item,name,out var value) ? value : throw new InvalidOperationException("API thiếu trường "+name);
    public static string Text(JsonElement item,string name) => TryGet(item,name,out var value)&&value.ValueKind!=JsonValueKind.Null?value.ToString():"";
    public void Dispose() => http.Dispose();
}
public sealed class ApiSessionExpiredException(string message):InvalidOperationException(message);
public sealed class ApiNotFoundException(string message):InvalidOperationException(message);
public sealed class ApiLoginRejectedException(string message):InvalidOperationException(message);
