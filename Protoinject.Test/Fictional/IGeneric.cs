namespace Protoinject.Test
{
    using System;
    using Protoinject.Example;

    public interface IGeneric<T1, T2>
        where T1 : class, IPlayer where T2 : IComparable<string>
    {
    }
}