using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;
using premake.Repo;
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
        private readonly Cache<UserRepo[]> _userRepoCache;
        private readonly Cache<UserRepo> _repoCache;
        private readonly Cache<Owner> _ownerCache;
        private readonly IJSRuntime _js;
        private readonly CurrentUser _user;
        private readonly HttpClient _client;
        private readonly CollectionReference _reposCollection;
        public GitRepoRepository(IMemoryCache cache,IJSRuntime js, CurrentUser user,HttpClient client, FirestoreDb firestore)
        {
            _reposCollection = firestore.Collection("RegisteredRepos");
            _js = js;
            _user = user;
            _client = client;
            _userRepoCache = new Cache<UserRepo[]>(cache);
            _repoCache = new Cache<UserRepo>(cache);
            _ownerCache = new Cache<Owner>(cache);
        }

        public async Task<UserRepo[]?> GetUserReposAsync()
        {
            if (string.IsNullOrWhiteSpace(_user.ReposUri))
                return Array.Empty<UserRepo>();

            string cacheKey = $"userrepos_{_user.UserName}";

            return await _userRepoCache.CacheComputeAsync(cacheKey, async () => (await _user.GetFromApiAsync<UserRepo[]>(_user.ReposUri)) ?? Array.Empty<UserRepo>()); ;
          
        }

        public async Task<UserRepo?> GetUserRepoAsync(RegistryRepo repo)
        {
            string cacheKey = $"repo_{repo.UserName}_{repo.RepoName}";
            return await _repoCache.CacheComputeAsync(cacheKey, async () =>
            {
                var userRepo = await _user.GetFromApiAsync<UserRepo>(repo.ApiUrl);
                if (userRepo == null)
                {
                    var owner = new Owner()
                    {
                        login = repo.UserName,
                        // The web profile: https://github.com/UserName
                        html_url = $"https://github.com/{repo.UserName}",

                        // The avatar: https://github.com/UserName.png
                        avatar_url = $"https://github.com/{repo.UserName}.png",

                        // The API endpoint: https://api.github.com/users/UserName
                    };
                    return new UserRepo() { description = "", full_name = $"{repo.UserName}/{repo.RepoName}", issues = 0, name = repo.RepoName, owner = owner, url = $"https://github.com/{repo.UserName}/{repo.RepoName}", html_url = "" };
                }
                  

                userRepo.owner = (await GetOwnerAsync(repo.UserName))!;
                return userRepo;
            });
        }

        public async Task<UserRepo[]> GetUserReposAsyncNonNull(RegistryRepo[] repos)
        {
            // Kick off all tasks
            var tasks = repos.Select(repo => GetUserRepoAsync(repo)).ToArray();
            var results = await Task.WhenAll(tasks);

            // Drop nulls
            return results.Where(r => r != null).ToArray()!;
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
            if (_user.IsLoggedIn() && repo.UserName == _user.UserName)
            {
                // Use RepoName + UserName as a unique key
                var docId = $"{repo.UserName}_{repo.RepoName}";
                var docRef = _reposCollection.Document(docId);

                await docRef.SetAsync(repo, SetOptions.Overwrite);
            }
        }

        /// <summary>
        /// Registers a repository in Firestore.
        /// </summary>
        public async Task UnregisterAsync(RegistryRepo repo)
        {
            //TODO force clear cache.
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            if (repo.UserName != _user.UserName)
            {
                throw new ArgumentException("registering other user repos is forbidden");
            }
            if (_user.IsLoggedIn() && repo.UserName == _user.UserName)
            {
                // Use RepoName + UserName as a unique key
                var docId = $"{repo.UserName}_{repo.RepoName}";
                var docRef = _reposCollection.Document(docId);

                await docRef.DeleteAsync();
            }
        }

        /// <summary>
        /// Get all registered repositories for the current user from Firestore.
        /// </summary>
        public async Task<RegistryRepo[]> GetRegisteredReposAsync()
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
        public async Task<Owner?> GetOwnerAsync(string name)
        {
            string cacheKey = $"owner_{name}";

            // Try cache first
            if (_ownerCache.CacheGet(cacheKey, out Owner cached))
                return cached;
            

            // Fetch from API
            var owner = await _user.GetFromApiAsync<Owner>($"https://api.github.com/users/{name}");
            if (owner == null)
                return null;
            

            // Store in cache
            return _ownerCache.CacheSet(owner, cacheKey);
        }


    }
}
