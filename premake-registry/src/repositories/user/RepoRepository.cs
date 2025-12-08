using Microsoft.JSInterop;
using premake.User.premake.Repo;
using premake.repositories.user.objects;
namespace premake.repositories.user
{
    public class RepoRepository
    {
        private readonly BrowserCache<UserRepo> _cache;
        private readonly IJSRuntime _js;
        public RepoRepository( IJSRuntime js)
        {
            _cache = new BrowserCache<UserRepo>(js);
            _js = js;
        }
    }
}
