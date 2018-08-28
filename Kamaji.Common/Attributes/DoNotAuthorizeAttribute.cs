namespace Kamaji.Common
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DoNotAuthorizeAttribute : Attribute
    {

    }
}
