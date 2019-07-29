using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace testit.tools
{
    class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();

        private static string testItUrl;
        private static string testItUser;
        private static string testItSecretKey;
        private static string testRunId;
        private static string testProjectId;

        static void Main(string[] args)
        {
            testItUrl = Environment.GetEnvironmentVariable("testItUrl");
            testItUser = Environment.GetEnvironmentVariable("testItUser");
            testItSecretKey = Environment.GetEnvironmentVariable("testItSecretKey");
            testRunId = Environment.GetEnvironmentVariable("testRunId");
            testProjectId = Environment.GetEnvironmentVariable("testProjectId");

            Task task = null;
            switch (args[0])
            {
                case "get":
                    task = GetTestRun(args[1]);
                    break;

                case "start":
                    task = StartRun();
                    break;

                case "complete":
                    task = CompleteRun();
                    break;

                case "stop":
                    task = StopRun();
                    break;

                default:
                    throw new ArgumentException(String.Format("Can't execute '{0}' command.", args[0]));
            }

            task.GetAwaiter().GetResult();
        }

        private async static Task GetTestRun(string outputPath)
        {
            var path = String.Format("/api/Public/GetTestRuns/{0}", testProjectId);
            var content = await Send(path, HttpMethod.Get, null);
            IEnumerable<object> runs = JsonConvert.DeserializeObject<IEnumerable<object>>(content);

            List<object> autoPoints = new List<object>();
            foreach (dynamic run in runs)
            {
                if (run.testRunId == testRunId)
                {
                    IEnumerable<object> points = run.testPoints;
                    foreach (dynamic point in points)
                    {
                        var configuration = point.configurationGlobalId;
                        IEnumerable<object> autotests = point.autoTestIds;
                        foreach (var autotest in autotests)
                        {
                            var autoPoint = new { autotest = autotest, configuration = configuration };
                            autoPoints.Add(autoPoint);
                        }
                    }
                }
            }

            File.WriteAllText(Path.Combine(outputPath, "points.json"), JsonConvert.SerializeObject(autoPoints.ToArray()));
        }

        private async static Task StartRun()
        {
            var path = String.Format("/api/Public/StartTestRun/{0}", testRunId);
            await Send(path, HttpMethod.Post, null);
        }

        private async static Task StopRun()
        {
            var path = String.Format("/api/Public/StopTestRun/{0}", testRunId);
            await Send(path, HttpMethod.Post, null);
        }

        private async static Task CompleteRun()
        {
            var path = String.Format("/api/Public/CompleteTestRun/{0}", testRunId);
            await Send(path, HttpMethod.Post, null);
        }

        private static async Task<string> Send(string relativePath, HttpMethod method, HttpContent content)
        {
            var apiUri = new Uri(new Uri(testItUrl), relativePath);
            using (var requestMessage = new HttpRequestMessage(method, apiUri))
            {

                requestMessage.Headers.Add("username", testItUser);
                requestMessage.Headers.Add("secretKeyBase64", testItSecretKey);
                httpClient.Timeout = TimeSpan.FromSeconds(5);

                if (method == HttpMethod.Post && content != null)
                {
                    requestMessage.Content = content;
                }

                var response = await httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                return jsonString;
            }
        }
    }
}
