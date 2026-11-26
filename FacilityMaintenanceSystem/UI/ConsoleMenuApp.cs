using FacilityMaintenanceSystem.Models;
using FacilityMaintenanceSystem.Services;

namespace FacilityMaintenanceSystem.UI;

/// <summary>
/// Presentation controller managing role-specific interactive workflows.
/// </summary>
public class ConsoleMenuApp
{
    private readonly AuthService _authService;
    private readonly MaintenanceService _maintenanceService;
    private User? _currentUser;

    public ConsoleMenuApp(AuthService authService, MaintenanceService maintenanceService)
    {
        _authService = authService;
        _maintenanceService = maintenanceService;
    }

    public void Run()
    {
        bool running = true;
        while (running)
        {
            if (_currentUser is null)
            {
                running = ShowAuthMenu();
            }
            else
            {
                switch (_currentUser.Role)
                {
                    case UserRole.Reporter:
                        ShowReporterMenu();
                        break;
                    case UserRole.Technician:
                        ShowTechnicianMenu();
                        break;
                    case UserRole.Admin:
                        ShowAdminMenu();
                        break;
                }
            }
        }

        Console.WriteLine("\nGoodbye! Exiting Facility Maintenance Request System.");
    }

    private bool ShowAuthMenu()
    {
        ConsoleHelper.PrintHeader("WELCOME (GROUP 19)");
        Console.WriteLine("Demo Accounts (password for all: 'password'):");
        Console.WriteLine("  - Admin      : admin");
        Console.WriteLine("  - Technician : tech1 / tech2");
        Console.WriteLine("  - Reporter   : user1\n");
        Console.WriteLine("1. Login");
        Console.WriteLine("2. Register New Reporter Account");
        Console.WriteLine("0. Exit");
        Console.Write("\nChoose an option: ");

        string? choice = Console.ReadLine()?.Trim();
        switch (choice)
        {
            case "1":
                HandleLogin();
                return true;
            case "2":
                HandleRegister();
                return true;
            case "0":
                return false;
            default:
                ConsoleHelper.PrintError("Invalid option. Please enter 1, 2, or 0.");
                ConsoleHelper.Pause();
                return true;
        }
    }

    private void HandleLogin()
    {
        ConsoleHelper.PrintHeader("USER LOGIN");
        string username = ConsoleHelper.ReadRequiredString("Username");
        string password = ConsoleHelper.ReadRequiredString("Password");

        User? user = _authService.Login(username, password);
        if (user is null)
        {
            ConsoleHelper.PrintError("Invalid username or password.");
            ConsoleHelper.Pause();
            return;
        }

        _currentUser = user;
        ConsoleHelper.PrintSuccess($"Welcome, {user.FullName} — {user.GetRoleSummary()}");
        ConsoleHelper.Pause();
    }

    private void HandleRegister()
    {
        ConsoleHelper.PrintHeader("REGISTER REPORTER");
        try
        {
            string username = ConsoleHelper.ReadRequiredString("Choose Username (no spaces)");
            string password = ConsoleHelper.ReadRequiredString("Choose Password (min 4 chars)");
            string fullName = ConsoleHelper.ReadRequiredString("Full Name");
            string dept = ConsoleHelper.ReadRequiredString("Department / Unit");

            Reporter created = _authService.RegisterReporter(username, password, fullName, dept);
            ConsoleHelper.PrintSuccess($"Account '{created.Username}' created! You can now log in.");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError(ex.Message);
        }
        ConsoleHelper.Pause();
    }

    private void ShowReporterMenu()
    {
        ConsoleHelper.PrintHeader($"REPORTER DASHBOARD ({_currentUser!.FullName})");
        Console.WriteLine("1. Submit New Maintenance Request");
        Console.WriteLine("2. View My Submitted Requests");
        Console.WriteLine("3. Track Specific Request Progress");
        Console.WriteLine("0. Logout");
        Console.Write("\nChoose an option: ");

        switch (Console.ReadLine()?.Trim())
        {
            case "1":
                HandleSubmitRequest();
                break;
            case "2":
                ConsoleHelper.PrintHeader("MY MAINTENANCE REQUESTS");
                ConsoleHelper.PrintRequestsTable(_maintenanceService.GetRequestsByReporter(_currentUser.Username));
                ConsoleHelper.Pause();
                break;
            case "3":
                HandleTrackRequest(enforceOwnerOnly: true);
                break;
            case "0":
                _currentUser = null;
                break;
            default:
                ConsoleHelper.PrintError("Invalid menu selection.");
                ConsoleHelper.Pause();
                break;
        }
    }

