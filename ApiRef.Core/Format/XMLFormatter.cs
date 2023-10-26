using System;
using System.Xml;

namespace ApiRef.Core.Format
{
    /// <summary>
    /// Responsável pela formatação de arquivos XML em Markdown.
    /// </summary>
    public static class XMLFormatter
    {
        /// <summary>
        /// Formata o sumário do membro.
        /// </summary>
        public static void FormatSummary(this XmlNode member, XmlNode docs, MarkdownBuilder builder, NestedNamespace namespaces, string rootDirectory, bool breakLine)
        {
            XmlNode summary = member.SelectSingleNode("summary");

            if (summary != null)
            {
                FormatText(summary, docs, builder, namespaces, rootDirectory);

                if (breakLine) builder.InsertBr();
            }
        }

        /// <summary>
        /// Formata os parâmetros, os genéricos e o retorno do membro.
        /// </summary>
        public static void FormatParamsAndReturn(this XmlNode member, XmlNode docs, MarkdownBuilder builder, NestedNamespace namespaces, string rootDirectory, int titleSize)
        {
            for (int i = 0; i < member.ChildNodes.Count; i++)
            {
                XmlNode node = member.ChildNodes[i];

                if (node.Name == "param" || node.Name == "typeparam")
                {
                    builder.InsertBold(node.Attributes[0].Value);
                    builder.InsertText(": ");
                    FormatText(node, docs, builder, namespaces, rootDirectory);
                    builder.InsertBr();
                }
                else if (node.Name == "returns")
                {
                    builder.InsertBold("Retorna: ");
                    FormatText(node, docs, builder, namespaces, rootDirectory);
                    builder.InsertBr();
                }
            }
        }

        /// <summary>
        /// Formata as observações do membro.
        /// </summary>
        public static void FormatRemarks(this XmlNode member, XmlNode docs, MarkdownBuilder builder, NestedNamespace namespaces, string rootDirectory, int titleSize)
        {
            XmlNode remarks = member.SelectSingleNode("remarks");

            if (remarks != null)
            {
                builder.InsertH("Observações", titleSize);
                FormatText(remarks, docs, builder, namespaces, rootDirectory);
                builder.InsertBr();
            }
        }

        /// <summary>
        /// Formata os exemplos do membro.
        /// </summary>
        public static void FormatExample(this XmlNode member, XmlNode docs, MarkdownBuilder builder, NestedNamespace namespaces, string rootDirectory, int titleSize)
        {
            XmlNode example = member.SelectSingleNode("example");

            if (example != null)
            {
                builder.InsertH("Exemplos", titleSize);
                FormatText(example, docs, builder, namespaces, rootDirectory);
                builder.InsertBr();
            }
        }

        /// <summary>
        /// Formata as exceções do membro.
        /// </summary>
        public static void FormatExceptions(this XmlNode member, XmlNode docs, MarkdownBuilder builder, NestedNamespace namespaces, string rootDirectory, int titleSize)
        {
            for (int i = 0; i < member.ChildNodes.Count; i++)
            {
                XmlNode node = member.ChildNodes[i];

                if (node.Name == "exception")
                {
                    builder.InsertBold(node.Attributes[0].Value.Substring(2));
                    builder.InsertText(": ");
                    FormatText(node, docs, builder, namespaces, rootDirectory);
                    builder.InsertBr();
                }
            }
        }

        private static void FormatText(XmlNode node, XmlNode docs, MarkdownBuilder builder, NestedNamespace namespaces, string rootDirectory)
        {
            if (node.ChildNodes.Count > 0)
            {
                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                    XmlNode child = node.ChildNodes[i];

                    switch (child.Name)
                    {
                        case "c": builder.InsertInlineCode(child.Value); break;
                        case "code": builder.InsertCode(child.Value); break;
                        case "para":
                            FormatText(child, docs, builder, namespaces, rootDirectory);
                            builder.InsertBr();
                            break;
                        case "#text": builder.InsertText(child.Value.Trim('\r', '\n', '\t', ' ')); break;
                        case "see": case "seealso":
                            if (child.Name == "seealso")
                            {
                                builder.InsertBold("Veja também");
                                builder.InsertText(": ");
                            }

                            if (child.Attributes.Count > 0)
                            {
                                XmlAttribute attribute = child.Attributes[0];

                                builder.InsertText(" ");

                                if (attribute.Name == "href") builder.InsertLink(child.InnerText.Length > 0 ? child.InnerText : attribute.Value, attribute.Value);
                                else InsertRefLink(attribute.Value, namespaces, builder, rootDirectory);
                            }
                            break;
                    }
                }
            }
            else builder.InsertText(node.InnerText.Trim());
        }
        
        private static void InsertRefLink(string fullNamespace, NestedNamespace namespaces, MarkdownBuilder builder, string rootDirectory)
        {
            if (fullNamespace.StartsWith("N:"))
            {
                builder.InsertText("[Não encontrado!]");

                return;
            }

            fullNamespace = fullNamespace.Substring(2);
            string[] splitted = fullNamespace.Split('.');
            string text = string.Empty;

            FindMember(namespaces, splitted, out NestedNamespace type, out NestedNamespace last);

            if (last == namespaces || type == namespaces) builder.InsertText("[Não encontrado!]");
            else
            {
                if (last.Type != null) text = FormatTools.TypeAsString(last.Type, type.Type);
                else if (last.MemberInfo != null) text = string.Format("{0}.{1}", FormatTools.TypeAsString(last.MemberInfo.DeclaringType, type.Type), FormatTools.GetMemberName(last.MemberInfo));

                builder.InsertLink(text, string.Format("{0}/{1}", rootDirectory, type.Type.FullName.Replace('.', '/')), text.Replace("<", "\\<"));
            }
        }
        private static void FindMember(NestedNamespace namespaces, string[] splittedNamespace, out NestedNamespace type, out NestedNamespace last)
        {
            type = namespaces;
            last = namespaces;;

            for (int i = 0; i < splittedNamespace.Length; i++)
            {
                if (last.Child.TryGetValue(splittedNamespace[i], out NestedNamespace newLast))
                {
                    last = newLast;

                    if (last.Type != null && type.Type == null) type = last;
                }
            }
        }
    }
}