using Google.Cloud.Firestore;
using Microsoft.Extensions.Caching.Memory;
using premake.Repo;
using premake.repositories.registry.objects;
using premake_registry.src.frontend.Components.User.UserRepo;
using src.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using System.Timers;

#nullable enable

namespace premake.repositories.registry
{
    [YamlSerializable]
    internal class IndexLibrary
    {
        [YamlMember(Order = 0)]
        public required string name { get; set; }
        [YamlMember(Order = 1)]
        public string? description { get; set; }

    }
    /// <summary>
    /// Representation of the common-index yaml
    /// </summary>
    [YamlSerializable]
    internal class IndexView
    {
        [YamlMember(Order = 0)]
        public required string remote { get; set; }
        [YamlMember(Order = 1)]
        public required Dictionary<string, IList<IndexLibrary>> libraries;
    }
    public class IndexRepositories
    {
        private readonly Timer _refreshTimer;
        private readonly Cache<RegistryRepo[]> _cache;
        private IndexView _commonIndex { get; set; }
        public IndexRepositories(IMemoryCache cache)
        {

            _cache = new Cache<RegistryRepo[]>(cache);
            _commonIndex = YamlSerializer.Deserialize<IndexView>("lolrobbe2", "premake-common-registry", "premakeIndex.yml");
            _cache.CacheClear();
            _refreshTimer = new Timer(TimeSpan.FromDays(1).TotalMilliseconds);
            _refreshTimer.Elapsed += (s, e) => RefreshIndex();
            _refreshTimer.AutoReset = true;
            _refreshTimer.Enabled = true;
        }

        // --- Search by username ---
        public async Task<IReadOnlyList<RegistryRepo>> FindByUserNameAsync(string userName)
        {
            string cacheKey = $"index_user_{userName}";
            if (_cache.CacheGet(cacheKey, out RegistryRepo[] cached))
            {
                return cached;
            }

            var repos = _commonIndex.libraries.First(userLibs => userLibs.Key == userName);

            var convertedRepos = repos.Value.Select(lib => {
                return new RegistryRepo() { UserName = userName, RepoName = lib.name, isLib = true };
            });
            return _cache.CacheSet(convertedRepos.ToArray(), cacheKey);
        }

        // --- Search by repository name ---
        public async Task<IReadOnlyList<RegistryRepo>> FindByRepoNameAsync(string repoName)
        {
            string cacheKey = $"index_repo_{repoName}";
            if (_cache.CacheGet(cacheKey, out RegistryRepo[] cached))
            {
                return cached;
            }
            IList<RegistryRepo> repos = new List<RegistryRepo>();
            foreach (var (name, libs) in _commonIndex.libraries)
            {
                IndexLibrary? foundLib = libs.FirstOrDefault(lib =>
                {
                    return lib.name == repoName;
                });
                if(foundLib is not null)
                {   
                    repos.Add(new RegistryRepo() { UserName = name, RepoName = foundLib.name, isLib = true });
                }
            }
            return _cache.CacheSet(repos.ToArray(), cacheKey);
        }
        public async Task<IReadOnlyList<RegistryRepo>> GetFirst(int count)
        {
            string cacheKey = $"index_recent_{count}";
            if (_cache.CacheGet(cacheKey, out RegistryRepo[] cached))
            {
                return cached;
            }
            IList<RegistryRepo> libs = _commonIndex.libraries
                // 1. Flatten the Dictionary into one big list of RegistryRepos
                .SelectMany(kvp => kvp.Value.Select(lib => new RegistryRepo
                {
                    UserName = kvp.Key,
                    RepoName = lib.name,
                    isLib = true
                }))
                // 2. Sort the entire flattened list by the Repository Name
                .OrderBy(r => r.RepoName)
                // 3. Take only the requested amount
                .Take(count)
                .ToList();

            return _cache.CacheSet(libs.ToArray(), cacheKey);
        }
        private void RefreshIndex()
        {
            try
            {
                _commonIndex = YamlSerializer.Deserialize<IndexView>("lolrobbe2", "premake-common-registry", "premakeIndex.yml");
                Console.WriteLine("Refreshed index!");
            }
            catch (Exception ex)
            {
                // Log error so you don't lose your existing data on a failed download
                Console.WriteLine($"Failed to refresh index: {ex.Message}");
            }
        }
    }
}

