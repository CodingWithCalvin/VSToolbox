using CodingWithCalvin.VSToolbox.Core.Models;

namespace CodingWithCalvin.VSToolbox.Core.Services;

public interface IVSDetectionService
{
    Task<IReadOnlyList<VisualStudioInstance>> GetInstalledInstancesAsync(CancellationToken cancellationToken = default);
    bool IsVSWhereAvailable();
}
