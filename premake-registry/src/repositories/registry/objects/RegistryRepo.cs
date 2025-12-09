using Google.Cloud.Firestore;
using Microsoft.Extensions.Caching.Memory;
using premake.Repo;
using premake_registry.src.frontend.Components.User.UserRepo;
using System;

namespace premake.repositories.registry.objects
{
    /// <summary>
    /// This represents a registered premake library
    /// </summary>
    [FirestoreData]
    public class RegistryRepo
    {
        [FirestoreProperty]
        public string UserName { get; set; }
        [FirestoreProperty]
        public string RepoName { get; set; }
        [FirestoreProperty]
        public string[] tags { get; set; } = Array.Empty<string>();
        [FirestoreProperty]
        public Timestamp CreatedAt { get; set; } = Timestamp.FromDateTime(DateTime.UtcNow);
        [FirestoreProperty]
        public bool isLib { get; set; }
        /// <summary>
        /// Getter to get the repository git link
        /// </summary>
        public string RepoUrl { get => $"https://github.com/{UserName}/{RepoName}.git"; }
        /// <summary>
        /// Getter to get the raw readme link
        /// </summary>
        public string RepoReadme { get => $"https://raw.githubusercontent.com/{UserName}/{RepoName}/refs/heads/main/README.md"; }
        public string ApiUrl => $"https://api.github.com/repos/{UserName}/{RepoName}";
    }
}
