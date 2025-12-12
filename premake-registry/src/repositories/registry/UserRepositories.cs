using DocumentFormat.OpenXml.Wordprocessing;
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
        private readonly Cache<RegistryRepo> _cacheSingle;
        private readonly CollectionReference _reposCollection;
        private readonly int pageSize = 10;
        public UserRepositories(IMemoryCache cache, FirestoreDb firestore)
        {
            _cache = new Cache<RegistryRepo[]>(cache);
            _cacheSingle = new Cache<RegistryRepo>(cache);

            _reposCollection = firestore.Collection("RegisteredRepos");
        }
        public async Task<int> GetPageCount()
        {
            var snapshot = await _reposCollection.GetSnapshotAsync();
            return snapshot.Documents.Count / pageSize;
        }
        public async Task<RegistryRepo[]> GetByFieldPaged(string fieldPath, string value,int page)
        {
            var snapshot = await _reposCollection
             .Offset(page * pageSize)
             .Limit(pageSize)
             .WhereEqualTo(fieldPath, value)
             .GetSnapshotAsync();

            return snapshot.Documents.Select(d => d.ConvertTo<RegistryRepo>()).ToArray();
        }

        // --- Search by username ---
        public async Task<IReadOnlyList<RegistryRepo>> FindByUserNameAsync(string userName, int page)
        {
            string cacheKey = $"user_{userName}_{page}";
          
            return await _cache.CacheComputeAsync(cacheKey, async () => await GetByFieldPaged("UserName",userName,page));
        }

        // --- Search by repository name ---
        public async Task<IReadOnlyList<RegistryRepo>> FindByRepoNameAsync(string repoName, int page)
        {
            string cacheKey = $"repo_{repoName}_{page}";
            return await _cache.CacheComputeAsync(cacheKey, async () => await GetByFieldPaged("RepoName", repoName, page));
        }

        // --- Search by tag ---
        public async Task<IReadOnlyList<RegistryRepo>> FindByTagAsync(string tag, int page)
        {
            string cacheKey = $"tag_{tag}_{page}";
            return await _cache.CacheComputeAsync(cacheKey, async () => await GetByFieldPaged("tags", tag, page));
        }

        // --- Most recent repos ---
        public async Task<IReadOnlyList<RegistryRepo>> GetMostRecentAsync(int page)
        {
            string cacheKey = $"recent_{page}";
            return await _cache.CacheComputeAsync(cacheKey, async ()=>{
                var snapshot = await _reposCollection
                    .Offset(page * pageSize)
                    .Limit(pageSize)
                    .OrderByDescending(nameof(RegistryRepo.CreatedAt))
                    .GetSnapshotAsync();

                return snapshot.Documents.Select(d => d.ConvertTo<RegistryRepo>()).ToArray();
            });
        }

        public async Task<RegistryRepo> GetRepo(string UserName,string RepoName)
        {
            string cacheKey = $"{UserName}_{RepoName}";
            return await _cacheSingle.CacheComputeAsync(cacheKey, async () => {
                var snapshot = await _reposCollection
                    .WhereEqualTo("UserName", UserName)
                    .WhereEqualTo("RepoName", RepoName)
                    .GetSnapshotAsync();

                return snapshot.Documents.Select(d => d.ConvertTo<RegistryRepo>()).First();
            });
        }
    }
}
