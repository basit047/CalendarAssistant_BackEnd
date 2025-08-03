using CalendarAssistant.Models;
using System.Text.Json;

namespace CalendarAssistant.Helpers
{
    public static class UserSyncFileWriter
    {
        public static void AddNewUserSync(UserSyncModel newUser)
        {
            string filePath = "users.json";
            List<UserSyncModel> users;

            // If file exists, read and deserialize
            if (File.Exists(filePath))
            {
                string existingJson = File.ReadAllText(filePath);
                users = JsonSerializer.Deserialize<List<UserSyncModel>>(existingJson) ?? new List<UserSyncModel>();
            }
            else
            {
                users = new List<UserSyncModel>();
            }

            // Add new user
            users.Add(newUser);

            // Serialize and save updated list
            string updatedJson = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, updatedJson);
        }

        public static bool UpdateUserSync(string? email)
        {
            string filePath = "users.json";

            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found.");
                return false;
            }

            // Read file
            string json = File.ReadAllText(filePath);
            List<UserSyncModel> users = JsonSerializer.Deserialize<List<UserSyncModel>>(json) ?? new List<UserSyncModel>();

            // Find the user
            var user = users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                Console.WriteLine("User not found.");
                return false;
            }

            // Update the user's data
            user.SyncDateTime = DateTime.UtcNow;

            // Serialize and write back to file
            string updatedJson = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, updatedJson);

            Console.WriteLine("User updated successfully.");
            return true;
        }

        public static UserSyncModel GetUserByEmail(string? email)
        {
            string filePath = "users.json";
            if (!File.Exists(filePath))
                return null;

            string json = File.ReadAllText(filePath);
            List<UserSyncModel> users = JsonSerializer.Deserialize<List<UserSyncModel>>(json) ?? new List<UserSyncModel>();

            return users.FirstOrDefault(u => u.Email == email) ?? new UserSyncModel();
        }
    }
}
