using Microsoft.Graph;
using System.Net.Http.Headers;

namespace WebApp.Outlook
{
    public class SDKHelper
    {
        // Get an authenticated Microsoft Graph Service client.
        public static GraphServiceClient AuthenticatedClient
        {
            get
            {
                var graphClient = new GraphServiceClient(
                    new DelegateAuthenticationProvider(
                        async (requestMessage) =>
                        {
                            var accessToken = await AuthProvider.AuthProvider.Instance.GetAccessTokenAsync();

                            // Append the access token to the request.
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                        }));
                return graphClient;
            }
        }
    }
}