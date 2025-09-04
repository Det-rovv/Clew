using Clew.Api.Contracts;
using Clew.Api.Extensions;
using Clew.Application.Abstractions;
using Clew.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Clew.Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class ProjectsController(IProjectsService projectsService) : ControllerBase
{
    [HttpPost("resolve/list")]
    public async Task<ActionResult<ProjectListDownloadUrlsDto>> ResolveProjectList(
        [FromBody] ProjectListResolveParametersDto projectListRequestDto, CancellationToken ct)
    {
        var projectListParameters = projectListRequestDto.ToDomainProjectListParams();

        try
        {
            var urls = await projectsService.GetModListDownloadUrlsAsync(projectListParameters, ct);

            return Ok(new ProjectListDownloadUrlsDto
            {
                InitialProjectsDownloadUrls = urls.InitialProjects,
                DependenciesDownloadUrls = urls.Dependencies
            });
        }
        catch (ProjectNotFoundException e)
        {
            return BadRequest(e.Message);
        }
        catch (ProjectVersionNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (AggregateException e)
        {
            throw e.InnerException ?? e;
        }
    }
}