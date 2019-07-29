using System;

namespace webinar
{
    public class TestCaseIdAttribute : Attribute
    {
        public string TestCaseId { get; private set; }

        public TestCaseIdAttribute(ulong testCaseId) : this(testCaseId.ToString())
        {
        }

        public TestCaseIdAttribute(string testCaseId)
        {
            TestCaseId = testCaseId;
        }
    }
}
