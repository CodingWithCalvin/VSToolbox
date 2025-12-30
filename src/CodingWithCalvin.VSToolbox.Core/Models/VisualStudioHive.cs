namespace CodingWithCalvin.VSToolbox.Core.Models;

public sealed class VisualStudioHive
{
    public required string Name { get; init; }
    public required string RootSuffix { get; init; }
    public required string DataPath { get; init; }
    public VisualStudioInstance? ParentInstance { get; set; }

    public bool IsExperimental => RootSuffix.Equals("Exp", StringComparison.OrdinalIgnoreCase);
    public bool IsDefault => string.IsNullOrEmpty(RootSuffix);

    public string DisplayName => IsDefault ? "Default" : IsExperimental ? "Experimental" : RootSuffix;
}
