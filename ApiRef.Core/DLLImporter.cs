using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace ApiRef.Core
{
    /// <summary>
    /// Responsável pela importação dinâmica de tipos de uma biblioteca.
    /// </summary>
    public static class DLLImporter
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        private static readonly Dictionary<MemberTypes, int> MemberOrder = new Dictionary<MemberTypes, int>
        {
            { MemberTypes.Constructor, 0 },
            { MemberTypes.Field, 1 },
            { MemberTypes.Property, 2 },
            { MemberTypes.Event, 3 },
            { MemberTypes.Method, 4 },
            { MemberTypes.NestedType, 5 }
        };

        /// <summary>
        /// Retorna os namespaces de uma biblioteca.
        /// </summary>
        /// <param name="filterPublic">Indica que só os tipos públicos da biblioteca serão retornados.</param>
        public static NestedNamespace Import(string path, bool filterPublic)
        {
            Assembly dll = Assembly.LoadFile(path);
            Type[] types;
            NestedNamespace namespaces = new NestedNamespace(string.Empty);

            if (filterPublic) types = dll.GetExportedTypes();
            else types = dll.GetTypes();

            foreach (Type type in types) AddNestedNamespace(namespaces, type, type.FullName.Split('.', '+'), filterPublic);

            return namespaces;
        }

        /// <summary>
        /// Adiciona o tipo num namespace aninhado.
        /// </summary>
        private static void AddNestedNamespace(NestedNamespace currentSpace, Type type, string[] fullNameSpace, bool filterPublic, int index = 0)
        {
            if (index == fullNameSpace.Length - 1)
            {
                NestedNamespace newSpace = new NestedNamespace(Namespace(currentSpace.FullName, type.Name), type);
                IEnumerable<MemberInfo> members = type.GetMembers(Flags).OrderBy((m) => MemberOrder[m.MemberType]);

                currentSpace.Child.Add(fullNameSpace[fullNameSpace.Length - 1], newSpace);

                foreach (MemberInfo member in members)
                {
                    if (type.IsEnum && (member.Name == "value__" || member.MemberType != MemberTypes.Field)) continue;
                    else if (!Filter(member, filterPublic)) continue;

                    string result = member.Stringfy();

                    if (result.Length > 0) newSpace.Child[result] = new NestedNamespace(Namespace(newSpace.FullName, result), member);
                }
            }
            else
            {
                NestedNamespace nextSpace;

                if (!currentSpace.Child.TryGetValue(fullNameSpace[index], out nextSpace))
                {
                    nextSpace = new NestedNamespace(Namespace(currentSpace.FullName, fullNameSpace[index]));

                    currentSpace.Child.Add(fullNameSpace[index], nextSpace);
                }

                AddNestedNamespace(nextSpace, type, fullNameSpace, filterPublic, index + 1);
            }
        }

        /// <summary>
        /// Transforma o <see cref="MemberInfo"/> em uma string.
        /// </summary>
        private static string Stringfy(this MemberInfo member)
        {
            StringBuilder builder = new StringBuilder();
            Dictionary<string, int> declaringGenerics = member.DeclaringType.GetGenericArguments().GetGenericMap();

            if (member is MethodBase method)
            {
                builder.Append(method.Name.Replace('.', '#'));

                Dictionary<string, int> methodGenerics = !method.IsConstructor ? method.GetGenericArguments().GetGenericMap() : null;
                ParameterInfo[] parameters = method.GetParameters();

                if (methodGenerics != null && methodGenerics.Count > 0) builder.AppendFormat("``{0}", methodGenerics.Count);

                if (parameters.Length > 0)
                {
                    builder.Append('(');

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        builder.Append(parameters[i].ParameterType.DocName(declaringGenerics, methodGenerics));
                        builder.Append(',');
                    }

                    builder.Remove(builder.Length - 1, 1);
                    builder.Append(')');
                }

                if (method.IsStatic && method.Name.StartsWith("op_")) builder.AppendFormat("~{0}", ((MethodInfo)method).ReturnType.FullName);
                else if (member is MethodInfo methodInfo && methodInfo.IsSpecialName) return string.Empty;
            }
            else if (member.MemberType != MemberTypes.TypeInfo && member.MemberType != MemberTypes.NestedType)
            {
                if (member.Name.EndsWith(">k__BackingField")) return string.Empty;

                builder.Append(member.Name);

                if (member is PropertyInfo property)
                {
                    ParameterInfo[] parameters = property.GetIndexParameters();

                    if (parameters.Length > 0)
                    {
                        builder.Append('(');

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            builder.Append(parameters[i].ParameterType.DocName(declaringGenerics, null));
                            builder.Append(',');
                        }

                        builder.Remove(builder.Length - 1, 1);

                        builder.Append(')');
                    }
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Retorna um dicionário com o nome dos tipos e o seu index.
        /// </summary>
        private static Dictionary<string, int> GetGenericMap(this Type[] generics)
        {
            Dictionary<string, int> map = new Dictionary<string, int>();

            if (generics != null && generics.Length > 0)
            {
                for (int i = 0; i < generics.Length; i++) map.Add(generics[i].Name, i);
            }

            return map;
        }

        /// <summary>
        /// Retorna o nome de parâmetros de forma adequada.
        /// </summary>
        private static string DocName(this Type type, Dictionary<string, int> declaringGenerics, Dictionary<string, int> methodGenerics)
        {
            StringBuilder builder = new StringBuilder();

            if (type.IsGenericType)
            {
                builder.Append(Namespace(type.Namespace, type.Name.Split('`')[0]));

                Type[] arguments = type.GetGenericArguments();

                builder.Append('{');

                for (int i = 0; i < arguments.Length; i++)
                {
                    builder.Append(arguments[i].DocName(declaringGenerics, methodGenerics));
                    builder.Append(',');
                }

                builder.Remove(builder.Length - 1, 1);
                builder.Append('}');
            }
            else if (type.IsGenericParameter)
            {
                if (methodGenerics != null && methodGenerics.TryGetValue(type.Name, out int methodIndex)) builder.AppendFormat("``{0}", methodIndex);
                else if (declaringGenerics != null && declaringGenerics.TryGetValue(type.Name, out int declaringIndex)) builder.AppendFormat("`{0}", declaringIndex);
                else throw new Exception(string.Format("Tipo \"{0}\" não consta!", type.Name));
            }
            else if (type.IsByRef)
            {
                builder.Append(type.GetElementType().DocName(declaringGenerics, methodGenerics));
                builder.Append('@');
            }
            else if (type.IsArray)
            {
                builder.Append(type.GetElementType().DocName(declaringGenerics, methodGenerics));
                builder.Append('[');

                int rank = type.GetArrayRank();

                builder.Append(',', rank - 1);
                builder.Append(']');
            }
            else builder.Append(Namespace(type.Namespace, type.Name));

            return builder.ToString();
        }

        /// <summary>
        /// Formata um namespace composto por um ou mais namespaces.
        /// </summary>
        private static string Namespace(string previous, string current)
        {
            if (previous.Length > 0) return string.Format("{0}.{1}", previous, current);

            return current;
        }

        /// <summary>
        /// Se <see cref="Options.FilterPublic"/>, só retorna true para public e protected;
        /// </summary>
        private static bool Filter(MemberInfo info, bool filterPublic)
        {
            if (filterPublic)
            {
                switch (info.MemberType)
                {
                    case MemberTypes.Field:
                        FieldInfo field = (FieldInfo)info;

                        return field.IsPublic || field.IsFamily;
                    case MemberTypes.Property:
                        PropertyInfo property = (PropertyInfo)info;
                        MethodInfo get = property.GetMethod, set = property.SetMethod;

                        return (get != null && (get.IsPublic || get.IsFamily)) || (set != null && (set.IsPublic || set.IsFamily));
                    case MemberTypes.Method: case MemberTypes.Constructor:
                        MethodBase method = (MethodBase)info;

                        return method.IsPublic || method.IsFamily;
                    case MemberTypes.Event:
                        EventInfo @event = (EventInfo)info;

                        return @event.AddMethod.IsPublic || @event.AddMethod.IsFamily;
                }
            }

            return true;
        }
    }
}