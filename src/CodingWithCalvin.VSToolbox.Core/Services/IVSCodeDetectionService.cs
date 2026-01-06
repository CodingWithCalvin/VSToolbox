using CodingWithCalvin.VSToolbox.Core.Models;

namespace CodingWithCalvin.VSToolbox.Core.Services;

public interface IVSCodeDetectionService
{
    Task<IReadOnlyList<VisualStudioInstance>> GetInstalledInstancesAsync(CancellationToken cancellationToken = default);
}
