using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using ApiRef.Core.Format;

namespace ApiRef.Core
{
    /// <summary>
    /// Gerador de referência para APIs.
    /// </summary>
    public class ApiReference
    {
        private Options options;

        public ApiReference(Options options) => this.options = options;

        /// <summary>
        /// Gera a referência para uma dll, com base no seu xml, se tiver.
        /// </summary>
        public void Generate()
        {
            NestedNamespace namespaces = DLLImporter.Import(options.LibraryPath, options.FilterPublic);
            XmlNode members = null;
            string xmlPath = Path.Combine(Path.GetDirectoryName(options.LibraryPath), Path.GetFileNameWithoutExtension(options.LibraryPath) + ".xml");

            if (File.Exists(xmlPath))
            {
                XmlDocument docs = new XmlDocument();

                docs.Load(xmlPath);

                members = docs.SelectSingleNode("doc/members");
            }
            
            Directory.CreateDirectory(options.OutputDirectory);
            ReferenceTo(options.OutputDirectory, namespaces, members);
        }

        /// <summary>
        /// Cria as referências de uma pasta/namespace.
        /// </summary>
        private void ReferenceTo(string output, NestedNamespace current, XmlNode members)
        {
            if (current.IsNamespace)
            {
                Directory.CreateDirectory(output);

                foreach (KeyValuePair<string, NestedNamespace> sub in current.Child)
                {
                    if (sub.Value.IsNamespace) ReferenceTo(Path.Combine(output, sub.Key), sub.Value, members);
                    else File.WriteAllText(Path.Combine(output, sub.Key + ".md"), MakeMD(sub.Value, members));
                }
            }
        }

        /// <summary>
        /// Gera o conteúdo markdown de um namespace.
        /// </summary>
        private string MakeMD(NestedNamespace current, XmlNode members, MarkdownBuilder builder = null)
        {
            MarkdownBuilder md = builder ?? new MarkdownBuilder();
            XmlNode member = members.SelectSingleNode(string.Format("member[@name=\"{0}\"]", current.DocName));
            string memberAsCode = string.Empty;

            if (current.Type != null)
            {
                Type baseType = current.Type.BaseType;
                memberAsCode = FormatTools.GetTypeAsCode(current.Type, baseType);

                md.InsertH1(FormatTools.TypeAsString(current.Type, current.Type));

                if (member != null) member.Format(md, 2);
            }
            else
            {
                if (current.MemberInfo.DeclaringType.IsEnum)
                {
                    FieldInfo info = (FieldInfo)current.MemberInfo;
                    string description = string.Empty;

                    if (member != null) description = "PLACEHOLDER";

                    md.InsertToTable(current.MemberInfo.Name, Convert.ChangeType(info.GetValue(null), Enum.GetUnderlyingType(info.FieldType)).ToString(), description);
                }
                else
                {
                    memberAsCode = FormatTools.GetMemberAsCode(current.MemberInfo);

                    if (current.MemberInfo.MemberType != MemberTypes.Constructor) md.InsertH2(current.MemberInfo.Name);
                    else md.InsertH2(FormatTools.TypeAsString(current.MemberInfo.DeclaringType, current.MemberInfo.DeclaringType));

                    if (member != null) member.Format(md, 3);
                }
            }

            if (memberAsCode.Length > 0) md.InsertCode(memberAsCode);

            if (current.Type != null && current.Type.IsEnum) md.InsertTableColumns(0, "Nome", "Valor", "Descrição");

            foreach (KeyValuePair<string, NestedNamespace> pair in current.Child) MakeMD(pair.Value, members, md);

            return md.ToString();
        }
    }
}