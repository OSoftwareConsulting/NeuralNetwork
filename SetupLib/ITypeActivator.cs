namespace SetupLib;

internal interface ITypeActivator
{
    object GetInstance(string fullyQualifiedTypeName);
}
