using System;
using System.IO;
using System.Threading;
using System.Xml;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace testit.datacollector
{
    [DataCollectorFriendlyName("TestITDataCollector")]
    [DataCollectorTypeUri("my://testit/datacollector")]
    public class TestITDataCollector : DataCollector
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private DataCollectionLogger logger;
        private DataCollectionEnvironmentContext context;

        public override void Initialize(XmlElement configurationElement, DataCollectionEvents events, DataCollectionSink dataSink, DataCollectionLogger logger, DataCollectionEnvironmentContext environmentContext)
        {
            this.logger = logger;
            this.context = environmentContext;
            events.TestCaseEnd += this.Events_TestCaseEnd;
        }

        private void Events_TestCaseEnd(object sender, TestCaseEndEventArgs e)
        {
            var resultTemplate = new { Name = "", Outcome = "", Message = "", Trace = "" };
            var jsonResult = File.ReadAllText(Path.Combine(Path.GetTempPath(), "lastresult.json"));
            var result = JsonConvert.DeserializeAnonymousType(jsonResult, resultTemplate);
            SendResult(result.Name, "Ready", result.Outcome, result.Message, result.Trace);
        }

        private async void SendResult(string autotestId, string status, string outcome, string message, string stackTrace)
        {
            var testItUrl = Environment.GetEnvironmentVariable("testItUrl");
            var testItUser = Environment.GetEnvironmentVariable("testItUser");
            var testItSecretKey = Environment.GetEnvironmentVariable("testItSecretKey");
            var testRunId = Environment.GetEnvironmentVariable("testRunId");
            var testPlanId = Environment.GetEnvironmentVariable("testPlanId");
            var configurationId = Environment.GetEnvironmentVariable("configurationId");

            var payload = new
            {
                testRunId = testRunId,
                testPlanGlobalId = testPlanId,
                autoTestExternalId = autotestId,
                configurationGlobalId = configurationId,
                status = status,
                outcome = MapOutcome(outcome),
                message = message,
                stackTrace = stackTrace
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            this.logger.LogWarning(this.context.SessionDataCollectionContext, jsonPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            content.Headers.Add("username", testItUser);
            content.Headers.Add("secretKeyBase64", testItSecretKey);

            var apiUri = new Uri(new Uri(testItUrl), String.Format("api/Public/SetAutoTestResult"));

            var response = await httpClient.PostAsync(apiUri, content);
            response.EnsureSuccessStatusCode();
        }

        private string MapOutcome(string outcome)
        {
            Dictionary<string, string> unitTestOutcomes = new Dictionary<string, string>
            {
                {"Failed", "Failed"},
                {"Inconclusive", "Blocked"},
                {"Passed", "Passed"},
                {"InProgress", ""},
                {"Error", "Blocked"},
                {"Timeout", "Blocked"},
                {"Aborted", "Skipped"},
                {"Unknown", "Blocked"},
                {"NotRunnable", "Skipped"},
            };

            return unitTestOutcomes[outcome];
        }
    }
}
