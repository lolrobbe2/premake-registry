using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using premake.repositories.registry;
using premake.repositories.registry.objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#nullable enable
namespace premake.controllers
{
    public enum RepoSearchType
    {
        UserName,
        RepoName,
        Tag,
        Recent
    }
    [ApiController]
    [Route("api/[controller]")]
    public class UserRepositoriesController : ControllerBase
    {
        private readonly UserRepositories _userRepositories;
        private readonly IndexRepositories _indexRepositories;
        public UserRepositoriesController(UserRepositories userRepositories, IndexRepositories indexRepositories)
        {
            _userRepositories = userRepositories;
            _indexRepositories = indexRepositories;
        }

        [HttpGet("pagecount")]
        [EnableCors("PublicApiPolicy")]
        public async Task<ActionResult<int>> PageCount()
        {
            return Ok(await _userRepositories.GetPageCount());
        }
        /// <summary>
        /// Unified search endpoint. Pass type + value.
        /// </summary>
        [HttpGet("search")]
        [EnableCors("PublicApiPolicy")]
        public async Task<ActionResult<IReadOnlyList<RegistryRepo>>> Search(
            [FromQuery] RepoSearchType type,
            [FromQuery] string? value,
            [FromQuery] int page = 10)
        {
            IReadOnlyList<RegistryRepo> results;
            IReadOnlyList<RegistryRepo> indexResults;

            switch (type)
            {
                case RepoSearchType.UserName:
                    results = await _userRepositories.FindByUserNameAsync(value ?? "", page);
                    indexResults = await _indexRepositories.FindByUserNameAsync(value ?? "", page);
                    break;

                case RepoSearchType.RepoName:
                    results = await _userRepositories.FindByRepoNameAsync(value ?? "", page);
                    indexResults = await _indexRepositories.FindByRepoNameAsync(value ?? "", page);
                    break;

                case RepoSearchType.Tag:
                    results = await _userRepositories.FindByTagAsync(value ?? "", page);
                    indexResults = new List<RegistryRepo>();
                    break;

                case RepoSearchType.Recent:
                    results = await _userRepositories.GetMostRecentAsync(page);
                    indexResults = new List<RegistryRepo>();
                    break;

                default:
                    return BadRequest("Invalid search type.");
            }

            return Ok(results.Concat(indexResults).ToList());
        }
    }
}


