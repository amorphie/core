namespace amorphie.core.Middleware.Logging;
public static class LoggingConstants
{
    private static readonly Dictionary<string, string> optionalHeaders = new Dictionary<string, string>
            {
                { "user_reference", "user_reference" },
                { "xsubject", "X-Subject" },
                { "xdeviceid", Headers.XDeviceId },
                { "xtokenid", "X-Token-Id" },
                { "xcustomer", Headers.XCustomer },
                { "xworkflowname", Headers.XWorkflowName },
                { "xinstanceid", "X-Instance-Id" },
                { "xrequestid", Headers.XRequestId },
                { "xinstallationid", Headers.XInstallationId },
                { "traceparent", "traceparent" },
                { "xapplication", "X-Application" },
                { "clientipaddress", "Clientipaddress" },
                { "clientid", "Clientid" },
                { "jti", "Jti" },
                { "xdeviceplatform", "X-Device-Platform" },
                { "xapplicationversion", "X-Application-Version" },
                { "xdeviceversion", "X-Device-Version" },
                { "xdeviceinfo", "X-Device-Info" },
                { "xlocation", "X-Location" },
            };

    public static Dictionary<string, string> OptionalHeaders
    {
        get
        {
            return optionalHeaders;
        }
    }
    public static class Headers
    {
        public const string XDeviceId = "X-Device-Id";
        public const string XCustomer = "X-Customer";
        public const string XWorkflowName = "X-Workflow-Name";
        public const string XRequestId = "X-Request-Id";
        public const string XInstallationId = "X-Installation-Id";
    }
}

