using System.Xml.Linq;

namespace LineConsole.Tests.Build;

/// <summary>
/// 專案參考的路徑大小寫要跟實際資料夾一模一樣：Windows 不分大小寫所以看不出來，Linux、Docker 會找不到。
/// （曾經發生：LineConsole.Server 參考寫成 Lineconsole.Infrastructure）
/// </summary>
public class ProjectReferenceTests
{
    public static TheoryData<string> Projects()
    {
        var data = new TheoryData<string>();
        foreach (var file in Directory.EnumerateFiles(RepoRoot.Path, "*.*proj", SearchOption.AllDirectories)
                     .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains("node_modules")))
            data.Add(Path.GetRelativePath(RepoRoot.Path, file));
        return data;
    }

    [Theory]
    [MemberData(nameof(Projects))]
    public void 參考路徑的大小寫跟實際檔案一致(string project)
    {
        var projectDir = Path.GetDirectoryName(Path.Combine(RepoRoot.Path, project))!;
        var references = XDocument.Load(Path.Combine(RepoRoot.Path, project))
            .Descendants("ProjectReference")
            .Select(e => (string?)e.Attribute("Include"))
            .OfType<string>();

        foreach (var include in references)
        {
            var current = projectDir;
            foreach (var part in include.Replace('\\', '/').Split('/'))
            {
                if (part == "..")
                {
                    current = Directory.GetParent(current)!.FullName;
                    continue;
                }
                var actual = Directory.EnumerateFileSystemEntries(current)
                    .Select(Path.GetFileName)
                    .FirstOrDefault(n => string.Equals(n, part, StringComparison.OrdinalIgnoreCase));
                Assert.True(actual is not null, $"{project}：找不到 {include}");
                Assert.True(actual == part, $"{project}：{include} 的「{part}」大小寫跟實際的「{actual}」不一樣");
                current = Path.Combine(current, actual!);
            }
        }
    }
}
