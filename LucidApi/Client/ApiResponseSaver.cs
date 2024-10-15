using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

public class ApiResponseSaver
{
    public async Task SaveJsonToFileAsync(JToken responseContent, string filePath)
    {
        if (responseContent == null)
        {
            throw new ArgumentNullException(nameof(responseContent), "Response content cannot be null.");
        }

        try
        {
            // Get the directory path and check if it's null
            string directoryPath = Path.GetDirectoryName(filePath) ?? "."; // Use the current directory if no directory is specified

            // Ensure the directory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Convert JToken to a formatted JSON string
            string jsonString = responseContent.ToString(Newtonsoft.Json.Formatting.Indented);

            // Write the JSON string to the specified file asynchronously
            await File.WriteAllTextAsync(filePath, jsonString);

            Console.WriteLine($"Response successfully saved to: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save JSON to file. Error: {ex.Message}");
            throw;
        }
    }

}
