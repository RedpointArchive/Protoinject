﻿using System;

namespace Protoinject
{
    /// <summary>
    ///     This is an attribute added by the factory generator; you should never add
    ///     it manually.
    /// </summary>
    public class GeneratedFactoryAttribute : Attribute
    {
        public string FullTypeName { get; }

        public GeneratedFactoryAttribute(string fullTypeName)
        {
            FullTypeName = fullTypeName;
        }
    }
}