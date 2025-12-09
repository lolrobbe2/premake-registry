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
            var snapshot = await _reposCollection
                .WhereEqualTo(nameof(RegistryRepo.UserName), userName)
                .GetSnapshotAsync();

            return snapshot.Documents.Select(d => d.ConvertTo<RegistryRepo>()).ToList();
        }

        // --- Search by repository name ---
        public async Task<IReadOnlyList<RegistryRepo>> FindByRepoNameAsync(string repoName)
        {
            var snapshot = await _reposCollection
                .WhereEqualTo(nameof(RegistryRepo.RepoName), repoName)
                .GetSnapshotAsync();

            return snapshot.Documents.Select(d => d.ConvertTo<RegistryRepo>()).ToList();
        }

        // --- Search by tag ---
        public async Task<IReadOnlyList<RegistryRepo>> FindByTagAsync(string tag)
        {
            var snapshot = await _reposCollection
                .WhereArrayContains(nameof(RegistryRepo.tags), tag)
                .GetSnapshotAsync();

            return snapshot.Documents.Select(d => d.ConvertTo<RegistryRepo>()).ToList();
        }

        public async Task<IReadOnlyList<RegistryRepo>> GetMostRecentAsync(int count = 10)
        {
            var snapshot = await _reposCollection
                .OrderByDescending(nameof(RegistryRepo.CreatedAt))
                .Limit(count)
                .GetSnapshotAsync();

            return snapshot.Documents.Select(d => d.ConvertTo<RegistryRepo>()).ToList();
        }
    }
}
