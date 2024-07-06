namespace Radiant.NET.Repository.Attributes
{
    /// <summary>
    /// Represents a custom attribute. This attribute is used to indicate which classes are to be used for database queries.
    /// It is a sealed class derived from the Attribute class, hence no other class can inherit from it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class QueryAttribute : Attribute
    {
    }
}