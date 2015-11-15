namespace Protoinject.Test
{
    using System;
    using Protoinject.Example;

    public interface IGenericFactory<T1, T3> : IGenerateFactory where T1 : class, IPlayer where T3 : IWorld
    {
        IGeneric<T1, T2> CreateGeneric<T2>(T1 a, T2 b) where T2 : IComparable<string>;
    }
}