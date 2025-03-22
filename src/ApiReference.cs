using System.Xml;
using System.Reflection;
using ApiRef.Format;

namespace ApiRef;

/// <summary>
/// Gerador de referência para APIs.
/// </summary>
public class ApiReference
{
	private string a { get; set; }
	private Options options;
	private NestedNamespace namespaces;

	public ApiReference(Options options) => this.options = options;

	/// <summary>
	/// Gera a referência para uma dll, com ajuda do seu xml (se tiver).
	/// </summary>
	public void Generate()
	{
		namespaces = DLLImporter.Import(options.LibraryPath, options.FilterPublic);
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
	private string MakeMD(NestedNamespace current, XmlNode docs, MarkdownBuilder builder = null)
	{
		MarkdownBuilder md = builder ?? new MarkdownBuilder();
		XmlNode member = docs.SelectSingleNode(string.Format("member[@name=\"{0}\"]", current.DocName));
		string memberAsCode = string.Empty;
		int titleSize;

		if (current.Type != null)
		{
			Type baseType = current.Type.BaseType;
			memberAsCode = FormatTools.GetTypeAsCode(current.Type, baseType);
			titleSize = 2;

			md.InsertH1(FormatTools.TypeAsString(current.Type, current.Type).Replace("<", "\\<"));
		}
		else
		{
			titleSize = 3;

			if (current.MemberInfo.DeclaringType.IsEnum)
			{
				FieldInfo info = (FieldInfo)current.MemberInfo;
				string description = string.Empty;

				if (member != null)
				{
					MarkdownBuilder tempMD = new MarkdownBuilder();

					member.FormatSummary(tempMD, namespaces, options.RootPath, false);

					description = tempMD.ToString().Replace("\r", "").Replace("\n", "<br />");
				}

				md.InsertToTable(current.MemberInfo.Name, Convert.ChangeType(info.GetValue(null), Enum.GetUnderlyingType(info.FieldType)).ToString(), description);
			}
			else
			{
				memberAsCode = FormatTools.GetMemberAsCode(current.MemberInfo);

				md.InsertH2(FormatTools.GetMemberName(current.MemberInfo).Replace("<", "\\<"));
			}
		}

		if (memberAsCode.Length > 0) md.InsertCode(memberAsCode);

		if ((current.Type != null || !current.MemberInfo.DeclaringType.IsEnum) && member != null)
		{
			member.FormatSummary(md, namespaces, options.RootPath, true);

			if (current.Type != null && current.Type.IsEnum) md.InsertTableColumns(0, "Nome", "Valor", "Descrição");

			member.FormatParamsAndReturn(md, namespaces, options.RootPath, titleSize);
			member.FormatExceptions(md, namespaces, options.RootPath, titleSize);
			member.FormatRemarks(md, namespaces, options.RootPath, titleSize);
			member.FormatExample(md, namespaces, options.RootPath, titleSize);
		}

		foreach (KeyValuePair<string, NestedNamespace> pair in current.Child) MakeMD(pair.Value, docs, md);

		return md.ToString();
	}
}
