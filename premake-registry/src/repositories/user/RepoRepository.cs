using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;
using premake.repositories.user.objects;
using premake.User;
using premake.User.premake.Repo;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#nullable enable
namespace premake.repositories.user
{
    
    public class GitRepoRepository
    {

        private readonly IJSRuntime _js;
        private readonly CurrentUser _user;
        private readonly HttpClient _client;
        public GitRepoRepository(IJSRuntime js, CurrentUser user,HttpClient client)
        {
            _js = js;
            _user = user;
            _client = client;   
        }

        /// <summary>
        /// Fetch repositories for the current user and convert to UserRepo[].
        /// </summary>
        public async Task<UserRepo[]?> GetUserReposAsync()
        {
            if (string.IsNullOrWhiteSpace(_user.ReposUri))
                return Array.Empty<UserRepo>();

            return await _user.GetFromApiAsync<UserRepo[]>(_user.ReposUri);
        }
    }
}
