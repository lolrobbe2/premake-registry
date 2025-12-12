using Microsoft.AspNetCore.Mvc;
using premake.repositories.registry;
using premake.repositories.registry.objects;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public UserRepositoriesController(UserRepositories userRepositories)
        {
            _userRepositories = userRepositories;
        }

        [HttpGet("pagecount")]
        public async Task<ActionResult<int>> PageCount()
        {
            return Ok(await _userRepositories.GetPageCount());
        }
        /// <summary>
        /// Unified search endpoint. Pass type + value.
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyList<RegistryRepo>>> Search(
            [FromQuery] RepoSearchType type,
            [FromQuery] string value,
            [FromQuery] int page = 10)
        {
            IReadOnlyList<RegistryRepo> results;

            switch (type)
            {
                case RepoSearchType.UserName:
                    results = await _userRepositories.FindByUserNameAsync(value,page);
                    break;

                case RepoSearchType.RepoName:
                    results = await _userRepositories.FindByRepoNameAsync(value,page);
                    break;

                case RepoSearchType.Tag:
                    results = await _userRepositories.FindByTagAsync(value, page);
                    break;

                case RepoSearchType.Recent:
                    results = await _userRepositories.GetMostRecentAsync(page);
                    break;

                default:
                    return BadRequest("Invalid search type.");
            }

            return Ok(results);
        }
    }
}


