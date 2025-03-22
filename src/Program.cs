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
			options.RootPath = Path.GetFileName(args[1]);
			options.OutputDirectory = Path.GetFullPath(Path.Combine(workingDir, Directory.GetParent(args[1]).FullName));
		}

		new ApiReference(options).Generate();
	}
}
