using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;
using premake.repositories.registry.objects;
using premake.repositories.user.objects;
using premake.User;
using premake.User.premake.Repo;
using System;
using System.Linq;
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
        private readonly CollectionReference _reposCollection;
        public GitRepoRepository(IJSRuntime js, CurrentUser user,HttpClient client, FirestoreDb firestore)
        {
            _reposCollection = firestore.Collection("RegisteredRepos");
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

        public async Task<UserRepo> GetUserRepoAsync(RegistryRepo repo)
        {
            return await _user.GetFromApiAsync<UserRepo>(repo.ApiUrl); 
        }
        public async Task<UserRepo[]> GetUserReposAsync(RegistryRepo[] repos)
        {
            Task<UserRepo>[] tasks = repos.Select(repo => GetUserRepoAsync(repo)).ToArray();
            return await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Registers a repository in Firestore.
        /// </summary>
        public async Task RegisterAsync(RegistryRepo repo)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            if(repo.UserName != _user.UserName)
            {
                throw new ArgumentException("registering other user repos is forbidden");
            }
            if (_user.IsLoggedIn())
            {
                // Use RepoName + UserName as a unique key
                var docId = $"{repo.UserName}_{repo.RepoName}";
                var docRef = _reposCollection.Document(docId);

                await docRef.SetAsync(repo, SetOptions.Overwrite);
            }
        }

        /// <summary>
        /// Get all registered repositories for the current user from Firestore.
        /// </summary>
        public async Task<RegistryRepo[]> GetMyRegisteredReposAsync()
        {
            if (!_user.IsLoggedIn())
                return Array.Empty<RegistryRepo>();

            // Query Firestore for repos where UserName == current user
            var query = _reposCollection.WhereEqualTo(nameof(RegistryRepo.UserName), _user.UserName);
            var snapshot = await query.GetSnapshotAsync();

            var repos = snapshot.Documents
                .Select(doc => doc.ConvertTo<RegistryRepo>())
                .ToArray();

            return repos;
        }

    }
}
