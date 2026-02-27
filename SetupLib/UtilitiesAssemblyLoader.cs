using UtilitiesLib;

namespace SetupLib;

internal sealed class UtilitiesAssemblyLoader : IAssemblyLoader
{
    public void LoadAssemblies(string[] assemblyPaths, string baseDirPath)
    {
        Utilities.LoadAssemblies(assemblyPaths, baseDirPath);
    }
}
