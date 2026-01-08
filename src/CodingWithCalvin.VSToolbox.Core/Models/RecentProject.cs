namespace CodingWithCalvin.VSToolbox.Core.Models;

public sealed class RecentProject
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required DateTimeOffset LastAccessed { get; init; }
    public bool IsSolution => Path.EndsWith(".sln", StringComparison.OrdinalIgnoreCase);
    public bool IsFolder => Directory.Exists(Path) && !File.Exists(Path);
    
    public string DisplayName => System.IO.Path.GetFileNameWithoutExtension(Name);
    
    public string ProjectType => Path switch
    {
        var p when p.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) => "Solution",
        var p when p.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) => "C# Project",
        var p when p.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase) => "VB.NET Project",
        var p when p.EndsWith(".fsproj", StringComparison.OrdinalIgnoreCase) => "F# Project",
        var p when p.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase) => "C++ Project",
        var p when Directory.Exists(p) => "Folder",
        _ => "Project"
    };

    public bool Exists => File.Exists(Path) || Directory.Exists(Path);
}
