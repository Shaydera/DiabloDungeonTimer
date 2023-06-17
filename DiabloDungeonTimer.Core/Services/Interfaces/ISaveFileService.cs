namespace DiabloDungeonTimer.Core.Services.Interfaces;

public interface ISaveFileService
{
    public Task<bool> SaveAsync<T>(T saveData, string? fileName = null);
    public Task<(bool, T?)> TryLoadAsync<T>(string fileName);
}