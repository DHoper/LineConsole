namespace LineConsole.Tests;

/// <summary>從測試的輸出資料夾往上找，找到放 LineConsole.sln 的那一層</summary>
internal static class RepoRoot
{
    public static string Path { get; } = Find();

    private static string Find()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(System.IO.Path.Combine(dir.FullName, "LineConsole.sln")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("找不到 LineConsole.sln");
    }
}
