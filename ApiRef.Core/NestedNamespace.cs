using System;
using System.Collections.Generic;
using System.Reflection;

namespace ApiRef.Core
{
    /// <summary>
    /// Representa a estrutura de um namespace.
    /// </summary>
    public class NestedNamespace
    {
        /// <summary>
        /// Tipo do namespace, se for um tipo.
        /// </summary>
        public Type Type;
        /// <summary>
        /// Informações de um membro, se for um membro.
        /// </summary>
        public MemberInfo MemberInfo;
        public Dictionary<string, NestedNamespace> Child = new Dictionary<string, NestedNamespace>();
    }
}