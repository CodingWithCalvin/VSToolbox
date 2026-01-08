using CodingWithCalvin.VSToolbox.Core.Models;

namespace CodingWithCalvin.VSToolbox.Core.Services;

public interface IRecentProjectsService
{
    IReadOnlyList<RecentProject> GetRecentProjects(VisualStudioInstance instance, int maxCount = 10);
}
