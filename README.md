# SplunkLoggingWithNLog
Splunk Logging with NLog from ASP.NET Core (v3.1) API

## Required Nuget Packages
* NLog
* NLog.Web.AspNetCore

## Notes
Usually, We may not need to configure the NLog programatically (C#) as we have all the options available in nlog.config. But this example also illustrates how to programatically configure NLog. See below code.

```
//Get nlog.config file
var nlogConfiguration = NLog.LogManager.Configuration;

//To ignore Splunk certificate errors
ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

var webServiceTarget = nlogConfiguration.FindTargetByName<WebServiceTarget>("Splunk");
webServiceTarget.Url = new Uri(Configuration["SplunkUrl"]);
MethodCallParameter headerParameter = new MethodCallParameter
{
    Name = Configuration["SplunkHeaderName"],
    Layout = Configuration["SplunkHeaderLayout"]
};

webServiceTarget.Headers.Add(headerParameter);

if (!String.IsNullOrWhiteSpace(AppEnvironment) && AppEnvironment.Equals("local"))
{
    var logconsole = new ConsoleTarget("logconsole");
    nlogConfiguration.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
}

NLog.LogManager.Configuration = nlogConfiguration;

//Set Global Diagnostics context
GlobalDiagnosticsContext.Set("application", Configuration["Application"]);
GlobalDiagnosticsContext.Set("environment", AppEnvironment);
```
