using EasyBL;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WebApp.Outlook.Models;

namespace WebApp.Outlook
{
    public static class Statics
    {
        public static T Deserialize<T>(this string result)
        {
            return JsonConvert.DeserializeObject<T>(result);
        }
    }

    public class GraphService
    {
        private static string GraphRootUri = Common.GetAppSettings("ida:GraphRootUri");

        /// <summary>
        /// Get the current user's id from their profile.
        /// </summary>
        /// <param name="accessToken">Access token to validate user</param>
        /// <returns></returns>
        public async Task<string> GetMyIdAsync(String accessToken)
        {
            //string endpoint = "https://graph.microsoft.com/v1.0/me";
            var endpoint = $"{GraphRootUri}/me";
            var queryParameter = "?$select=id";
            var userId = "";
            var response = await ServiceHelper.SendRequestAsync(HttpMethod.Get, endpoint + queryParameter, accessToken);
            if (response != null && response.IsSuccessStatusCode)
            {
                var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                userId = json.GetValue("id").ToString();
            }
            return userId?.Trim();
        }

        // Get the current user's email address from their profile.
        public async Task<string> GetMyEmailAddressAsync(GraphServiceClient graphClient)
        {
            // Get the current user. This sample only needs the user's email address, so select the
            // mail and userPrincipalName properties. If the mail property isn't defined,
            // userPrincipalName should map to the email for all account types.
            var me = await graphClient.Me.Request().Select("mail,userPrincipalName").GetAsync();
            return me.Mail ?? me.UserPrincipalName;
        }

        public async Task<IUserEventsCollectionPage> GetMyCalendarEventsAsync(GraphServiceClient graphClient)
        {
            var Events = await graphClient.Me.Events.Request().GetAsync();
            return Events;
        }

        public async Task<string> UpdateMyEventsAsync(string accessToken, string eventId, Event _event)
        {
            var endpoint = $"{GraphRootUri}/me/events/{eventId}";

            var response = await ServiceHelper.SendRequestAsync(new HttpMethod("PATCH"), endpoint, accessToken, _event);
            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase) { Source = JsonConvert.SerializeObject(response) };
            return response.ReasonPhrase;
        }

