using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.IdentityModel.Claims;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;
using System.Web;
using WebApp.Outlook;
using WebApp.Outlook.TokenStorage;

[assembly: OwinStartup(typeof(WebApp.Startup))]

namespace WebApp
{
    public partial class Startup
    {
        // Properties used to get and manage an access token.
        private string redirectUri = ServiceHelper.RedirectUri;

        private string appId = ServiceHelper.AppId;
        private string appPassword = ServiceHelper.AppSecret;
        private readonly string scopes = ServiceHelper.Scopes;

        public void Configuration(IAppBuilder app)
        {
            //憑證問題的網站出現遠端憑證是無效的錯誤問題，特定網站可以不檢查
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) =>
            {
                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    return true;
                }
                var request = sender as HttpWebRequest;
                if (request != null)
                {
                    var result = request.RequestUri.Host == "data.gcis.nat.gov.tw";
                    return result;
                }
                return false;
            };

            //允许CORS跨域
            //app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseOpenIdConnectAuthentication(
              new OpenIdConnectAuthenticationOptions
              {
                  // The `Authority` represents the Microsoft v2.0 authentication and authorization
                  // service. The `Scope` describes the permissions that your app will need. See https://azure.microsoft.com/documentation/articles/active-directory-v2-scopes/
                  ClientId = appId,
                  Authority = "https://login.microsoftonline.com/common/v2.0",
                  Scope = "openid offline_access profile email " + scopes,
                  RedirectUri = redirectUri,
                  PostLogoutRedirectUri = redirectUri,
                  TokenValidationParameters = new TokenValidationParameters
                  {
                      // For demo purposes only, see below
                      ValidateIssuer = false
                  },
                  Notifications = new OpenIdConnectAuthenticationNotifications
                  {
                      AuthorizationCodeReceived = OnAuthorizationCodeReceivedAsync,
                      AuthenticationFailed = OnAuthenticationFailedAsync
                  }
              }
            );
        }

        private async Task OnAuthorizationCodeReceivedAsync(AuthorizationCodeReceivedNotification notification)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Get the signed in user's id and create a token cache
            var signedInUserId = notification.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var tokenCache = new SessionTokenCache(
                signedInUserId, notification.OwinContext.Environment["System.Web.HttpContextBase"] as HttpContextBase).GetMsalCacheInstance();

            var cca = new ConfidentialClientApplication(appId, redirectUri, new ClientCredential(appPassword), tokenCache, null);

            try
            {
                var saScopes = scopes.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var result = await cca.AcquireTokenByAuthorizationCodeAsync(notification.Code, saScopes);
            }
            catch (MsalException ex)
            {
                var message = "AcquireTokenByAuthorizationCodeAsync threw an exception";
                var debug = ex.Message;
                notification.HandleResponse();
                notification.Response.Redirect("/Login/Error?message=" + message + "&debug=" + debug);
            }
        }

        private static Task OnAuthenticationFailedAsync(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            var redirect = "/Login/Error?message=" + notification.Exception.Message;
            if (notification.ProtocolMessage != null && !string.IsNullOrEmpty(notification.ProtocolMessage.ErrorDescription))
            {
                redirect += "&debug=" + notification.ProtocolMessage.ErrorDescription;
            }
            notification.Response.Redirect(redirect);
            return Task.FromResult(0);
        }
    }
}