namespace FacilityMaintenanceSystem.Models;

/// <summary>
/// Represents a single timestamped progress entry in a maintenance request's audit trail.
/// </summary>
public class StatusLogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public RequestStatus Status { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

/// <summary>
/// Core domain entity representing a reported equipment or facility fault.
/// </summary>
public class MaintenanceRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public RequestCategory Category { get; set; }
    public PriorityLevel Priority { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Submitted;
    public string ReporterUsername { get; set; } = string.Empty;
    public string? AssignedTechnicianUsername { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public List<StatusLogEntry> ProgressHistory { get; set; } = new();

    public void AddProgressEntry(RequestStatus newStatus, string updatedBy, string note)
    {
        Status = newStatus;
        UpdatedAt = DateTime.Now;
        ProgressHistory.Add(new StatusLogEntry
        {
            Timestamp = UpdatedAt,
            Status = newStatus,
            UpdatedBy = updatedBy,
            Note = note
        });
    }
}

public class MaintenanceSummary
{
    public int TotalRequests { get; set; }
    public int SubmittedCount { get; set; }
    public int AssignedCount { get; set; }
    public int InProgressCount { get; set; }
    public int OnHoldCount { get; set; }
    public int ResolvedCount { get; set; }
    public int ClosedCount { get; set; }
    public Dictionary<RequestCategory, int> CountByCategory { get; set; } = new();
}
