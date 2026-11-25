using FacilityMaintenanceSystem.Data;
using FacilityMaintenanceSystem.Models;

namespace FacilityMaintenanceSystem.Services;

/// <summary>
/// Core business logic for creating, categorizing, assigning, tracking, and analyzing maintenance requests.
/// </summary>
public class MaintenanceService
{
    private readonly IRepository<MaintenanceRequest> _requestRepository;
    private readonly IRepository<User> _userRepository;

    public MaintenanceService(
        IRepository<MaintenanceRequest> requestRepository,
        IRepository<User> userRepository)
    {
        _requestRepository = requestRepository;
        _userRepository = userRepository;
    }

    public MaintenanceRequest CreateRequest(
        string title,
        string description,
        string location,
        RequestCategory category,
        PriorityLevel priority,
        string reporterUsername)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Request title cannot be empty.");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Fault description cannot be empty.");

        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Facility or equipment location cannot be empty.");

        if (string.IsNullOrWhiteSpace(reporterUsername))
            throw new ArgumentException("Reporter username is required.");

        string nextId = GenerateNextRequestId();
        var request = new MaintenanceRequest
        {
            RequestId = nextId,
            Title = title.Trim(),
            Description = description.Trim(),
            Location = location.Trim(),
            Category = category,
            Priority = priority,
            ReporterUsername = reporterUsername.Trim()
        };

        request.AddProgressEntry(
            RequestStatus.Submitted,
            reporterUsername.Trim(),
            $"Fault reported under category '{category}' with '{priority}' priority.");

        _requestRepository.Add(request);
        _requestRepository.SaveChanges();

        return request;
    }

    public void AssignTechnician(string requestId, string technicianUsername, string adminUsername)
    {
        MaintenanceRequest request = GetRequiredRequest(requestId);

        if (request.Status == RequestStatus.Closed)
            throw new InvalidOperationException("Cannot assign a technician to a closed request.");

        User? techUser = _userRepository.GetById(technicianUsername.Trim());
        if (techUser is not Technician)
            throw new ArgumentException($"User '{technicianUsername}' is not a valid registered technician.");

        request.AssignedTechnicianUsername = techUser.Username;
        request.AddProgressEntry(
            RequestStatus.Assigned,
            adminUsername,
            $"Assigned to technician {techUser.FullName} ({techUser.Username}).");

        _requestRepository.Update(request);
        _requestRepository.SaveChanges();
    }

    public void UpdateRequestStatus(string requestId, RequestStatus newStatus, string actorUsername, string note)
    {
        MaintenanceRequest request = GetRequiredRequest(requestId);

        if (request.Status == RequestStatus.Closed)
            throw new InvalidOperationException("Closed requests are locked and cannot be modified.");

        if (newStatus == RequestStatus.Submitted)
            throw new InvalidOperationException("Cannot revert a request back to Submitted status.");

        if (newStatus == RequestStatus.Closed && request.Status != RequestStatus.Resolved)
            throw new InvalidOperationException("A request must be marked Resolved before it can be Closed.");

        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentException("A progress or resolution note is required for every status update.");

        request.AddProgressEntry(newStatus, actorUsername, note.Trim());

        _requestRepository.Update(request);
        _requestRepository.SaveChanges();
    }

    public MaintenanceRequest? GetRequestById(string requestId)
    {
        if (string.IsNullOrWhiteSpace(requestId)) return null;
        return _requestRepository.GetById(requestId.Trim());
    }

    public List<MaintenanceRequest> GetAllRequests() =>
        _requestRepository.GetAll().OrderByDescending(r => r.CreatedAt).ToList();

    public List<MaintenanceRequest> GetRequestsByReporter(string reporterUsername) =>
        _requestRepository.GetAll()
            .Where(r => r.ReporterUsername.Equals(reporterUsername, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

    public List<MaintenanceRequest> GetRequestsByTechnician(string technicianUsername) =>
        _requestRepository.GetAll()
            .Where(r => r.AssignedTechnicianUsername != null &&
                        r.AssignedTechnicianUsername.Equals(technicianUsername, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.Priority)
            .ThenByDescending(r => r.CreatedAt)
            .ToList();

    public List<MaintenanceRequest> FilterRequests(
        RequestCategory? category = null,
        RequestStatus? status = null,
        PriorityLevel? priority = null)
    {
        IEnumerable<MaintenanceRequest> query = _requestRepository.GetAll();

        if (category.HasValue)
            query = query.Where(r => r.Category == category.Value);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(r => r.Priority == priority.Value);

        return query.OrderByDescending(r => r.CreatedAt).ToList();
    }

    public MaintenanceSummary GetDashboardSummary()
    {
        List<MaintenanceRequest> all = _requestRepository.GetAll();

        return new MaintenanceSummary
        {
            TotalRequests = all.Count,
            SubmittedCount = all.Count(r => r.Status == RequestStatus.Submitted),
            AssignedCount = all.Count(r => r.Status == RequestStatus.Assigned),
            InProgressCount = all.Count(r => r.Status == RequestStatus.InProgress),
            OnHoldCount = all.Count(r => r.Status == RequestStatus.OnHold),
            ResolvedCount = all.Count(r => r.Status == RequestStatus.Resolved),
            ClosedCount = all.Count(r => r.Status == RequestStatus.Closed),
            CountByCategory = all
                .GroupBy(r => r.Category)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    private MaintenanceRequest GetRequiredRequest(string requestId)
    {
        MaintenanceRequest? request = GetRequestById(requestId);
        if (request is null)
            throw new KeyNotFoundException($"Maintenance request '{requestId}' was not found.");
        return request;
    }

    private string GenerateNextRequestId()
    {
        List<MaintenanceRequest> all = _requestRepository.GetAll();
        int maxNumber = 1000;

        foreach (MaintenanceRequest req in all)
        {
            if (req.RequestId.StartsWith("REQ-", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(req.RequestId[4..], out int num) &&
                num > maxNumber)
            {
                maxNumber = num;
            }
        }

        return $"REQ-{maxNumber + 1}";
    }
}
