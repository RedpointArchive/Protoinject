using System;

namespace Protoinject
{
    /// <summary>
    /// This attribute allows you to populate the following private fields on
    /// a class when it's initialized.  This initialization only occurs for
    /// base classes:
    /// <list type="bullet">
    /// <item>private readonly INode _node</item>
    /// <item>private readonly IHierarchy _hierarchy</item>
    /// </list>
    /// <para>
    /// We allow only these fields to use property injection, because knowing an
    /// object's place in the hierarchy is critical, and demanding that the injection
    /// of INode propagate to all derived classes is unnecessary since we know the
    /// dependency on INode can always be satisfied.
    /// </para>
    /// <para>
    /// For a class using this attribute, you should implement the constructors like
    /// this:
    /// </para>
    /// <code>
    /// protected MyClass()
    /// {
    ///     // Constructor which relies on fields being set.
    /// }
    /// 
    /// public MyClass(IHierarchy hierarchy, INode node)
    /// {
    ///     // You must set the fields in the public class.
    ///     _hierarchy = hierarchy;
    ///     _node = node;
    /// }
    /// </code>
    /// <para>
    /// If you don't wish to allow the class to be constructed publically, you can
    /// just omit the public constructor, as having only protected constructors will
    /// ensure that the fields are always set (since it will always be used as a base
    /// class).
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class InjectFieldsForBaseObjectInProtectedConstructorAttribute : Attribute
    {
    }
}
