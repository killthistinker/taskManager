using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using ConsoleApp2.Classes;
using RazorEngine;
using RazorEngine.Templating;
using Encoding = System.Text.Encoding;

namespace ConsoleApp2
{
    public class MyHttpServer
    {
        private static string _siteDirectory;
        private static HttpListener _listener;
        private static int _port;
        private static Thread _serverThread;
        private List<Task> _tasks;
        private Task _task;

        public MyHttpServer(string path, int port)
        {
            _tasks = new List<Task>();
            Initial(path, port);
        }

        private void Initial(string path, int port)
        {
            _siteDirectory = path;
            _listener = new HttpListener();
            _serverThread = new Thread(Listen);
            _serverThread.Start();
            _port = port;
        }

        private void Listen()
        {
            _listener.Prefixes.Add($"http://localhost:{_port}/");
            _listener.Start();

            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private void GetPostMethod(HttpListenerContext context)
        {
            var stream = context.Request.InputStream;
            var encodind = context.Request.ContentEncoding;
            var reader = new StreamReader(stream, encodind);
            string data = reader.ReadToEnd();
            if (data.Contains("%"))
            {
                data = HttpUtility.UrlDecode(data, Encoding.UTF8);
            }

            HttpListenerResponse response = context.Response;
            CreateTask(data);
            reader.Close();
            stream.Close();
            response.Redirect($"http://localhost:{_port}/index.html");
            response.Close();
        }
        
        private void Process(HttpListenerContext context)
        {
            string fileName = context.Request.Url.AbsolutePath;
            if (context.Request.QueryString.HasKeys())
            {
                TaskActions(fileName, context);
                return;
            }
            fileName = Path.Combine(_siteDirectory, fileName[1..]);
            if (context.Request.HttpMethod == "POST")
            {
                GetPostMethod(context);
                return;
            }
            byte[] htmlBytes = GetHtmlBytes(fileName);
            if (File.Exists(fileName))
            {
                try
                {
                    using Stream fileStream = new MemoryStream(htmlBytes);
                    context.Response.ContentType = GetContentType(fileName);
                    context.Response.ContentLength64 = fileStream.Length;
                    byte[] buffer = new byte[16 * 1024];
                    int dataLength;
                    do
                    {
                        dataLength = fileStream.Read(buffer, 0, buffer.Length);
                        context.Response.OutputStream.Write(buffer, 0, dataLength);
                    } while (dataLength > 0);

                    context.Response.StatusCode = (int) HttpStatusCode.OK;
                    context.Response.OutputStream.Flush();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    context.Response.OutputStream.Close();
                }
            }
            else
            {
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            }
            context.Response.OutputStream.Close();
        }

        private void TaskActions(string fileName, HttpListenerContext context)
        {
            int.TryParse(context.Request.QueryString["id"], out int id);
            TaskActions taskActions = new TaskActions();
            if (fileName.Contains("excute"))
            {
                taskActions.TaskExecute(ref _tasks, id);
            }

            if (fileName.Contains("delete"))
            {
                taskActions.TaskDelete(ref _tasks, id);
            }

            if (fileName.Contains("info"))
            {
                taskActions.TaskInfo(ref _tasks,ref _task,id);
                context.Response.Redirect($"http://localhost:{_port}/task.html");
                context.Response.Close();
                return;
            }
            TaskManager.WriteTasks(_tasks);
            _tasks = TaskManager.ReadTasks();
            context.Response.Redirect($"http://localhost:{_port}/index.html");
            context.Response.Close();
        }

        private byte[] GetHtmlBytes(string fileName)
        {
            byte[] htmlBytes;
            if (fileName.Contains("html"))
            {
                string content = BuildHtml(fileName);
                htmlBytes = Encoding.UTF8.GetBytes(content);
            }
            else
            {
                htmlBytes = File.ReadAllBytes(fileName);
            }

            return htmlBytes;
        }

        private string GetContentType(string fileName)
        {
            var dictionary = new Dictionary<string, string>
            {
                {".css", "text/css"},
                {".html", "text/html"},
                {".ico", "text/x-icon"},
                {"js", "application/x-javascript"},
                {".json", "application/json"},
                {".jpg", "image/jpg"}
            };

            string contentType = string.Empty;
            
            string fileExtension = Path.GetExtension(fileName);
            Console.WriteLine(fileExtension);
            dictionary.TryGetValue(fileExtension, out contentType);
            return contentType;
        }
        
        private void CreateTask(string data)
        {
            string[] subquery = data.Split("&");
            int id = _tasks.Count + 1;
            string header = subquery[0].Substring(subquery[0].IndexOf("=") + 1);
            string userName = subquery[1].Substring(subquery[1].IndexOf("=") + 1);
            string dateExpiration = subquery[2].Substring(subquery[2].IndexOf("=") + 1);
            string description = subquery[3].Substring(subquery[3].IndexOf("=") + 1);
            string dateCreate = DateTime.Now.ToShortDateString();
            
            _task = new Task(id, header, userName, dateCreate,dateExpiration, description);
            _tasks.Add(_task);
            TaskManager.WriteTasks(_tasks);
            _tasks = TaskManager.ReadTasks();
        }

        private string BuildHtml(string pathToFile)
        {
            string layoutPath = Path.Combine(_siteDirectory, "layout.html");
            var razorService = Engine.Razor;
            if (!razorService.IsTemplateCached("layout",null))
                razorService.AddTemplate("layout", File.ReadAllText(layoutPath));
            if (!razorService.IsTemplateCached(pathToFile, null))
            {
                razorService.AddTemplate(pathToFile, File.ReadAllText(pathToFile));
                razorService.Compile(pathToFile,null);
            }

            string result = razorService.Run(pathToFile, null, new
            {
                Task = _task,
                Tasks = _tasks
            });
            return result;
        }
    }
}