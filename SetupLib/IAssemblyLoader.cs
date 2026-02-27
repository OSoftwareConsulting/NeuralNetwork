namespace SetupLib;

internal interface IAssemblyLoader
{
    void LoadAssemblies(string[] assemblyPaths, string baseDirPath);
}
