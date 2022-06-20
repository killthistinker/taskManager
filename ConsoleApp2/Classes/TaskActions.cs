using System.Collections.Generic;

namespace ConsoleApp2.Classes
{
    public class TaskActions
    {
        public void TaskExecute(ref List<Task> tasks, int id)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i].Id == id)
                {
                    tasks[i].State = "done";
                }
            }
        }
        
        public void TaskDelete(ref List<Task> tasks, int id)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i].Id == id)
                {
                    tasks.Remove(tasks[i]);
                }
            }
        }

        public void TaskInfo(ref List<Task> tasks, ref Task task, int id)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i].Id == id)
                {
                    task = tasks[i];
                }
            }
        }
    }
}