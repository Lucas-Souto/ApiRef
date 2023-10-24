using System;
using System.Collections.Generic;

namespace ApiRef.Core
{
    /// <summary>
    /// Representa a estrutura de um namespace.
    /// </summary>
    public class NestedNamespace
    {
        /// <summary>
        /// Tipo do namespace, se for uma definição em um namespace pai.
        /// </summary>
        public Type Type;
        public Dictionary<string, NestedNamespace> Child = new Dictionary<string, NestedNamespace>();
    }
}