using System.Text.Json.Serialization;

namespace FacilityMaintenanceSystem.Models;

/// <summary>
/// Abstract base class representing any authenticated system user.
/// Uses JSON polymorphic attributes so derived roles serialize and deserialize cleanly.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Reporter), "reporter")]
[JsonDerivedType(typeof(Technician), "technician")]
[JsonDerivedType(typeof(FacilityAdmin), "admin")]
public abstract class User
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    [JsonIgnore]
    public abstract UserRole Role { get; }

    public abstract string GetRoleSummary();
}

public class Reporter : User
{
    public string Department { get; set; } = "General";

    public override UserRole Role => UserRole.Reporter;

    public override string GetRoleSummary() => $"Reporter ({Department})";
}

public class Technician : User
{
    public RequestCategory Specialization { get; set; } = RequestCategory.Other;

    public override UserRole Role => UserRole.Technician;

    public override string GetRoleSummary() => $"Technician [Specialty: {Specialization}]";
}

public class FacilityAdmin : User
{
    public string OfficeLocation { get; set; } = "Facilities HQ";

    public override UserRole Role => UserRole.Admin;

    public override string GetRoleSummary() => $"Facility Admin ({OfficeLocation})";
}
