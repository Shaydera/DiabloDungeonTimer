using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using DiabloDungeonTimer.Core.Models;

namespace DiabloDungeonTimer.Core.Services;

public class XmlSettingsService : SettingsServiceBase
{
    private static readonly string FileName = AppDomain.CurrentDomain.BaseDirectory + @"\settings.xml";

    private XmlSettingsService(Settings settings) : base(settings)
    {
    }

    public override async Task<bool> SaveAsync()
    {
        var serializer = new XmlSerializer(typeof(Settings));
        using var memoryStream = new MemoryStream();
        serializer.Serialize(memoryStream, Settings);
        memoryStream.Seek(0, SeekOrigin.Begin);
        try
        {
            await File.WriteAllTextAsync(FileName, string.Empty);
            await using var fileStream =
                new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 0);
            await memoryStream.CopyToAsync(fileStream);
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

    public override async Task ReloadAsync()
    {
        Settings? loadedSettings = await LoadAsync();
        if (loadedSettings != null)
            Settings = loadedSettings;
        if (!GameDirectoryValid()) Settings.GameDirectory = string.Empty;
    }

    public static async Task<XmlSettingsService> BuildSettingsServiceAsync()
    {
        var service = new XmlSettingsService(new Settings());
        await service.ReloadAsync();
        return service;
    }

    private static async Task<Settings?> LoadAsync()
    {
        if (!File.Exists(FileName))
            return null;

        try
        {
            var serializer = new XmlSerializer(typeof(Settings));
            await using var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            using var streamReader = new StreamReader(fileStream);
            var loadedSettings = serializer.Deserialize(streamReader) as Settings;
            return loadedSettings;
        }
        catch (Exception e)
        {
            if (e is IOException or InvalidOperationException)
                return new Settings();
            throw;
        }
    }
}