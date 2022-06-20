using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace ConsoleApp2.Classes
{
    public class TaskManager
    {
        private static readonly string Path;

        static TaskManager()
        {
            Path = "../../../employes.json";
        }
        
        public static void WriteTasks(List<Task> tasks)
        {
            if (!File.Exists(Path))
            {
                File.Create(Path).Close();
            }
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin,UnicodeRanges.Cyrillic),
                WriteIndented = true
            };
            string employeeJson = JsonSerializer.Serialize(tasks, options);
            File.WriteAllText(Path, employeeJson);
        }
        
        public static List<Task> ReadTasks()
        {
            List<Task> tasks;
            string tasksJson = File.ReadAllText(Path);
            tasks = JsonSerializer.Deserialize<List<Task>>(tasksJson); 
            return tasks;
        }
    }
}