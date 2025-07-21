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
                { "X-JWS-Signature", "X-JWS-Signature" },
                { "X-Client-Cert-Serial", "X-Client-Cert-Serial" },
                { "x_jws_signature", "x_jws_signature" },
                { "x_client_cert_serial", "x_client_cert_serial" },
                { "x_request_id", "x_request_id" },
                { "x_installation_id", "x_installation_id" },
                { "x_device_id", "x_device_id" },
                { "ResourceUrl", "ResourceUrl" },
                { "resourceurl", "resourceurl" },
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

