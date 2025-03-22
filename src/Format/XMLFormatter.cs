using System.Xml;

namespace ApiRef.Format;

/// <summary>
/// Responsável pela formatação de arquivos XML em Markdown.
/// </summary>
public static class XMLFormatter
{
	/// <summary>
	/// Formata o sumário do membro.
	/// </summary>
	public static void FormatSummary(this XmlNode member, MarkdownBuilder builder, NestedNamespace namespaces, string rootDirectory, bool breakLine)
	{
		XmlNode summary = member.SelectSingleNode("summary");

		if (summary != null)
		{
			FormatText(summary, builder, namespaces, rootDirectory);

			if (breakLine) builder.InsertBr();
		}
	}

	/// <summary>
	/// Formata os parâmetros, os genéricos e o retorno do membro.
	/// </summary>
	public static void FormatParamsAndReturn(this XmlNode member, MarkdownBuilder builder, NestedNamespace namespaces, string rootDirectory, int titleSize)
	{
		for (int i = 0; i < member.ChildNodes.Count; i++)
		{
			XmlNode node = member.ChildNodes[i];

			if (node.Name == "param" || node.Name == "typeparam")
			{
				builder.InsertBold(node.Attributes[0].Value);
				builder.InsertText(": ");
				FormatText(node, builder, namespaces, rootDirectory);
				builder.InsertBr();
			}
			else if (node.Name == "returns")
			{
				builder.InsertBold("Retorna");
				builder.InsertText(": ");
				FormatText(node, builder, namespaces, rootDirectory);
				builder.InsertBr();
			}
		}
	}

	/// <summary>
	/// Formata as observações do membro.
	/// </summary>
	public static void FormatRemarks(this XmlNode member, MarkdownBuilder builder, NestedNamespace namespaces, string rootDirectory, int titleSize)
	{
		XmlNode remarks = member.SelectSingleNode("remarks");

		if (remarks != null)
		{
			builder.InsertH("Observações", titleSize);
			FormatText(remarks, builder, namespaces, rootDirectory);
			builder.InsertBr();
		}
	}

	/// <summary>
	/// Formata os exemplos do membro.
	/// </summary>
	public static void FormatExample(this XmlNode member, MarkdownBuilder builder, NestedNamespace namespaces, string rootDirectory, int titleSize)
	{
		XmlNode example = member.SelectSingleNode("example");

		if (example != null)
		{
			builder.InsertH("Exemplos", titleSize);
			FormatText(example, builder, namespaces, rootDirectory);
			builder.InsertBr();
		}
	}

	/// <summary>
	/// Formata as exceções do membro.
	/// </summary>
	public static void FormatExceptions(this XmlNode member, MarkdownBuilder builder, NestedNamespace namespaces, string rootDirectory, int titleSize)
	{
		bool titled = false;

		for (int i = 0; i < member.ChildNodes.Count; i++)
		{
			XmlNode node = member.ChildNodes[i];

			if (node.Name == "exception")
			{
				if (!titled)
				{
					titled = true;

					builder.InsertH("Exceções", titleSize);
				}

				builder.InsertBold(node.Attributes[0].Value.Substring(2));
				builder.InsertText(": ");
				FormatText(node, builder, namespaces, rootDirectory);
				builder.InsertBr();
			}
		}
	}

	private static void FormatText(XmlNode node, MarkdownBuilder builder, NestedNamespace namespaces, string rootDirectory)
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
						if (i > 0 && node.ChildNodes[i - 1].Name != child.Name) builder.InsertBr();

						FormatText(child, builder, namespaces, rootDirectory);
						builder.InsertBr();
						break;
					case "#text": case "value":
						builder.InsertText(child.Value.Trim('\r', '\n', '\t', ' '));
						break;
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

							builder.InsertText(" ");
						}
						break;
					case "paramref": case "typeparamref":
						if (child.Attributes.Count > 0)
						{
							XmlAttribute attribute = child.Attributes[0];

							builder.InsertText(" ");
							builder.InsertInlineCode(attribute.Value);
							builder.InsertText(" ");
						}
						break;
				}
			}
		}
		else if (node.NodeType == XmlNodeType.Text) builder.InsertText(node.Value.Trim('\r', '\n', '\t', ' '));
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
		int methodStart = fullNamespace.IndexOf('(');
		string splitString = methodStart >= 0 ? fullNamespace.Substring(0, methodStart) : fullNamespace;
		string[] splitted = splitString.Split('.');

		if (splitted.Length > 0 && methodStart >= 0) splitted[splitted.Length - 1] += fullNamespace.Substring(methodStart);

		string text = string.Empty, title = string.Empty;
		int typeIndex = FindMember(namespaces, splitted, out NestedNamespace type);

		if (type == namespaces) builder.InsertText("[Não encontrado!]");
		else
		{
			NestedNamespace member = typeIndex < splitted.Length - 1 ? type.Child[splitted[typeIndex + 1]] : null;
			text = FormatTools.TypeAsString(type.Type.DeclaringType != null ? type.Type.DeclaringType : type.Type, type.Type);

			if (member != null)
			{
				string memberName = FormatTools.GetMemberName(member.MemberInfo ?? member.Type).Replace("<", "\\<");
				text += string.Format(".{0}", memberName);
				title = memberName;
			}
			else title = text;

			builder.InsertLink(text, string.Format("{0}/{1}.md", rootDirectory, type.Type.FullName.Replace('.', '/')), title.Replace("<", "\\<"));
		}
	}
	private static int FindMember(NestedNamespace namespaces, string[] splittedNamespace, out NestedNamespace type)
	{
		type = namespaces;
		NestedNamespace last = namespaces;
		int typeIndex = 0;

		for (int i = 0; i < splittedNamespace.Length; i++)
		{
			if (last.Child.TryGetValue(splittedNamespace[i], out NestedNamespace newLast))
			{
				last = newLast;

				if (last.Type != null && type.Type == null)
				{
					type = last;
					typeIndex = i;
				}
			}
		}

		return typeIndex;
	}
}
