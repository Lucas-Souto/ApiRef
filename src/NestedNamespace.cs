using System.Reflection;

namespace ApiRef;

/// <summary>
/// Representa a estrutura de um namespace.
/// </summary>
public class NestedNamespace
{
	/// <summary>
	/// <see cref="FullName"/> como um XML member.
	/// </summary>
	public string DocName
	{
		get
		{
			char id = '?';

			if (Type != null) id = 'T';
			else if (MemberInfo != null)
			{
				switch (MemberInfo.MemberType)
				{
					case MemberTypes.Field: id = 'F'; break;
					case MemberTypes.Property: id = 'P'; break;
					case MemberTypes.Event: id = 'E'; break;
					case MemberTypes.Constructor: case MemberTypes.Method: id = 'M'; break;
				}
			}
			else return FullName;

			return string.Format("{0}:{1}", id, FullName);
		}
	}
	/// <summary>
	/// Diz se este <see cref="NestedNamespace"/> representa um namespace real.
	/// </summary>
	public readonly bool IsNamespace;
	/// <summary>
	/// Nome completo deste namespace.
	/// </summary>
	public readonly string FullName;
	/// <summary>
	/// Tipo do namespace, se for um tipo.
	/// </summary>
	public readonly Type Type;
	/// <summary>
	/// Informações de um membro, se for um membro.
	/// </summary>
	public readonly MemberInfo MemberInfo;
	public Dictionary<string, NestedNamespace> Child;

	public NestedNamespace(string fullName) : this(fullName, null, null) { }
	public NestedNamespace(string fullName, Type type) : this(fullName, type, null) { }
	public NestedNamespace(string fullName, MemberInfo memberInfo) : this(fullName, null, memberInfo) { }
	private NestedNamespace(string fullName, Type type, MemberInfo memberInfo)
	{
		IsNamespace = type == null && memberInfo == null;
		FullName = fullName;
		Type = type;
		MemberInfo = memberInfo;
		Child = new Dictionary<string, NestedNamespace>();
	}
}