        public async Task<bool> DeleteMyEventsAsync(string accessToken, string eventId)
        {
            var endpoint = $"{GraphRootUri}/me/events/{eventId}";

            var response = await ServiceHelper.SendRequestAsync(HttpMethod.Delete, endpoint, accessToken);
            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase) { Source = JsonConvert.SerializeObject(response) };
            return response.ReasonPhrase == "No Content";
        }

        /// <summary>
        /// Create new channel.
        /// </summary>
        /// <param name="accessToken">Access token to validate user</param>
        /// <param name="teamId">Id of the team in which new channel needs to be created</param>
        /// <param name="channelName">New channel name</param>
        /// <param name="channelDescription">New channel description</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> CreateChannelAsync(string accessToken, string teamId, string channelName, string channelDescription)
        {
            var endpoint = $"{GraphRootUri}/teams/{teamId}/channels";

            var content = new Channel()
            {
                Description = channelDescription,
                DisplayName = channelName
            };

            var response = await ServiceHelper.SendRequestAsync(HttpMethod.Post, endpoint, accessToken, content);

            return response;//.ReasonPhrase;
        }

        /// <summary>
        /// Get all channels of the given.
        /// </summary>
        /// <param name="accessToken">Access token to validate user</param>
        /// <param name="teamId">Id of the team to get all associated channels</param>
        /// <param name="resourcePropId">todo: describe resourcePropId parameter on GetChannelsAsync</param>
        /// <returns></returns>
        public async Task<IEnumerable<ResultsItem>> GetChannelsAsync(string accessToken, string teamId, string resourcePropId)
        {
            var endpoint = $"{GraphRootUri}/teams/{teamId}/channels";

            var items = new List<ResultsItem>();
            var response = await ServiceHelper.SendRequestAsync(HttpMethod.Get, endpoint, accessToken);
            if (response != null && response.IsSuccessStatusCode)
            {
                items = await ServiceHelper.GetResultsItemAsync(response, "id", "displayName", resourcePropId);
            }
            return items;
        }

        /// <summary>
        /// Get all teams which user is the member of.
        /// </summary>
        /// <param name="accessToken">Access token to validate user</param>
        /// <param name="resourcePropId">todo: describe resourcePropId parameter on GetMyTeamsAsync</param>
        /// <returns></returns>
        public async Task<IEnumerable<ResultsItem>> GetMyTeamsAsync(string accessToken, string resourcePropId)
        {
            var endpoint = $"{GraphRootUri}/me/joinedTeams";

            var items = new List<ResultsItem>();
            var response = await ServiceHelper.SendRequestAsync(HttpMethod.Get, endpoint, accessToken);
            if (response != null && response.IsSuccessStatusCode)
            {
                items = await ServiceHelper.GetResultsItemAsync(response, "id", "displayName", resourcePropId);
            }
            return items;
        }

        /// <summary>
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="teamId"></param>
        /// <param name="channelId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostMessageAsync(string accessToken, string teamId, string channelId, string message)
        {
            var endpoint = $"{GraphRootUri}/teams/{teamId}/channels/{channelId}/chatThreads";

            var content = new PostMessage()
            {
                RootMessage = new RootMessage()
                {
                    body = new MSG()
                    {
                        Content = message
                    }
                }
            };
            var items = new List<ResultsItem>();
            var response = await ServiceHelper.SendRequestAsync(HttpMethod.Post, endpoint, accessToken, content);

            return response;//response.ReasonPhrase;
        }

        /// <summary>
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public async Task<string> CreateNewTeamAndGroupAsync(string accessToken, Models.Group group)
        {
            // create group
            var endpoint = $"{GraphRootUri}/groups";
            if (group != null)
            {
                group.GroupTypes = new string[] { "Unified" };
                group.MailEnabled = true;
                group.SecurityEnabled = false;
                group.Visibility = "Private";
            }

            var response = await ServiceHelper.SendRequestAsync(HttpMethod.Post, endpoint, accessToken, group);
            if (!response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            var responseBody = await response.Content.ReadAsStringAsync(); ;
            var groupId = responseBody.Deserialize<Models.Group>().Id; // groupId is the same as teamId

            // add me as member
            var me = await GetMyIdAsync(accessToken);
            var payload = $"{{ '@odata.id': '{GraphRootUri}/users/{me}' }}";
            var responseRef = await ServiceHelper.SendRequestAsync(HttpMethod.Post,
                $"{GraphRootUri}/groups/{groupId}/members/$ref",
                accessToken, payload);

            // create team
            await AddTeamToGroupAsync(groupId, accessToken);
            return $"Created {groupId}";
        }

        /// <summary>
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task<String> AddTeamToGroupAsync(string groupId, string accessToken)
        {
            var endpoint = $"{GraphRootUri}/groups/{groupId}/team";
            var team = new Team
            {
                guestSettings = new Models.TeamGuestSettings() { AllowCreateUpdateChannels = false, AllowDeleteChannels = false }
            };

            var response = await ServiceHelper.SendRequestAsync(HttpMethod.Put, endpoint, accessToken, team);
            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase) { Source = JsonConvert.SerializeObject(response) };
            return response.ReasonPhrase;
        }

        /// <summary>
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task<String> UpdateTeamAsync(string teamId, string accessToken)
        {
            var endpoint = $"{GraphRootUri}/teams/{teamId}";

            var team = new Team
            {
                guestSettings = new Models.TeamGuestSettings() { AllowCreateUpdateChannels = true, AllowDeleteChannels = false }
            };

            var response = await ServiceHelper.SendRequestAsync(new HttpMethod("PATCH"), endpoint, accessToken, team);
            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase) { Source = JsonConvert.SerializeObject(response) };
            return response.ReasonPhrase;
        }

        /// <summary>
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="member"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task AddMemberAsync(string teamId, Member member, string accessToken)
        {
            // If you have a user's UPN, you can add it directly to a group, but then there will be a
            // significant delay before Microsoft Teams reflects the change. Instead, we find the
            // user object's id, and add the ID to the group through the Graph beta endpoint, which
            // is recognized by Microsoft Teams much more quickly. See
            // https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/resources/teams_api_overview
            // for more about delays with adding members.

            // Step 1 -- Look up the user's id from their UPN
            var endpoint = $"{GraphRootUri}/users/{member.Upn}";
            var response = await ServiceHelper.SendRequestAsync(HttpMethod.Get, endpoint, accessToken);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase) { Source = JsonConvert.SerializeObject(response) };

            var userId = responseBody.Deserialize<Member>().Id;

            // Step 2 -- add that id to the group
            var payload = $"{{ '@odata.id': '{GraphRootUri}/users/{userId}' }}";
            endpoint = $"{GraphRootUri}/groups/{teamId}/members/$ref";

            var responseRef = await ServiceHelper.SendRequestAsync(HttpMethod.Post, endpoint, accessToken, payload);
            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase) { Source = JsonConvert.SerializeObject(response) };

            if (member.Owner)
            {
                endpoint = $"{GraphRootUri}/groups/{teamId}/owners/$ref";
                var responseOwner = await ServiceHelper.SendRequestAsync(HttpMethod.Post, endpoint, accessToken, payload);
                if (!response.IsSuccessStatusCode)
                    throw new Exception(response.ReasonPhrase) { Source = JsonConvert.SerializeObject(response) };
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="teamId"></param>
        /// <param name="resourcePropId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ResultsItem>> ListAppsAsync(string accessToken, string teamId, string resourcePropId)
        {
            var response = await ServiceHelper.SendRequestAsync(
                HttpMethod.Get,
                $"{GraphRootUri}/teams/{teamId}/apps",
                accessToken);
            var responseBody = await response.Content.ReadAsStringAsync();

            var items = new List<ResultsItem>();
            if (response != null && response.IsSuccessStatusCode)
            {
                items = await ServiceHelper.GetResultsItemAsync(response, "id", "displayName", resourcePropId);
            }
            return items;
        }
    }
}