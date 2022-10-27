using EasyBL;
using Microsoft.Identity.Client;
using Microsoft.Office365.OutlookServices;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using WebApp.Outlook.TokenStorage;

namespace WebApp.Outlook.AuthProvider
{
    public sealed class AuthProvider : IAuthProvider
    {
        // Properties used to get and manage an access token.
        private readonly string redirectUri = ServiceHelper.RedirectUri;

        private readonly string appId = ServiceHelper.AppId;
        private readonly string appPassword = ServiceHelper.AppSecret;
        private readonly string graphScopes = ServiceHelper.Scopes;
        private readonly string graphScopes_Outlook = ServiceHelper.Scopes_Outlook;

        private static readonly AuthProvider instance = new AuthProvider();

        private AuthProvider()
        {
        }

        public static AuthProvider Instance
        {
            get
            {
                return instance;
            }
        }

        public string AppPassword => appPassword;

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAccessTokenAsync()
        {
            try
            {
                string accessToken = null;
                // Load the app config from web.config

                // Get the current user's ID
                var signedInUserID = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (!string.IsNullOrEmpty(signedInUserID))
                {
                    // Get the user's token cache
                    var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);
                    var tokenCache = new SessionTokenCache(signedInUserID, httpContextWrapper).GetMsalCacheInstance();

                    var cca = new ConfidentialClientApplication(appId, redirectUri, new ClientCredential(AppPassword), tokenCache, null);

                    // Call AcquireTokenSilentAsync, which will return the cached access token if it
                    // has not expired. If it has expired, it will handle using the refresh token to
                    // get a new one.
                    var scopes = graphScopes.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var result = await cca.AcquireTokenSilentAsync(scopes, cca.Users.First());

                    accessToken = result.AccessToken;
                }

                return accessToken;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetOutlookAccessTokenAsync()
        {
            try
            {
                string accessToken = null;
                // Get the current user's ID
                var signedInUserID = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (!string.IsNullOrEmpty(signedInUserID))
                {
                    var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);
                    var tokenCache = new SessionTokenCache(signedInUserID, httpContextWrapper).GetMsalCacheInstance();
                    var cca = new ConfidentialClientApplication(appId, redirectUri, new ClientCredential(AppPassword), tokenCache, null);
                    var scopes = graphScopes_Outlook.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var result = await cca.AcquireTokenSilentAsync(scopes, cca.Users.First());
                    accessToken = result.AccessToken;
                }
                return accessToken;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetCacheOutlookAccessTokenAsync()
        {
            try
            {
                var accessToken = (string)HttpRuntimeCache.Get("outlookaccesstoken");

                return accessToken;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetUserEmailAsync()
        {
            var client = new OutlookServicesClient(new Uri("https://outlook.office.com/api/v2.0"), AuthProvider.Instance.GetOutlookAccessTokenAsync);
            try
            {
                var userDetail = (User)await client.Me.ExecuteAsync();
                return userDetail.EmailAddress;
            }
            catch (MsalException ex)
            {
                return $"#ERROR#: Could not get user's email address. {ex.Message}";
            }
        }

        /// <summary>
        /// 請求初始化
        /// </summary>
        /// <param name="email">todo: describe email parameter on GetRequestAsync</param>
        /// <returns></returns>
        public async Task<OutlookServicesClient> GetRequestAsync(string email = null)
        {
            var userEmail = email;
            if (email == null)
            {
                userEmail = await GetUserEmailAsync();
            }
            Func<Task<string>> Getter = AuthProvider.Instance.GetOutlookAccessTokenAsync;
            if (email != null)
            {
                Getter = AuthProvider.Instance.GetCacheOutlookAccessTokenAsync;
            }
            var client = new OutlookServicesClient(new Uri("https://outlook.office.com/api/v2.0"), Getter);
            client.Context.SendingRequest2 += new EventHandler<Microsoft.OData.Client.SendingRequest2EventArgs>(
                (sender, e) => InsertXAnchorMailboxHeader(e, userEmail));
            return client;
        }

        /// <summary>
        /// 添加Header郵件地址
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="email"></param>
        public void InsertXAnchorMailboxHeader(Microsoft.OData.Client.SendingRequest2EventArgs e, string email)
        {
            e.RequestMessage.SetHeader("X-AnchorMailbox", email);
        }
    }
}