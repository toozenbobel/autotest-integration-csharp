using System;
using System.Linq;
using System.Reflection;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace AutotestsSync
{
    public class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            var testModulePaths = args;
            var testItUrl = Environment.GetEnvironmentVariable("testItUrl");
            var testItUser = Environment.GetEnvironmentVariable("testItUser");
            var testItSecretKey = Environment.GetEnvironmentVariable("testItSecretKey");

            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var testClassAttributeName = config["testClassAttributeName"];
            var testCaseIdAttributeName = config["testCaseIdAttributeName"];
            var testCaseIdAttributeProperty = config["testCaseIdAttributeProperty"];
            var sourceUrl = config["sourceUrl"];

            foreach (string testModulePath in testModulePaths)
            {
                Assembly a = Assembly.LoadFrom(testModulePath);
                var types = a.GetTypes().Where(t => t.GetCustomAttributes().Any(x => x.GetType().FullName.Contains(testClassAttributeName)));
                foreach (Type t in types)
                {
                    var methods = t.GetMethods().Where(m => m.GetCustomAttributes().Any(x => x.GetType().FullName.Contains(testCaseIdAttributeName)));
                    foreach (MethodInfo m in methods)
                    {
                        Attribute testCaseIdAttribute = m.GetCustomAttributes().Single(x => x.GetType().FullName.Contains(testCaseIdAttributeName));
                        var testCaseId = testCaseIdAttribute.GetType().GetProperty(testCaseIdAttributeProperty).GetValue(testCaseIdAttribute).ToString();

                        var fullyQualifiedName = String.Format("{0}.{1}", t.FullName, m.Name);
                        var apiUri = new Uri(new Uri(testItUrl), String.Format("api/Public/AddAutoTestToTestCase/{0}", testCaseId));
                        var payload = new
                        {
                            autotestExternalId = fullyQualifiedName,
                            testLinkInRepository = new Uri(new Uri(sourceUrl), String.Format("{0}.cs", t.FullName.Replace('.', '/')))
                        };
                        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                        content.Headers.Add("username", testItUser);
                        content.Headers.Add("secretKeyBase64", testItSecretKey);

                        var response = await httpClient.PostAsync(apiUri, content);
                        if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Conflict)
                        {
                            throw new HttpRequestException(string.Format("{0} ({1}) - Can't link \"{2}\" to test case \"{3}\".", (int)response.StatusCode, response.ReasonPhrase, fullyQualifiedName, testCaseId));
                        }
                    }
                }
            }
        }
    }
}
