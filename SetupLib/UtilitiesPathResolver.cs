using UtilitiesLib;

namespace SetupLib;

internal sealed class UtilitiesPathResolver : IPathResolver
{
    public string GetAbsoluteFilePath(string filePath, string baseDirPath)
    {
        return Utilities.GetAbsoluteFilePath(filePath, baseDirPath);
    }
}
