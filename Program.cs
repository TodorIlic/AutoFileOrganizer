using System;
using System.IO;

namespace AutoFileOrganizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Automated File Organizer ===");

            // Dynamically grab the folder where this application is running
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Set the watch directory to a 'Sorting' folder right next to the executable
            string watchDirectory = Path.Combine(appDirectory, "Sorting");

            // Create the directory if it doesn't exist
            Directory.CreateDirectory(watchDirectory);

            FileOrganizer organizer = new FileOrganizer(watchDirectory);

            organizer.OnLogMessage = message =>
            {
                Console.WriteLine(message);
            };

            organizer.StartWatching();

            Console.WriteLine($"Drop files to be organized into: {watchDirectory}");
            Console.WriteLine("Press 'q' to quit.");

            // Keep the app running until the user presses 'q'
            while (Console.Read() != 'q') ;

            organizer.StopWatching();
        }
    }
}
