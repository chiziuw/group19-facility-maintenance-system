using FacilityMaintenanceSystem.Data;
using FacilityMaintenanceSystem.Services;
using FacilityMaintenanceSystem.UI;

namespace FacilityMaintenanceSystem;

public class Program
{
    public static void Main(string[] args)
    {
        string dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
        var userRepository = new JsonUserRepository(Path.Combine(dataDirectory, "users.json"));
        var requestRepository = new JsonRequestRepository(Path.Combine(dataDirectory, "requests.json"));

        var authService = new AuthService(userRepository);
        var maintenanceService = new MaintenanceService(requestRepository, userRepository);

        var app = new ConsoleMenuApp(authService, maintenanceService);
        app.Run();
    }
}
