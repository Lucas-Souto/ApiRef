using System;
using System.Collections.Generic;
using System.Text;

namespace ApiRef.Core.Format
{
    /// <summary>
    /// Responsável pelo manuseio de um <see cref="StringBuilder"/> para markdown.
    /// </summary>
    public class MarkdownBuilder
    {
        private StringBuilder builder = new StringBuilder();

        /// <summary>
        /// Insere um H1.
        /// </summary>
        public void InsertH1(string text) => InsertH(text, 1);
        /// <summary>
        /// Insere um H2.
        /// </summary>
        public void InsertH2(string text) => InsertH(text, 2);
        /// <summary>
        /// Insere um H3.
        /// </summary>
        public void InsertH3(string text) => InsertH(text, 3);
        /// <summary>
        /// Insere um H(X).
        /// </summary>
        public void InsertH(string text, int number)
        {
            builder.Append('#', number);
            builder.Append(' ');
            builder.Append(text);
            builder.Append('\n');
        }

        /// <summary>
        /// Insere um texto.
        /// </summary>
        public void InsertText(string text) => builder.Append(text);

        /// <summary>
        /// Insere um texto em negrito.
        /// </summary>
        public void InsertBold(string text) => builder.AppendFormat("**{0}**", text);

        /// <summary>
        /// Insere um br e um \n (quebra de linha).
        /// </summary>
        public void InsertBr() => builder.Append("<br />\n");

        /// <summary>
        /// Insere um \n (quebra de linha).
        /// </summary>
        public void InsertNl() => builder.Append('\n');

        /// <summary>
        /// Insere um item de uma lista.
        /// </summary>
        public void InsertListItem(string text)
        {
            builder.AppendFormat("* {0}", text);
            builder.Append('\n');
        }

        /// <summary>
        /// Insere um bloco de código de uma linguagem.
        /// </summary>
        public void InsertCode(string code)
        {
            builder.Append("```csharp\n");
            builder.Append(code);
            builder.Append("\n```\n");
        }

        /// <summary>
        /// Insere uma lista de colunas para uma tabela.
        /// </summary>
        public void InsertTableColumns(int tabCount, params string[] columns)
        {
            Tab(tabCount);
            builder.Append('|');

            for (int i = 0; i < columns.Length; i++)
            {
                builder.Append(columns[i]);
                builder.Append('|');
            }

            builder.Append('\n');
            Tab(tabCount);
            builder.Append('|');

            for (int i = 0; i < columns.Length; i++)
            {
                builder.Append("---");
                builder.Append('|');
            }
            
            builder.Append('\n');
        }
        /// <summary>
        /// Insere uma lista de valores para cada coluna da tabela atual.
        /// </summary>
        public void InsertToTable(params string[] values)
        {
            builder.Append('|');

            for (int i = 0; i < values.Length; i++)
            {
                builder.Append(values[i]);
                builder.Append('|');
            }

            builder.Append('\n');
        }

        /// <summary>
        /// Insere um link.
        /// </summary>
        public void InsertLink(string text, string link) => string.Format("[{0}]({1})", text, link);

        /// <summary>
        /// Insere um número específico de tabulações.
        /// </summary>
        public void Tab(int count) => builder.Append('\t', count);

        /// <summary>
        /// Retorna o resultado atual do arquivo.
        /// </summary>
        public override string ToString() => builder.ToString();
    }
}