    private void ShowTechnicianMenu()
    {
        ConsoleHelper.PrintHeader($"TECHNICIAN DASHBOARD ({_currentUser!.FullName})");
        Console.WriteLine("1. View My Assigned Maintenance Requests");
        Console.WriteLine("2. Update Request Maintenance Progress");
        Console.WriteLine("3. View Request Details & Timeline");
        Console.WriteLine("0. Logout");
        Console.Write("\nChoose an option: ");

        switch (Console.ReadLine()?.Trim())
        {
            case "1":
                ConsoleHelper.PrintHeader("ASSIGNED REQUESTS");
                ConsoleHelper.PrintRequestsTable(_maintenanceService.GetRequestsByTechnician(_currentUser.Username));
                ConsoleHelper.Pause();
                break;
            case "2":
                HandleTechnicianStatusUpdate();
                break;
            case "3":
                HandleTrackRequest(enforceOwnerOnly: false);
                break;
            case "0":
                _currentUser = null;
                break;
            default:
                ConsoleHelper.PrintError("Invalid menu selection.");
                ConsoleHelper.Pause();
                break;
        }
    }

    private void ShowAdminMenu()
    {
        ConsoleHelper.PrintHeader($"ADMIN DASHBOARD ({_currentUser!.FullName})");
        Console.WriteLine("1. View All Maintenance Requests");
        Console.WriteLine("2. Filter Requests (Category / Status / Priority)");
        Console.WriteLine("3. Assign Technician to Request");
        Console.WriteLine("4. Update Request Status / Close Resolved Ticket");
        Console.WriteLine("5. Track Request Details & Audit Log");
        Console.WriteLine("6. View Maintenance Analytics Summary");
        Console.WriteLine("0. Logout");
        Console.Write("\nChoose an option: ");

        switch (Console.ReadLine()?.Trim())
        {
            case "1":
                ConsoleHelper.PrintHeader("ALL MAINTENANCE REQUESTS");
                ConsoleHelper.PrintRequestsTable(_maintenanceService.GetAllRequests());
                ConsoleHelper.Pause();
                break;
            case "2":
                HandleFilterRequests();
                break;
            case "3":
                HandleAssignTechnician();
                break;
            case "4":
                HandleAdminStatusUpdate();
                break;
            case "5":
                HandleTrackRequest(enforceOwnerOnly: false);
                break;
            case "6":
                HandleShowAnalytics();
                break;
            case "0":
                _currentUser = null;
                break;
            default:
                ConsoleHelper.PrintError("Invalid menu selection.");
                ConsoleHelper.Pause();
                break;
        }
    }

