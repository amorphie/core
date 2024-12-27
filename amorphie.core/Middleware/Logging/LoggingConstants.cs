namespace amorphie.core.Middleware.Logging;
public static class LoggingConstants
{
    private static readonly Dictionary<string, string> optionalHeaders = new Dictionary<string, string>
            {
                { "user_reference", "user_reference" },
                { "xsubject", "X-Subject" },
                { "xdeviceid", "X-Device-Id" },
                { "xtokenid", "X-Token-Id" },
                { "xcustomer", "X-Customer" },
                { "xworkflowname", "X-Workflow-Name" },
                { "xinstanceid", "X-Instance-Id" },
                { "xrequestid", "X-Request-Id" },
                { "xinstallationid", "X-Installation-Id" },
                { "traceparent", "traceparent" },
                { "xapplication", "X-Application" },
                { "clientipaddress", "Clientipaddress" },
                { "clientid", "Clientid" },
                { "jti", "Jti" },
                { "xdeviceplatform", "X-Device-Platform" },
                { "xapplicationversion", "X-Application-Version" },
                { "xdeviceversion", "X-Device-Version" },
                { "xdeviceinfo", "X-Device-Info" },
            };

    public static Dictionary<string, string> OptionalHeaders
    {
        get
        {
            return optionalHeaders;
        }
    }
}

