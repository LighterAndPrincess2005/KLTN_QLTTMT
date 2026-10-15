using System.Text.Json;

namespace LhlArt.Data;
public class JsonStore
{
    public string PathName { get; }

    public JsonStore(string? path = null)
    {
        if (path != null)
        {
            PathName = path;
            return;
        }

        string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string current = Path.Combine(folder, "LhlArt", "data.json");
        // Giữ đường dẫn dữ liệu của bản cũ để không tách hoặc ghi đè hồ sơ đã có.
        string previous = Path.Combine(folder, "MyThuatDemo", "data.json");
        PathName = File.Exists(current) ? current : File.Exists(previous) ? previous : current;
    }

    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };
    public AppData Load()
    {
        if (!File.Exists(PathName))
            return new AppData();
        return JsonSerializer.Deserialize<AppData>(File.ReadAllText(PathName), Options) ?? throw new Exception("Tệp dữ liệu rỗng.");
    }

    public void Save(AppData data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(PathName)!);
        string temp = PathName + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(data, Options));
        if (File.Exists(PathName))
            File.Replace(temp, PathName, PathName + ".bak");
        else
            File.Move(temp, PathName);
    }
}
