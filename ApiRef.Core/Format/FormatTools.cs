using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace ApiRef.Core.Format
{
    public static class FormatTools
    {
        private static Dictionary<string, string> NativeTypes = new Dictionary<string, string>
        {
            { "System.String", "string" }, { "System.Char", "char" },
            { "System.Byte", "byte" }, { "System.SByte", "sbyte" },
            { "System.Int16", "short" }, { "System.UInt16", "ushort" },
            { "System.Int32", "int" }, { "System.UInt32", "uint" },
            { "System.Int64", "long" }, { "System.UInt64", "ulong" },
            { "System.Single", "float" }, { "System.Double", "double" },
            { "System.Boolean", "bool" },
            { "System.Void", "void" }, { "System.Object", "object" }
        };
        private static Dictionary<string, string> OperatorString = new Dictionary<string, string>
        {
            { "op_Addition", "+" }, { "op_Subtraction", "-" }, { "op_Multiply", "*" }, { "op_Division", "/" }, { "op_Modulus", "%" },
            { "op_BitwiseAnd", "&" }, { "op_BitwiseOr", "|" }, { "op_ExclusiveOr", "^" }, { "op_OnesComplement", "~" },
            { "op_Equality", "==" }, { "op_Inequality", "!=" }, { "op_LessThan", "<" }, { "op_GreaterThan", ">" }, { "op_LessThanOrEqual", "<=" }, { "op_GreaterThanOrEqual", ">=" },
            { "op_LeftShift", "<<" }, { "op_RightShift", ">>" },
            { "op_UnaryNegation", "-" }, { "op_UnaryPlus", "+" }
        };

        /// <summary>
        /// Retorna o tipo de acesso como string.
        /// </summary>
        public static string GetAccessType(bool isPublic, bool isFamily, bool isAssembly)
        {
            string result;

            if (isPublic) result = "public";
            else if (isFamily)
            {
                result = "protected";

                if (isAssembly) result += " internal";
            }
            else if (isAssembly) result = "internal";
            else result = "private";

            return result;
        }

        /// <summary>
        /// Retorna a palavra chave de um tipo.
        /// </summary>
        public static string GetTypeDefinition(bool isClass, bool isInterface, bool isEnum)
        {
            if (isClass) return "class";
            else if (isInterface) return "interface";
            else if (isEnum) return "enum";

            return "struct";
        }

        /// <summary>
        /// Retorna uma representação do tipo, como um pedaço de código.
        /// </summary>
        public static string GetTypeAsCode(Type type, Type inherit)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(GetAccessType(type.IsPublic || type.IsNestedPublic, false, type.IsVisible));
            
            if (type.IsAbstract && type.IsSealed) builder.Append(" static");
            else if (!type.IsInterface && type.IsAbstract) builder.Append(" abstract");
            else if (type.IsClass && type.IsSealed) builder.Append(" sealed");

            builder.AppendFormat(" {0} ", GetTypeDefinition(type.IsClass, type.IsInterface, type.IsEnum));
            builder.Append(TypeAsString(type, type));

            if (!type.IsValueType && inherit != null && inherit != typeof(object)) builder.AppendFormat(" : {0}", TypeAsString(inherit));

            return builder.ToString();
        }

        /// <summary>
        /// Formata o tipo para ser uma representação em código.
        /// </summary>
        public static string TypeAsString(Type type, Type declaring = null, bool isOut = false)
        {
            StringBuilder builder = new StringBuilder();

            if (type.IsByRef)
            {
                string key = isOut ? "out" : "ref";

                builder.AppendFormat("{0} {1}", key, TypeAsString(type.GetElementType(), declaring));
            }
            else if (type.IsGenericType)
            {
                Type genericDefinition = type.GetGenericTypeDefinition();
                Type[] generics = type.GetGenericArguments();

                if (genericDefinition == typeof(Nullable<>)) builder.AppendFormat("{0}?", TypeAsString(generics[0]), declaring);
                else
                {
                    if (declaring != null && declaring.IsGenericType)
                    {
                        string name = (genericDefinition == declaring.GetGenericTypeDefinition() ? genericDefinition.Name : genericDefinition.FullName);

                        builder.AppendFormat("{0}<", name.Split('`')[0]);
                    }
                    else builder.AppendFormat("{0}<", genericDefinition.FullName.Split('`')[0]);

                    for (int i = 0; i < generics.Length; i++) builder.AppendFormat("{0},", TypeAsString(generics[i], declaring));

                    builder.Remove(builder.Length - 1, 1);
                    builder.Append('>');
                }
            }
            else if (type.IsArray)
            {
                builder.AppendFormat("{0}[", TypeAsString(type.GetElementType(), declaring));
                builder.Append(',', type.GetArrayRank() - 1);
                builder.Append(']');
            }
            else if (type.IsGenericParameter) builder.Append(type.Name);
            else
            {
                if (NativeTypes.TryGetValue(type.FullName, out string native)) builder.Append(native);
                else if (type == declaring || (declaring != null && type.DeclaringType == declaring)) builder.Append(type.Name);
                else builder.Append(type.FullName);
            }

            if (type.IsPointer) builder.Append('*');

            return builder.ToString();
        }

        /// <summary>
        /// Retorna o nome do membro, formatado.
        /// </summary>
        public static string GetMemberName(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Constructor: return TypeAsString(member.DeclaringType, member.DeclaringType);
                case MemberTypes.Method:
                    MethodInfo method = (MethodInfo)member;

                    if (method.IsSpecialName && method.Name.StartsWith("op_"))
                    {
                        if (method.Name == "op_Implicit") return "implicit operator";

                        return string.Format("operator {0}", OperatorString[method.Name]);
                    }

                    return method.Name;
                case MemberTypes.Property:
                    PropertyInfo property = (PropertyInfo)member;
                    StringBuilder builder = new StringBuilder();
                    ParameterInfo[] parameters = property.GetIndexParameters();

                    if (parameters.Length > 0)
                    {
                        builder.Append(TypeAsString(member.DeclaringType, member.DeclaringType));
                        builder.Append('[');

                        for (int i = 0; i < parameters.Length; i++) builder.AppendFormat("{0} {1}, ", TypeAsString(parameters[i].ParameterType, member.DeclaringType), parameters[i].Name);

                        builder.Remove(builder.Length - 2, 2);
                        builder.Append(']');
                    }
                    else builder.Append(property.Name);

                    return builder.ToString();
                default: return member.Name;
            }
        }

        /// <summary>
        /// Retorna uma representação do membro como um pedaço de código.
        /// </summary>
        public static string GetMemberAsCode(MemberInfo info)
        {
            StringBuilder builder = new StringBuilder();
            Type declaringType = info.DeclaringType;

            switch (info.MemberType)
            {
                case MemberTypes.Field: FieldAsCode((FieldInfo)info, declaringType, builder); break;
                case MemberTypes.Property: PropertyAsCode((PropertyInfo)info, declaringType, builder); break;
                case MemberTypes.Constructor: case MemberTypes.Method: MethodAsCode((MethodBase)info, declaringType, builder); break;
                case MemberTypes.Event: EventAsCode((EventInfo)info, declaringType, builder); break;
            }

            return builder.ToString();
        }
        private static void FieldAsCode(FieldInfo field, Type declaringType, StringBuilder builder)
        {
            builder.Append(GetAccessType(field.IsPublic, field.IsFamily, field.IsAssembly));

            if (field.IsLiteral) builder.Append(" const");
            else if (field.IsStatic) builder.Append(" static");

            if (field.IsInitOnly) builder.Append(" readonly");

            builder.AppendFormat(" {0} {1};", TypeAsString(field.FieldType, declaringType), field.Name);
        }
        private static void PropertyAsCode(PropertyInfo property, Type declaringType, StringBuilder builder)
        {
            MethodInfo get = property.GetMethod, set = property.SetMethod;
            bool isGetPublic = get != null && get.IsPublic, isGetFamily = get != null && get.IsFamily, isGetAssembly = get != null && get.IsAssembly;
            bool isSetPublic = set != null && set.IsPublic, isSetFamily = set != null && set.IsFamily, isSetAssembly = set != null && set.IsAssembly;

            builder.Append(GetAccessType(isGetPublic, isGetFamily, isGetAssembly));

            if ((get != null && get.IsStatic) || (set != null && set.IsStatic)) builder.Append(" static");
            else if (!declaringType.IsInterface && (get != null && get.IsAbstract) || (set != null && set.IsAbstract)) builder.Append(" abstract");
            else if (!declaringType.IsInterface && (get != null && get.IsVirtual) || (set != null && set.IsVirtual)) builder.Append(" virtual");

            string type = TypeAsString(property.PropertyType, declaringType);

            builder.AppendFormat(" {0} ", type);
            builder.Append(GetMemberName(property));

            builder.Append(" { ");

            if (get != null)
            {
                if (isGetPublic) builder.Append("get; ");
                else builder.AppendFormat("{0} get; ", GetAccessType(isGetPublic, isGetFamily, isGetAssembly));
            }

            if (set != null)
            {
                if (isSetPublic) builder.Append("set; ");
                else builder.AppendFormat("{0} set; ", GetAccessType(isSetPublic, isSetFamily, isSetAssembly));
            }

            builder.Append("} ");
        }
        private static void MethodAsCode(MethodBase method, Type declaringType, StringBuilder builder)
        {
            builder.Append(GetAccessType(method.IsPublic, method.IsFamily, method.IsAssembly));

            if (method.IsStatic) builder.Append(" static");
            else if (!declaringType.IsInterface && method.IsAbstract) builder.Append(" abstract");
            else if (!declaringType.IsInterface && method.IsVirtual) builder.Append(" virtual");

            if (method.IsConstructor) builder.AppendFormat(" {0}", GetMemberName(method));
            else
            {
                builder.AppendFormat(" {0} {1}", TypeAsString(((MethodInfo)method).ReturnType, declaringType), GetMemberName(method));

                Type[] generics = method.GetGenericArguments();

                if (generics.Length > 0)
                {
                    builder.Append('<');

                    for (int i = 0; i < generics.Length; i++) builder.AppendFormat("{0},", TypeAsString(generics[i], declaringType));

                    builder.Remove(builder.Length - 1, 1);
                    builder.Append('>');
                }
            }

            builder.Append('(');

            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++) builder.AppendFormat("{0} {1}, ", TypeAsString(parameters[i].ParameterType, declaringType, parameters[i].IsOut), parameters[i].Name);

                builder.Remove(builder.Length - 2, 2);
            }

            builder.Append(") { }");
        }
        private static void EventAsCode(EventInfo @event, Type declaringType, StringBuilder builder)
        {
            builder.Append(GetAccessType(@event.AddMethod.IsPublic, @event.AddMethod.IsFamily, @event.AddMethod.IsAssembly));

            if (@event.AddMethod.IsStatic) builder.Append(" static");

            builder.Append(" event ");
            builder.Append(TypeAsString(@event.EventHandlerType, declaringType));
            builder.AppendFormat(" {0};", @event.Name);
        }
    }
}