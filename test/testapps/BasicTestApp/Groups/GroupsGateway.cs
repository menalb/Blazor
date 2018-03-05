using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BlazorHttpClient = Microsoft.AspNetCore.Blazor.Browser.Services.Temporary;

namespace BasicTestApp.Groups
{
    public interface IQueryGateway<TMessage>
    {
        Task<IEnumerable<TMessage>> GetAll();
    }

    public class GroupsQuery : IQueryGateway<Group>
    {
        private readonly BlazorHttpClient.HttpClient _client;
        private readonly GatewaysConfiguration _baseUri;

        public GroupsQuery(BlazorHttpClient.HttpClient client, GatewaysConfiguration baseUri)
        {
            _client = client;
            _baseUri=baseUri;
        }

        public async Task<IEnumerable<Group>> GetAll()
        {
            var response = await _client.GetAsync(_baseUri.GroupsBaseUrl);
            var responseStatusCode = response.StatusCode;
            var responseBody = await response.Content.ReadAsStringAsync();

            var groupsResponse = JsonConvert.DeserializeObject<GroupsViewModel>(responseBody);
            return groupsResponse.Groups;
        }
    }
}