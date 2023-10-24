using System;
using System.Reflection;

namespace ApiRef.Core
{
    /// <summary>
    /// Responsável pela importação dinâmica de tipos de uma biblioteca.
    /// </summary>
    public static class DLLImporter
    {
        /// <summary>
        /// Retorna os namespaces de uma biblioteca.
        /// </summary>
        /// <param name="filterPublic">Indica que só os tipos públicos da biblioteca serão retornados.</param>
        public static NestedNamespace Import(string path, bool filterPublic)
        {
            Assembly dll = Assembly.LoadFile(path);
            Type[] types;
            NestedNamespace namespaces = new NestedNamespace();

            if (filterPublic) types = dll.GetExportedTypes();
            else types = dll.GetTypes();

            foreach (Type type in types) AddNestedNamespace(namespaces, type, type.FullName.Split('.', '+'));

            return namespaces;
        }

        /// <summary>
        /// Adiciona o tipo num namespace aninhado.
        /// </summary>
        private static void AddNestedNamespace(NestedNamespace currentSpace, Type type, string[] fullNameSpace, int index = 0)
        {
            if (index == fullNameSpace.Length - 1)
            {
                NestedNamespace newSpace = new NestedNamespace();
                newSpace.Type = type;

                currentSpace.Child.Add(fullNameSpace[fullNameSpace.Length - 1], newSpace);
            }
            else
            {
                NestedNamespace nextSpace;

                if (!currentSpace.Child.TryGetValue(fullNameSpace[index], out nextSpace))
                {
                    nextSpace = new NestedNamespace();

                    currentSpace.Child.Add(fullNameSpace[index], nextSpace);
                }

                AddNestedNamespace(nextSpace, type, fullNameSpace, index + 1);
            }
        }
    }
}