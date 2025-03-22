namespace ApiRef;

class Program
{
	static void Main(string[] args)
	{
		if (args.Length < 1)
		{
			Console.WriteLine("Comando inválido! É necessário indicar o caminho da DLL!");

			return;
		}

		string workingDir = Directory.GetCurrentDirectory();
		Options options = new()
		{
			FilterPublic = true,
			OutputDirectory = workingDir,
			LibraryPath = Path.GetFullPath(Path.Combine(workingDir, args[0])),
			RootPath = "api"
		};

		if (args.Length > 1)
		{
			bool readingOutput = false;

			for (int i = 1; i < args.Length; i++)
			{
				switch (args[i])
				{
					case "--all": options.FilterPublic = false; break;
					case "-o": case "--output": readingOutput = true; break;
					default:
						if (readingOutput)
						{
							string tmp = Path.GetFullPath(Path.Combine(workingDir, args[i]));

							switch (tmp[tmp.Length -1])
							{
								case '/': case '\\': tmp = tmp.Substring(0, tmp.Length - 1); break;
							}

							options.RootPath = Path.GetFileName(tmp);
							options.OutputDirectory = tmp;
							readingOutput = false;
						}
						break;
				}
			}
		}

		new ApiReference(options).Generate();
	}
}
