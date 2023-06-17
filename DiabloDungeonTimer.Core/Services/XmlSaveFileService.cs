using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using DiabloDungeonTimer.Core.Models;
using DiabloDungeonTimer.Core.Services.Interfaces;

namespace DiabloDungeonTimer.Core.Services;

public class XmlSaveFileService : ISaveFileService
{
    public async Task<bool> SaveAsync<T>(T saveData, string? fileName = null)
    {
        if (string.IsNullOrEmpty(fileName))
            fileName = Path.Combine(Directory.GetCurrentDirectory(), $"{typeof(T).Name}.xml");
        else
        {
            string? directory = Path.GetDirectoryName(fileName);
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentException("Filename must include a directory", nameof(fileName));
            if (!Directory.Exists(directory))
                throw new ArgumentException("Save directory does not exist.", nameof(fileName));
        }
        
        var serializer = new XmlSerializer(typeof(T));
        using var memoryStream = new MemoryStream();
        serializer.Serialize(memoryStream, saveData);
        memoryStream.Seek(0, SeekOrigin.Begin);
        try
        {
            await File.WriteAllTextAsync(fileName, string.Empty, Encoding.UTF8);
            await using var fileStream =
                new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 0);
            await memoryStream.CopyToAsync(fileStream);
            Debug.Print($"XmlSaveFileService: Saved {typeof(T)} to {fileName}");
            return true;
        }
        catch (Exception e)
        {
            Debug.Fail($"{nameof(SaveAsync)} failed.");
            if (e is IOException or InvalidOperationException)
                return false;
            throw;
        }
    }

    public async Task<(bool, T?)> TryLoadAsync<T>(string fileName)
    {
        if (!File.Exists(fileName))
            return default;

        try
        {
            var serializer = new XmlSerializer(typeof(T));
            await using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            using var streamReader = new StreamReader(fileStream);
            object? loadedData = serializer.Deserialize(streamReader);
            return loadedData is T data ? (true, data) : default;
        }
        catch (Exception e)
        {
            if (e is IOException or InvalidOperationException)
                return default;
            throw;
        }
    }
}