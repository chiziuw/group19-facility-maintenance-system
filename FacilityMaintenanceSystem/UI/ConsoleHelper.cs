using FacilityMaintenanceSystem.Models;

namespace FacilityMaintenanceSystem.UI;

/// <summary>
/// Reusable console formatting, validation prompts, and table rendering helpers.
/// </summary>
public static class ConsoleHelper
{
    public static void PrintHeader(string title)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("======================================================================");
        Console.WriteLine($"  FACILITY MAINTENANCE REQUEST SYSTEM  |  {title}");
        Console.WriteLine("======================================================================");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[SUCCESS] {message}");
        Console.ResetColor();
    }

    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n[ERROR] {message}");
        Console.ResetColor();
    }

    public static void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n[INFO] {message}");
        Console.ResetColor();
    }

    public static void Pause()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }

    public static string ReadRequiredString(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            string? input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                return input.Trim();
            PrintError("This field cannot be empty. Please try again.");
        }
    }

    public static TEnum ReadEnumChoice<TEnum>(string prompt) where TEnum : struct, Enum
    {
        Array values = Enum.GetValues(typeof(TEnum));
        Console.WriteLine($"\n{prompt}:");

        foreach (TEnum val in values)
            Console.WriteLine($"  {Convert.ToInt32(val)}. {val}");

        while (true)
        {
            Console.Write("Select option number: ");
            string? input = Console.ReadLine()?.Trim();

            if (int.TryParse(input, out int numericChoice) && Enum.IsDefined(typeof(TEnum), numericChoice))
                return (TEnum)Enum.ToObject(typeof(TEnum), numericChoice);

            PrintError("Invalid selection. Enter one of the listed numbers.");
        }
    }

    public static TEnum? ReadOptionalEnumChoice<TEnum>(string prompt) where TEnum : struct, Enum
    {
        Array values = Enum.GetValues(typeof(TEnum));
        Console.WriteLine($"\n{prompt} (Press Enter to skip):");

        foreach (TEnum val in values)
            Console.WriteLine($"  {Convert.ToInt32(val)}. {val}");

        while (true)
        {
            Console.Write("Select option number (or Enter for All): ");
            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
                return null;

            if (int.TryParse(input, out int numericChoice) && Enum.IsDefined(typeof(TEnum), numericChoice))
                return (TEnum)Enum.ToObject(typeof(TEnum), numericChoice);

            PrintError("Invalid selection. Enter a valid number or press Enter to skip.");
        }
    }

    public static void PrintRequestsTable(List<MaintenanceRequest> requests)
    {
        if (requests.Count == 0)
        {
            PrintInfo("No maintenance requests match this view.");
            return;
        }

        Console.WriteLine($"{"ID",-10} {"Title",-22} {"Category",-13} {"Priority",-10} {"Status",-12} {"Tech",-10} {"Location",-15}");
        Console.WriteLine(new string('-', 98));

        foreach (MaintenanceRequest req in requests)
        {
            string shortTitle = Truncate(req.Title, 20);
            string shortLoc = Truncate(req.Location, 14);
            string tech = req.AssignedTechnicianUsername ?? "Unassigned";

            Console.Write($"{req.RequestId,-10} {shortTitle,-22} {req.Category,-13} ");
            SetPriorityColor(req.Priority);
            Console.Write($"{req.Priority,-10}");
            Console.ResetColor();
            Console.Write(" ");
            SetStatusColor(req.Status);
            Console.Write($"{req.Status,-12}");
            Console.ResetColor();
            Console.WriteLine($" {tech,-10} {shortLoc,-15}");
        }
    }

    public static void PrintRequestDetails(MaintenanceRequest req)
    {
        Console.WriteLine($"Request ID   : {req.RequestId}");
        Console.WriteLine($"Title        : {req.Title}");
        Console.WriteLine($"Category     : {req.Category}");
        Console.WriteLine($"Priority     : {req.Priority}");
        Console.WriteLine($"Status       : {req.Status}");
        Console.WriteLine($"Location     : {req.Location}");
        Console.WriteLine($"Reported By  : {req.ReporterUsername}");
        Console.WriteLine($"Assigned To  : {req.AssignedTechnicianUsername ?? "Unassigned"}");
        Console.WriteLine($"Created At   : {req.CreatedAt:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"Last Updated : {req.UpdatedAt:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"Description  : {req.Description}");

        Console.WriteLine("\n--- Maintenance Progress Timeline ---");
        foreach (StatusLogEntry log in req.ProgressHistory)
            Console.WriteLine($"  [{log.Timestamp:yyyy-MM-dd HH:mm}] {log.Status,-11} | By: {log.UpdatedBy,-10} | {log.Note}");
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";

    private static void SetPriorityColor(PriorityLevel priority)
    {
        Console.ForegroundColor = priority switch
        {
            PriorityLevel.Critical => ConsoleColor.Red,
            PriorityLevel.High => ConsoleColor.DarkYellow,
            PriorityLevel.Medium => ConsoleColor.Yellow,
            _ => ConsoleColor.Gray
        };
    }

    private static void SetStatusColor(RequestStatus status)
    {
        Console.ForegroundColor = status switch
        {
            RequestStatus.Submitted => ConsoleColor.White,
            RequestStatus.Assigned => ConsoleColor.Cyan,
            RequestStatus.InProgress => ConsoleColor.Blue,
            RequestStatus.OnHold => ConsoleColor.Magenta,
            RequestStatus.Resolved => ConsoleColor.Green,
            RequestStatus.Closed => ConsoleColor.DarkGray,
            _ => ConsoleColor.White
        };
    }
}
