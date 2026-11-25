using System.Text.Json;
using FacilityMaintenanceSystem.Models;

namespace FacilityMaintenanceSystem.Data;

/// <summary>
/// Persists polymorphic User entities to JSON and seeds default Group 19 accounts on first launch.
/// </summary>
public class JsonUserRepository : IRepository<User>
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private List<User> _users;

    public JsonUserRepository(string filePath)
    {
        _filePath = filePath;
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        _users = LoadFromDisk();

        if (_users.Count == 0)
        {
            SeedDefaultUsers();
            SaveChanges();
        }
    }

    public List<User> GetAll() => _users.ToList();

    public User? GetById(string username) =>
        _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

    public void Add(User entity) => _users.Add(entity);

    public void Update(User entity)
    {
        int index = _users.FindIndex(u => u.Username.Equals(entity.Username, StringComparison.OrdinalIgnoreCase));
        if (index >= 0) _users[index] = entity;
    }

    public void SaveChanges()
    {
        string? dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string json = JsonSerializer.Serialize(_users, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private List<User> LoadFromDisk()
    {
        if (!File.Exists(_filePath)) return new List<User>();

        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<User>>(json, _jsonOptions) ?? new List<User>();
        }
        catch
        {
            return new List<User>();
        }
    }

    private void SeedDefaultUsers()
    {
        _users = new List<User>
        {
            new FacilityAdmin
            {
                Username = "admin",
                Password = "password",
                FullName = "Uwadileke Chizaram",
                OfficeLocation = "Main Engineering Block"
            },
            new Technician
            {
                Username = "tech1",
                Password = "password",
                FullName = "Emmanuel Chinonye",
                Specialization = RequestCategory.Electrical
            },
            new Technician
            {
                Username = "tech2",
                Password = "password",
                FullName = "Campus IT Technician",
                Specialization = RequestCategory.IT_Equipment
            },
            new Reporter
            {
                Username = "user1",
                Password = "password",
                FullName = "Igwagu Chidi",
                Department = "Computer Science"
            }
        };
    }
}
