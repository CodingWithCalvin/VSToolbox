using CodingWithCalvin.VSToolbox.Core.Models;

namespace CodingWithCalvin.VSToolbox.Core.Services;

public interface IVSLaunchService
{
    void LaunchInstance(VisualStudioInstance instance, string? rootSuffix = null);
    void LaunchInstanceWithSolution(VisualStudioInstance instance, string solutionPath, string? rootSuffix = null);
}
