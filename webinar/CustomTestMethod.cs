using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.IO;

namespace webinar
{
    public class CustomTestMethod : TestMethodAttribute
    {
        public override TestResult[] Execute(ITestMethod testMethod)
        {
            var results = base.Execute(testMethod);
            foreach (var result in results)
            {
                var fullyQualifiedName = String.Format("{0}.{1}", testMethod.TestClassName, testMethod.TestMethodName);
                var currResult = new {
                    Name = fullyQualifiedName,
                    Outcome = result.Outcome.ToString(),
                    Message = result.TestFailureException == null ? "" : result.TestFailureException.Message,
                    Trace = result.TestFailureException == null ? "" : result.TestFailureException.InnerException.StackTrace
                };
                var serializedResult = JsonConvert.SerializeObject(currResult);
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "lastresult.json"), serializedResult);
            }
            return results;
        }
    }
}
