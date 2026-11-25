using FacilityMaintenanceSystem.Data;
using FacilityMaintenanceSystem.Models;

namespace FacilityMaintenanceSystem.Services;

/// <summary>
/// Handles authentication, account registration, and technician directory lookups.
/// </summary>
public class AuthService
{
    private readonly IRepository<User> _userRepository;

    public AuthService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public User? Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        User? user = _userRepository.GetById(username.Trim());
        if (user is null || user.Password != password)
            return null;

        return user;
    }

    public Reporter RegisterReporter(string username, string password, string fullName, string department)
    {
        username = username.Trim();
        fullName = fullName.Trim();
        department = department.Trim();

        if (string.IsNullOrWhiteSpace(username) || username.Contains(' '))
            throw new ArgumentException("Username is required and cannot contain spaces.");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
            throw new ArgumentException("Password must be at least 4 characters long.");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.");

        if (_userRepository.GetById(username) is not null)
            throw new InvalidOperationException($"Username '{username}' is already taken.");

        var reporter = new Reporter
        {
            Username = username,
            Password = password,
            FullName = fullName,
            Department = string.IsNullOrWhiteSpace(department) ? "General" : department
        };

        _userRepository.Add(reporter);
        _userRepository.SaveChanges();
        return reporter;
    }

    public List<Technician> GetAllTechnicians() =>
        _userRepository.GetAll().OfType<Technician>().ToList();
}
