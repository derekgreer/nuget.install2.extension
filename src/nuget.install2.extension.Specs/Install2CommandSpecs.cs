using System;
using System.IO;
using System.Linq;
using Machine.Specifications;
using Moq;
using NuGet;
using It = Machine.Specifications.It;

namespace nuget.install2.extension.Specs
{
    public class when_called_with_an_outputdir_switch
    {
        static Install2 _subject;

        Cleanup after = () =>
            {
                File.Delete("dependencies.txt");
                Directory.Delete("OutputDir", true);
            };

        Establish context = () =>
            {
                using (var sr = new StreamWriter("dependencies.txt"))
                {
                    sr.WriteLine("NHibernate 3.2.0.4000");
                }

                _subject = new Install2(PackageRepositoryFactory.Default, new PackageSourceProvider(Settings.LoadDefaultSettings()));
                _subject.Arguments.Add("dependencies.txt");
                _subject.OutputDirectory = "OutputDir";
            };

        Because of = () => _subject.ExecuteCommand();

        It should_install_packages_to_the_specified_output = () => Directory.Exists("OutputDir").ShouldBeTrue();
    }

    public class when_called_with_an_excludeversion_switch
    {
        static Install2 _subject;

        Cleanup after = () =>
            {
                File.Delete("dependencies.txt");
                Directory.Delete("NHibernate", true);
            };

        Establish context = () =>
            {
                using (var sr = new StreamWriter("dependencies.txt"))
                {
                    sr.WriteLine("NHibernate 3.2.0.4000");
                }
                
                _subject = new Install2(PackageRepositoryFactory.Default, new PackageSourceProvider(Settings.LoadDefaultSettings()));
                _subject.Arguments.Add("dependencies.txt");
                _subject.ExcludeVersion = true;

            };

        Because of = () => _subject.ExecuteCommand();

        It should_not_include_version_in_the_output_folder_name = () => Directory.Exists("NHibernate").ShouldBeTrue();
    }
}