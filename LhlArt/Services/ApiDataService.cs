using System.Globalization;
using System.Text;
using System.Text.Json;
using LhlArt.Networking;

namespace LhlArt.Services;

// API chưa hỗ trợ tìm kiếm danh mục: đọc đủ các trang rồi lọc trên giá trị đã giải mã.
public sealed class ApiDataService(ArtApiClient api)
{
    public async Task<List<JsonElement>> All(string route, bool paged = true)
    {
        var result = new List<JsonElement>();
        for (var page = 1; ; page++)
        {
            var data = await api.Read(route + (paged ? (route.Contains('?') ? "&" : "?") + $"trang={page}&kichThuoc=100" : ""));
            var list = ArtApiClient.TryGet(data, "duLieu", out var rows) ? rows : data;
            if (list.ValueKind != JsonValueKind.Array) return [data];
            var batch = list.EnumerateArray().Select(x => x.Clone()).ToList();
            result.AddRange(batch);
            if (!paged || batch.Count == 0 || !ArtApiClient.TryGet(data, "tongSo", out var total) || result.Count >= total.GetInt32()) return result;
        }
    }

    public static string Normalize(string value)
    {
        var chars = value.Normalize(NormalizationForm.FormD).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return string.Join(' ', new string(chars.ToArray()).Replace('đ', 'd').Replace('Đ', 'D').ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    public static bool Matches(JsonElement row, string query)
    {
        var values = row.ValueKind == JsonValueKind.Object ? row.EnumerateObject().Select(x => ValueText(x.Value)) : [ValueText(row)];
        var text = Normalize(string.Join(' ', values));
        return Normalize(query).Split(' ', StringSplitOptions.RemoveEmptyEntries).All(text.Contains);
    }
    static string ValueText(JsonElement v) => v.ValueKind switch
    {
        JsonValueKind.String => v.GetString() ?? "",
        JsonValueKind.Object => string.Join(' ', v.EnumerateObject().Select(x => ValueText(x.Value))),
        JsonValueKind.Array => string.Join(' ', v.EnumerateArray().Select(ValueText)),
        _ => v.ToString()
    };
    public static JsonElement With(JsonElement row, params (string Key, object? Value)[] extra)
    {
        var fields = row.EnumerateObject().ToDictionary(x => x.Name, x => (object?)x.Value.Clone(), StringComparer.OrdinalIgnoreCase);
        foreach (var pair in extra) fields[pair.Key] = pair.Value;
        return JsonSerializer.SerializeToElement(fields);
    }
    public static long Id(JsonElement row, string key = "id") => long.TryParse(ArtApiClient.Text(row, key), out var id) ? id : 0;
    public static bool ActiveLink(JsonElement row)
    {
        var now = BusinessTime.Now;
        return DateTime.TryParse(ArtApiClient.Text(row, "hieuLucTu"), out var start) && start <= now
            && (!DateTime.TryParse(ArtApiClient.Text(row, "hieuLucDen"), out var end) || end > now);
    }
    public static string MemberKind(long memberId, IEnumerable<JsonElement> links)
    {
        var relations = links.Where(x => Id(x, "thanhVienId") == memberId && ActiveLink(x)).Select(x => ArtApiClient.Text(x, "quanHe")).Distinct().ToArray();
        var self = relations.Contains("TU_BAN_THAN");
        var parent = relations.Any(x => x is "CHA" or "ME");
        var guardian = relations.Contains("GIAM_HO");
        var names = new List<string>();
        if (self) names.Add("Người học");
        if (parent) names.Add("Phụ huynh");
        if (guardian) names.Add("Người giám hộ");
        return names.Count == 0 ? "Chưa liên kết học viên" : string.Join(" / ", names);
    }
    public static string Channel(JsonElement row) => ArtApiClient.Text(row, "kenhDangKy").ToUpperInvariant() switch
    {
        "ONLINE" or "WEB" => "Đăng ký online",
        "OFFLINE" or "WINFORMS" => "Đăng ký tại trung tâm",
        _ => "Chưa được API ghi nhận"
    };
}

public static class ProfileRules
{
    public static void Birth(DateTime birthday)
    {
        if (birthday.Year < 1900 || birthday.Date > BusinessTime.Now.Date)
            throw new InvalidOperationException("Ngày sinh phải từ năm 1900 đến hôm nay.");
    }
    public static void Self(DateTime birthday)
    {
        Birth(birthday);
        if (birthday.Date.AddYears(18) > BusinessTime.Now.Date)
            throw new InvalidOperationException("Người dưới 18 tuổi cần cha, mẹ hoặc người giám hộ đại diện.");
    }
    public static string Required(string value, string label, int max = 150)
    {
        value = value.Trim().Normalize(NormalizationForm.FormC);
        if (string.IsNullOrWhiteSpace(value) || value.Length > max)
            throw new InvalidOperationException($"{label}: cần nhập từ 1 đến {max} ký tự.");
        return value;
    }
    public static void Password(string password, string confirmation)
    {
        if (password != confirmation) throw new InvalidOperationException("Hai lần nhập mật khẩu chưa khớp.");
        if (password.Length is < 8 or > 128 || !password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            throw new InvalidOperationException("Mật khẩu cần 8–128 ký tự, có chữ và số.");
    }
}
