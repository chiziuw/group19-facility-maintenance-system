namespace FacilityMaintenanceSystem.Models;

/// <summary>
/// Defines role-based access levels across the system.
/// </summary>
public enum UserRole
{
    Reporter,
    Technician,
    Admin
}

/// <summary>
/// Categories for classifying faulty equipment or facility issues.
/// </summary>
public enum RequestCategory
{
    Electrical = 1,
    Plumbing = 2,
    HVAC = 3,
    IT_Equipment = 4,
    Structural = 5,
    Furniture = 6,
    Other = 7
}

/// <summary>
/// Severity levels guiding maintenance triage.
/// </summary>
public enum PriorityLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Lifecycle stages of a maintenance request.
/// </summary>
public enum RequestStatus
{
    Submitted = 1,
    Assigned = 2,
    InProgress = 3,
    OnHold = 4,
    Resolved = 5,
    Closed = 6
}
