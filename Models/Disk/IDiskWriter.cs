using System.IO;
using System.Text.Json;

public interface IDiskWriter
{
    public void Write<T>(T obj, string path)
    {
        var opt = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        if (!File.Exists(path))
            File.Create(path).Close();

        File.WriteAllText(path, JsonSerializer.Serialize(obj, opt));
    }
}