    private void HandleSubmitRequest()
    {
        ConsoleHelper.PrintHeader("SUBMIT MAINTENANCE REQUEST");
        try
        {
            string title = ConsoleHelper.ReadRequiredString("Short Title (e.g., Broken Lab AC)");
            string location = ConsoleHelper.ReadRequiredString("Location (e.g., Block B, Room 204)");
            string description = ConsoleHelper.ReadRequiredString("Detailed Fault Description");
            RequestCategory category = ConsoleHelper.ReadEnumChoice<RequestCategory>("Select Fault Category");
            PriorityLevel priority = ConsoleHelper.ReadEnumChoice<PriorityLevel>("Select Priority Level");

            MaintenanceRequest created = _maintenanceService.CreateRequest(
                title, description, location, category, priority, _currentUser!.Username);

            ConsoleHelper.PrintSuccess($"Maintenance request submitted! Tracking ID: {created.RequestId}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError(ex.Message);
        }
        ConsoleHelper.Pause();
    }

    private void HandleTrackRequest(bool enforceOwnerOnly)
    {
        ConsoleHelper.PrintHeader("TRACK REQUEST PROGRESS");
        string reqId = ConsoleHelper.ReadRequiredString("Enter Request ID (e.g., REQ-1001)");

        MaintenanceRequest? req = _maintenanceService.GetRequestById(reqId);
        if (req is null)
        {
            ConsoleHelper.PrintError($"Request '{reqId}' not found.");
            ConsoleHelper.Pause();
            return;
        }

        if (enforceOwnerOnly && !req.ReporterUsername.Equals(_currentUser!.Username, StringComparison.OrdinalIgnoreCase))
        {
            ConsoleHelper.PrintError("Access denied. You can only track requests you submitted.");
            ConsoleHelper.Pause();
            return;
        }

        Console.WriteLine();
        ConsoleHelper.PrintRequestDetails(req);
        ConsoleHelper.Pause();
    }

    private void HandleFilterRequests()
    {
        ConsoleHelper.PrintHeader("FILTER MAINTENANCE REQUESTS");
        RequestCategory? category = ConsoleHelper.ReadOptionalEnumChoice<RequestCategory>("Filter by Category");
        RequestStatus? status = ConsoleHelper.ReadOptionalEnumChoice<RequestStatus>("Filter by Status");
        PriorityLevel? priority = ConsoleHelper.ReadOptionalEnumChoice<PriorityLevel>("Filter by Priority");

        List<MaintenanceRequest> results = _maintenanceService.FilterRequests(category, status, priority);
        Console.WriteLine();
        ConsoleHelper.PrintRequestsTable(results);
        ConsoleHelper.Pause();
    }

    private void HandleAssignTechnician()
    {
        ConsoleHelper.PrintHeader("ASSIGN TECHNICIAN");
        ConsoleHelper.PrintRequestsTable(_maintenanceService.GetAllRequests());
        Console.WriteLine("\nAvailable Technicians:");
        foreach (Technician tech in _authService.GetAllTechnicians())
            Console.WriteLine($"  - {tech.Username,-10} | {tech.FullName,-20} | Specialty: {tech.Specialization}");

        Console.WriteLine();
        try
        {
            string reqId = ConsoleHelper.ReadRequiredString("Enter Request ID to Assign");
            string techUsername = ConsoleHelper.ReadRequiredString("Enter Technician Username");

            _maintenanceService.AssignTechnician(reqId, techUsername, _currentUser!.Username);
            ConsoleHelper.PrintSuccess($"Request {reqId} successfully assigned to {techUsername}.");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError(ex.Message);
        }
        ConsoleHelper.Pause();
    }

    private void HandleTechnicianStatusUpdate()
    {
        ConsoleHelper.PrintHeader("UPDATE MAINTENANCE PROGRESS");
        List<MaintenanceRequest> assigned = _maintenanceService.GetRequestsByTechnician(_currentUser!.Username);
        ConsoleHelper.PrintRequestsTable(assigned);

        if (assigned.Count == 0)
        {
            ConsoleHelper.Pause();
            return;
        }

        Console.WriteLine();
        try
        {
            string reqId = ConsoleHelper.ReadRequiredString("Enter Request ID");
            MaintenanceRequest? req = _maintenanceService.GetRequestById(reqId);

            if (req is null || !string.Equals(req.AssignedTechnicianUsername, _currentUser.Username, StringComparison.OrdinalIgnoreCase))
            {
                ConsoleHelper.PrintError("Request not found or not assigned to you.");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine("\nSelect New Status:");
            Console.WriteLine("  3. InProgress");
            Console.WriteLine("  4. OnHold");
            Console.WriteLine("  5. Resolved");
            Console.Write("Choice: ");
            string? choice = Console.ReadLine()?.Trim();

            RequestStatus newStatus = choice switch
            {
                "3" => RequestStatus.InProgress,
                "4" => RequestStatus.OnHold,
                "5" => RequestStatus.Resolved,
                _ => throw new ArgumentException("Invalid status selection for technician.")
            };

            string note = ConsoleHelper.ReadRequiredString("Progress / Resolution Note");
            _maintenanceService.UpdateRequestStatus(reqId, newStatus, _currentUser.Username, note);
            ConsoleHelper.PrintSuccess($"Request {reqId} updated to {newStatus}.");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError(ex.Message);
        }
        ConsoleHelper.Pause();
    }

    private void HandleAdminStatusUpdate()
    {
        ConsoleHelper.PrintHeader("ADMIN STATUS UPDATE / CLOSE TICKET");
        ConsoleHelper.PrintRequestsTable(_maintenanceService.GetAllRequests());
        Console.WriteLine();

        try
        {
            string reqId = ConsoleHelper.ReadRequiredString("Enter Request ID");
            RequestStatus newStatus = ConsoleHelper.ReadEnumChoice<RequestStatus>("Select Target Status");
            string note = ConsoleHelper.ReadRequiredString("Status Update Note");

            _maintenanceService.UpdateRequestStatus(reqId, newStatus, _currentUser!.Username, note);
            ConsoleHelper.PrintSuccess($"Request {reqId} transitioned to {newStatus}.");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError(ex.Message);
        }
        ConsoleHelper.Pause();
    }

    private void HandleShowAnalytics()
    {
        ConsoleHelper.PrintHeader("MAINTENANCE ANALYTICS SUMMARY");
        MaintenanceSummary summary = _maintenanceService.GetDashboardSummary();

        Console.WriteLine($"Total Requests : {summary.TotalRequests}");
        Console.WriteLine($"  - Submitted  : {summary.SubmittedCount}");
        Console.WriteLine($"  - Assigned   : {summary.AssignedCount}");
        Console.WriteLine($"  - InProgress : {summary.InProgressCount}");
        Console.WriteLine($"  - OnHold     : {summary.OnHoldCount}");
        Console.WriteLine($"  - Resolved   : {summary.ResolvedCount}");
        Console.WriteLine($"  - Closed     : {summary.ClosedCount}");

        Console.WriteLine("\nBreakdown by Category:");
        if (summary.CountByCategory.Count == 0)
            Console.WriteLine("  No requests recorded yet.");
        else
            foreach (var kvp in summary.CountByCategory.OrderByDescending(x => x.Value))
                Console.WriteLine($"  - {kvp.Key,-15}: {kvp.Value}");

        ConsoleHelper.Pause();
    }
}
