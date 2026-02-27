using UtilitiesLib;

namespace SetupLib;

internal sealed class UtilitiesTypeActivator : ITypeActivator
{
    public object GetInstance(string fullyQualifiedTypeName)
    {
        return Utilities.GetInstance(fullyQualifiedTypeName);
    }
}
