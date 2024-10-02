namespace Radiant.Repository.Attributes
{
    /// <summary>
    /// The 'RepositoryAttribute' class, derived from 'Attribute', is a custom attribute class. It may be used to annotate repository classes.
    /// This class is 'sealed' which prevents further derivation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RepositoryAttribute : Attribute
    {
    }
}