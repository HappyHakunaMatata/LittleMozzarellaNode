using System.Collections;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using Amazon;
using Amazon.CertificateManager;
using Grpc.Net.Client.Web;
using gRPCCertificateSign.Services;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Node.Certificate;
using Node.Certificate.Models;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682



/*string? AWSAccessKey = System.Configuration.ConfigurationManager.AppSettings["AWSAccessKey"];
string? AWSSecretKey = System.Configuration.ConfigurationManager.AppSettings["AWSSecretKey"];
string? ARN = System.Configuration.ConfigurationManager.AppSettings["ARN"];
if (string.IsNullOrEmpty(AWSAccessKey) || string.IsNullOrEmpty(AWSSecretKey) || string.IsNullOrEmpty(ARN))
{
    Environment.Exit(1);
}*/


///TEST
///
var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"LittleMozzarella/Server");
CertificateSettings certificateSettings = new CertificateSettings(path);
CertificateAuthorityConfig certificateAuthorityConfig = new CertificateAuthorityConfig();
certificateAuthorityConfig.Difficulty = 14;
///
//var CertificateBuilder = new ConcreteSystemSecurityBuilder(certificateAuthorityConfig, certificateSettings);
CertificateAuthorizer loader = new CertificateAuthorizer(path);
if (!loader.TryLoadIdentConfig(out var fullIdentity) || !loader.TryLoadFullCAConfig(out var fullCertificateAuthority))
{
    string? AWSAccessKey = System.Configuration.ConfigurationManager.AppSettings["AWSAccessKey"];
    string? AWSSecretKey = System.Configuration.ConfigurationManager.AppSettings["AWSSecretKey"];
    string? ARN = System.Configuration.ConfigurationManager.AppSettings["ARN"];
    if (string.IsNullOrEmpty(AWSAccessKey) || string.IsNullOrEmpty(AWSSecretKey) || string.IsNullOrEmpty(ARN))
    {
        Environment.Exit(1);
    }
}




// Add services to the container.
builder.Services.AddGrpc();
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(o =>
    {
        //o.ClientCertificateValidation += ValidateClientCertificate;
        o.AllowAnyClientCertificate();
        o.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
        o.CheckCertificateRevocation = false;
    });
    options.ConfigureEndpointDefaults(options =>
    {
        options.Protocols = HttpProtocols.Http2;
        options.UseHttps();
    });
});



/*bool ValidateClientCertificate(X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
{
    return true;
}*/

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<CertificateSignService>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");


app.Run();

