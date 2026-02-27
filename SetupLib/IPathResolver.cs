namespace SetupLib;

internal interface IPathResolver
{
    string GetAbsoluteFilePath(string filePath, string baseDirPath);
}
