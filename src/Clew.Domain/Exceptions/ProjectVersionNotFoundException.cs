using Clew.Domain.Models;

namespace Clew.Domain.Exceptions;

public sealed class ProjectVersionNotFoundException : Exception
{
    public ProjectResolveParameters Parameters { get; }
    
    public ProjectVersionNotFoundException(ProjectResolveParameters projectParameters) 
        : base($"No version found for project '{projectParameters.Identifier}' " +
               $"with parameters:\n{projectParameters.ProjectVersionFilters}")
    {
        Parameters = projectParameters;
    }
    
    public ProjectVersionNotFoundException(ProjectResolveParameters projectParameters, string message) : base(message)
    {
        Parameters = projectParameters;
    }
}