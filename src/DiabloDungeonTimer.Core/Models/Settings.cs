using System.Xml.Serialization;

namespace DiabloDungeonTimer.Core.Models;

/// <summary>
///     Application Settings Record
/// </summary>
public record Settings
{
    [XmlElement] public string GameDirectory = string.Empty;
    [XmlElement] public bool KeepHistory;
}