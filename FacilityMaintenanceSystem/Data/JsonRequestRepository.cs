using System.Text.Json;
using FacilityMaintenanceSystem.Models;

namespace FacilityMaintenanceSystem.Data;

/// <summary>
/// Persists MaintenanceRequest records to a local JSON file.
/// </summary>
public class JsonRequestRepository : IRepository<MaintenanceRequest>
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private List<MaintenanceRequest> _requests;

    public JsonRequestRepository(string filePath)
    {
        _filePath = filePath;
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        _requests = LoadFromDisk();
    }

    public List<MaintenanceRequest> GetAll() => _requests.ToList();

    public MaintenanceRequest? GetById(string id) =>
        _requests.FirstOrDefault(r => r.RequestId.Equals(id, StringComparison.OrdinalIgnoreCase));

    public void Add(MaintenanceRequest entity) => _requests.Add(entity);

    public void Update(MaintenanceRequest entity)
    {
        int index = _requests.FindIndex(r => r.RequestId.Equals(entity.RequestId, StringComparison.OrdinalIgnoreCase));
        if (index >= 0) _requests[index] = entity;
    }

    public void SaveChanges()
    {
        string? dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string json = JsonSerializer.Serialize(_requests, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private List<MaintenanceRequest> LoadFromDisk()
    {
        if (!File.Exists(_filePath)) return new List<MaintenanceRequest>();

        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<MaintenanceRequest>>(json, _jsonOptions)
                   ?? new List<MaintenanceRequest>();
        }
        catch
        {
            return new List<MaintenanceRequest>();
        }
    }
}
