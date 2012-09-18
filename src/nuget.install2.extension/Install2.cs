using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.RegularExpressions;
using NuGet;
using NuGet.Commands;

namespace nuget.install2.extension
{
	[Command(typeof (Install2Resources), "install2", "Description", MinArgs = 0, MaxArgs = 1,
		UsageSummaryResourceName = "UsageSummary", UsageDescriptionResourceName = "UsageDescription")]
	public class Install2 : Command
	{
		readonly IPackageRepositoryFactory _repositoryFactory;
		readonly IPackageSourceProvider _sourceProvider;


		readonly List<string> _sources = new List<string>();

		[ImportingConstructor]
		public Install2(IPackageRepositoryFactory repositoryFactory, IPackageSourceProvider sourceProvider)
		{
			_repositoryFactory = repositoryFactory;
			_sourceProvider = sourceProvider;
		}

		[Option(typeof (Install2Resources), "SourceDescription", AltName = "src")]
		public ICollection<string> Source
		{
			get { return _sources; }
		}

		[Option(typeof (Install2Resources), "CommandVersionDescription")]
		public bool CommandVersion { get; set; }

		[Option(typeof (Install2Resources), "ExcludeVersionDescription")]
		public bool ExcludeVersion { get; set; }

		[Option(typeof (Install2Resources), "OutputDirectoryDescription")]
		public string OutputDirectory { get; set; }

		public override void ExecuteCommand()
		{
			if (CommandVersion)
			{
				Console.WriteLine("{0} Version: {1}", GetType().Assembly.GetName().Name,
				                  GetType().Assembly.GetName().Version.ToString());
				return;
			}

			string file = Arguments[0];

			if (Source.Count == 0)
				Source.Add("http://packages.nuget.org/v1/FeedService.svc/");

			ProcessPackagesFile(file);
		}

		void ProcessPackagesFile(string dependencyFile)
		{
			var packages = new List<Package>(GetPackages(dependencyFile));

			foreach (Package package in packages)
			{
				RetrievePackage(package);
			}
		}

		void RetrievePackage(Package package)
		{
			var command = new InstallCommand(_repositoryFactory, _sourceProvider);
			command.Arguments.Add(package.Id);
			command.OutputDirectory = OutputDirectory;
			command.Console = base.Console;
			command.ExcludeVersion = ExcludeVersion;

			foreach (string source in Source)
			{
				command.Source.Add(source);
			}

			// Any version qualifies as a range (e.g. 1.0 --> 1.0 ≤ x)
			if (!string.IsNullOrEmpty(package.Version))
			{
				SemanticVersion semanticVersion = null;

				if (SemanticVersion.TryParse(package.Version, out semanticVersion))
				{
					foreach (string source in Source)
					{
						IPackageRepository repository = _repositoryFactory.CreateRepository(source);
						IPackage specificPackage = repository.FindPackage(package.Id, semanticVersion);

						if (specificPackage != null)
						{
							command.Version = semanticVersion.ToString();
							break;
						}
					}
				}
			}
			command.ExecuteCommand();
		}


		IEnumerable<Package> GetPackages(string dependencyFile)
		{
			var dependencies = new List<Package>();

			using (StreamReader sr = File.OpenText(dependencyFile))
			{
				string line = "";
				while ((line = sr.ReadLine()) != null)
				{
					string[] fields = Regex.Split(line, @"\s+");

					string version = null;

					if (fields.Length == 2)
						version = fields[1];

					dependencies.Add(new Package(fields[0], version));
				}
			}
			return dependencies;
		}
	}
}