using Clew.Domain.Models;

namespace Clew.Domain.Exceptions;

public sealed class ProjectNotFoundException : Exception
{
    public GlobalProjectIdentifier ProjectIdentifier { get; }

    public ProjectNotFoundException(GlobalProjectIdentifier projectIdentifier) 
        : base($"Project with identifier: '{projectIdentifier}' was not found.")
    {
        ProjectIdentifier = projectIdentifier;
    }
    
    public ProjectNotFoundException(GlobalProjectIdentifier projectIdentifier, string message) : base(message)
    {
        ProjectIdentifier = projectIdentifier;
    }
}