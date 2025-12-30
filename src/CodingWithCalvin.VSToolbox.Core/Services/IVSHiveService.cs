using CodingWithCalvin.VSToolbox.Core.Models;

namespace CodingWithCalvin.VSToolbox.Core.Services;

public interface IVSHiveService
{
    IReadOnlyList<VisualStudioHive> GetHivesForInstance(VisualStudioInstance instance);
}
