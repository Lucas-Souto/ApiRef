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
        public static void FormatSummary(this XmlNode member, XmlNode docs, MarkdownBuilder builder, string rootDirectory)
        {
            XmlNode summary = member.SelectSingleNode("summary");

            if (summary != null)
            {
                FormatText(summary, docs, builder, rootDirectory);
                builder.InsertNl();
            }
        }

        /// <summary>
        /// Formata os parâmetros, os genéricos e o retorno do membro.
        /// </summary>
        public static void FormatParamsAndReturn(this XmlNode member, XmlNode docs, MarkdownBuilder builder, string rootDirectory, int titleSize)
        {
            for (int i = 0; i < member.ChildNodes.Count; i++)
            {
                XmlNode node = member.ChildNodes[i];

                if (node.Name == "param" || node.Name == "typeparam")
                {
                    builder.InsertBold(node.Attributes[0].Value);
                    builder.InsertText(": ");
                    FormatText(node, docs, builder, rootDirectory);
                    builder.InsertBr();
                }
                else if (node.Name == "returns")
                {
                    builder.InsertBold("Retorna: ");
                    FormatText(node, docs, builder, rootDirectory);
                    builder.InsertBr();
                }
            }
        }

        /// <summary>
        /// Formata as observações do membro.
        /// </summary>
        public static void FormatRemarks(this XmlNode member, XmlNode docs, MarkdownBuilder builder, string rootDirectory, int titleSize)
        {
            XmlNode remarks = member.SelectSingleNode("remarks");

            if (remarks != null)
            {
                builder.InsertH("Observações", titleSize);
                FormatText(remarks, docs, builder, rootDirectory);
                builder.InsertNl();
            }
        }

        /// <summary>
        /// Formata os exemplos do membro.
        /// </summary>
        public static void FormatExample(this XmlNode member, XmlNode docs, MarkdownBuilder builder, string rootDirectory, int titleSize)
        {
            XmlNode example = member.SelectSingleNode("example");

            if (example != null)
            {
                builder.InsertH("Exemplos", titleSize);
                FormatText(example, docs, builder, rootDirectory);
                builder.InsertNl();
            }
        }

        /// <summary>
        /// Formata as exceções do membro.
        /// </summary>
        public static void FormatExceptions(this XmlNode member, XmlNode docs, MarkdownBuilder builder, string rootDirectory, int titleSize)
        {
            for (int i = 0; i < member.ChildNodes.Count; i++)
            {
                XmlNode node = member.ChildNodes[i];

                if (node.Name == "exception")
                {
                    builder.InsertBold(node.Attributes[0].Value.Substring(2));
                    builder.InsertText(": ");
                    FormatText(node, docs, builder, rootDirectory);
                    builder.InsertBr();
                }
            }
        }

        private static void FormatText(XmlNode node, XmlNode docs, MarkdownBuilder builder, string rootDirectory)
        {

        }
    }
}