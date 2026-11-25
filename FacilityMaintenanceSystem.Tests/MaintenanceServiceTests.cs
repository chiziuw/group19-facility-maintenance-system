using FacilityMaintenanceSystem.Data;
using FacilityMaintenanceSystem.Models;
using FacilityMaintenanceSystem.Services;
using Xunit;

namespace FacilityMaintenanceSystem.Tests;

internal class InMemoryRepository<T> : IRepository<T>
{
    private readonly List<T> _items = new();
    private readonly Func<T, string> _idSelector;

    public InMemoryRepository(Func<T, string> idSelector) => _idSelector = idSelector;

    public List<T> GetAll() => _items.ToList();
    public T? GetById(string id) => _items.FirstOrDefault(x => _idSelector(x).Equals(id, StringComparison.OrdinalIgnoreCase));
    public void Add(T entity) => _items.Add(entity);
    public void Update(T entity)
    {
        int idx = _items.FindIndex(x => _idSelector(x).Equals(_idSelector(entity), StringComparison.OrdinalIgnoreCase));
        if (idx >= 0) _items[idx] = entity;
    }
    public void SaveChanges() { }
}

public class MaintenanceServiceTests
{
    private readonly InMemoryRepository<MaintenanceRequest> _reqRepo;
    private readonly InMemoryRepository<User> _userRepo;
    private readonly MaintenanceService _service;
    private readonly AuthService _authService;

    public MaintenanceServiceTests()
    {
        _reqRepo = new InMemoryRepository<MaintenanceRequest>(r => r.RequestId);
        _userRepo = new InMemoryRepository<User>(u => u.Username);

        _userRepo.Add(new FacilityAdmin { Username = "admin", Password = "password", FullName = "Admin User" });
        _userRepo.Add(new Technician { Username = "tech1", Password = "password", FullName = "Tech One", Specialization = RequestCategory.Electrical });
        _userRepo.Add(new Reporter { Username = "user1", Password = "password", FullName = "Reporter One", Department = "CS" });

        _service = new MaintenanceService(_reqRepo, _userRepo);
        _authService = new AuthService(_userRepo);
    }

    [Fact]
    public void CreateRequest_ValidInputs_GeneratesSequentialIdAndSubmittedStatus()
    {
        var req = _service.CreateRequest("Flickering Light", "Bulb flickers constantly", "Room 101", RequestCategory.Electrical, PriorityLevel.Medium, "user1");

        Assert.Equal("REQ-1001", req.RequestId);
        Assert.Equal(RequestStatus.Submitted, req.Status);
        Assert.Single(req.ProgressHistory);
    }

    [Fact]
    public void CreateRequest_EmptyTitle_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _service.CreateRequest("", "Description", "Room 101", RequestCategory.Plumbing, PriorityLevel.High, "user1"));
    }

    [Fact]
    public void AssignTechnician_ValidTechnician_UpdatesStatusAndLogsAuditTrail()
    {
        var req = _service.CreateRequest("AC Not Cooling", "Blowing warm air", "Lab 3", RequestCategory.HVAC, PriorityLevel.High, "user1");

        _service.AssignTechnician(req.RequestId, "tech1", "admin");

        Assert.Equal("tech1", req.AssignedTechnicianUsername);
        Assert.Equal(RequestStatus.Assigned, req.Status);
        Assert.Equal(2, req.ProgressHistory.Count);
    }

    [Fact]
    public void AssignTechnician_NonTechnicianUser_ThrowsArgumentException()
    {
        var req = _service.CreateRequest("Broken Chair", "Wheel detached", "Library", RequestCategory.Furniture, PriorityLevel.Low, "user1");

        Assert.Throws<ArgumentException>(() => _service.AssignTechnician(req.RequestId, "user1", "admin"));
    }

    [Fact]
    public void UpdateRequestStatus_CloseBeforeResolved_ThrowsInvalidOperationException()
    {
        var req = _service.CreateRequest("Projector Dead", "No power", "Hall A", RequestCategory.IT_Equipment, PriorityLevel.Critical, "user1");

        Assert.Throws<InvalidOperationException>(() =>
            _service.UpdateRequestStatus(req.RequestId, RequestStatus.Closed, "admin", "Attempting premature close"));
    }

    [Fact]
    public void UpdateRequestStatus_FullValidLifecycle_SucceedsAndLocksClosedTicket()
    {
        var req = _service.CreateRequest("Leaking Sink", "Pipe dripping", "Restroom 2", RequestCategory.Plumbing, PriorityLevel.High, "user1");

        _service.AssignTechnician(req.RequestId, "tech1", "admin");
        _service.UpdateRequestStatus(req.RequestId, RequestStatus.InProgress, "tech1", "Inspecting pipe joints");
        _service.UpdateRequestStatus(req.RequestId, RequestStatus.Resolved, "tech1", "Replaced faulty gasket");
        _service.UpdateRequestStatus(req.RequestId, RequestStatus.Closed, "admin", "Verified no leaks");

        Assert.Equal(RequestStatus.Closed, req.Status);
        Assert.Equal(5, req.ProgressHistory.Count);

        Assert.Throws<InvalidOperationException>(() =>
            _service.UpdateRequestStatus(req.RequestId, RequestStatus.InProgress, "tech1", "Reopening illegally"));
    }

    [Fact]
    public void FilterRequests_ByCategoryAndPriority_ReturnsExactMatches()
    {
        _service.CreateRequest("Socket Sparks", "Burning smell", "Lab 1", RequestCategory.Electrical, PriorityLevel.Critical, "user1");
        _service.CreateRequest("Loose Door", "Hinge broken", "Lab 2", RequestCategory.Structural, PriorityLevel.Low, "user1");

        var filtered = _service.FilterRequests(category: RequestCategory.Electrical, priority: PriorityLevel.Critical);

        Assert.Single(filtered);
        Assert.Equal("Socket Sparks", filtered[0].Title);
    }

    [Fact]
    public void RegisterReporter_DuplicateUsername_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _authService.RegisterReporter("user1", "secret123", "Duplicate User", "Engineering"));
    }

    [Fact]
    public void JsonRequestRepository_IntegrationTest_PersistsAndReloadsFromDisk()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"fmrs_test_{Guid.NewGuid()}.json");
        try
        {
            var repo1 = new JsonRequestRepository(tempFile);
            var svc1 = new MaintenanceService(repo1, _userRepo);
            var created = svc1.CreateRequest("Network Switch Down", "No link lights", "Server Room", RequestCategory.IT_Equipment, PriorityLevel.Critical, "user1");

            var repo2 = new JsonRequestRepository(tempFile);
            var reloaded = repo2.GetById(created.RequestId);

            Assert.NotNull(reloaded);
            Assert.Equal("Network Switch Down", reloaded!.Title);
            Assert.Equal(RequestCategory.IT_Equipment, reloaded.Category);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
