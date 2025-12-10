using Google.Cloud.Firestore;
using Microsoft.Extensions.Caching.Memory;
using premake.Repo;
using premake.repositories.registry.objects;
using premake.User;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace premake.repositories.registry
{
    public class UserRepositories
    {
        private readonly Cache<RegistryRepo[]> _cache;
        private readonly CollectionReference _reposCollection;

        public UserRepositories(IMemoryCache cache, FirestoreDb firestore)
        {
            _cache = new Cache<RegistryRepo[]>(cache);
            _reposCollection = firestore.Collection("RegisteredRepos");
        }

        // --- Search by username ---
        public async Task<IReadOnlyList<RegistryRepo>> FindByUserNameAsync(string userName)
        {
            string cacheKey = $"user_{userName}";
            if (_cache.CacheGet(cacheKey, out RegistryRepo[] cached))
            {
                return cached;
            }

            var snapshot = await _reposCollection
                .WhereEqualTo(nameof(RegistryRepo.UserName), userName)
                .GetSnapshotAsync();

            var repos = snapshot.Documents.Select(d => d.ConvertTo<RegistryRepo>()).ToArray();
            return _cache.CacheSet(repos, cacheKey);
        }

        // --- Search by repository name ---
        public async Task<IReadOnlyList<RegistryRepo>> FindByRepoNameAsync(string repoName)
        {
            string cacheKey = $"repo_{repoName}";
            if (_cache.CacheGet(cacheKey, out RegistryRepo[] cached))
            {
                return cached;
            }

            var snapshot = await _reposCollection
                .WhereEqualTo(nameof(RegistryRepo.RepoName), repoName)
                .GetSnapshotAsync();

            var repos = snapshot.Documents.Select(d => d.ConvertTo<RegistryRepo>()).ToArray();
            return _cache.CacheSet(repos, cacheKey);
        }

        // --- Search by tag ---
        public async Task<IReadOnlyList<RegistryRepo>> FindByTagAsync(string tag)
        {
            string cacheKey = $"tag_{tag}";
            if (_cache.CacheGet(cacheKey, out RegistryRepo[] cached))
            {
                return cached;
            }

            var snapshot = await _reposCollection
                .WhereArrayContains(nameof(RegistryRepo.tags), tag)
                .GetSnapshotAsync();

            var repos = snapshot.Documents.Select(d => d.ConvertTo<RegistryRepo>()).ToArray();
            return _cache.CacheSet(repos, cacheKey);
        }

        // --- Most recent repos ---
        public async Task<IReadOnlyList<RegistryRepo>> GetMostRecentAsync(int count = 10)
        {
            string cacheKey = $"recent_{count}";
            if (_cache.CacheGet(cacheKey, out RegistryRepo[] cached))
            {
                return cached;
            }

            var snapshot = await _reposCollection
                .OrderByDescending(nameof(RegistryRepo.CreatedAt))
                .Limit(count)
                .GetSnapshotAsync();

            var repos = snapshot.Documents.Select(d => d.ConvertTo<RegistryRepo>()).ToArray();
            return _cache.CacheSet(repos, cacheKey);
        }
    }
}
