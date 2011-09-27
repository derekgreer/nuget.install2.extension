require 'rubygems'
require 'albacore'
require 'configatron'

FRAMEWORK_PATH = "C:/Windows/Microsoft.NET/Framework/v4.0.30319/"
BUILD_PATH = File.expand_path('build')
MERGE_PATH = "#{BUILD_PATH}/merge"
TOOLS_PATH = File.expand_path('tools')
ARTIFACTS_PATH = File.expand_path('artifacts')
LIB_PATH = File.expand_path('lib')
SOLUTION = "src/nuget.install2.extension.sln"
COMPILE_TARGET = "Release"
FEEDS = ["https://go.microsoft.com/fwlink/?LinkID=206669" ]
load "VERSION.txt"
require './packages.rb'

task :default => ["build:all"]

namespace :build do

	task :all => [:clean, :dependencies, :compile, :package]

	assemblyinfo :versioning do |asm|
  		asm.output_file = "src/CommonAssemblyInfo.cs"
  		asm.version = "#{BUILD_VERSION}"
	end

	task :clean do
		rm_rf "#{BUILD_PATH}"
		rm_rf "#{ARTIFACTS_PATH}"
	end

	task :compile => [:versioning] do

		mkdir "#{BUILD_PATH}"
		sh "#{FRAMEWORK_PATH}msbuild.exe /p:Configuration=#{COMPILE_TARGET} #{SOLUTION}"
		copyOutputFiles "src/nuget.install2.extension/bin/#{COMPILE_TARGET}", "*.{exe,dll,pdb}", "#{BUILD_PATH}"
	end

	task :package do
		mkdir_p "#{ARTIFACTS_PATH}"
		rm Dir.glob("#{ARTIFACTS_PATH}/*.nupkg")
		FileList["packaging/nuget/*.nuspec"].each do |spec|
		sh "nuget pack #{spec} -o #{ARTIFACTS_PATH} -Version #{BUILD_VERSION} -BasePath ."
		end
	end

	task :dependencies do
		configatron.packages.each do | package |
			FEEDS.each do | feed | 
				!(File.exists?("#{LIB_PATH}/#{package[0]}")) and
					sh "nuget Install #{package[0]} -Version #{package[1]} -o #{LIB_PATH} -Source #{feed} -ExcludeVersion" do | cmd, results | cmd  end
			end
		end
	end

	def copyOutputFiles(fromDir, filePattern, outDir)
		Dir.glob(File.join(fromDir, filePattern)){|file| 		
			copy(file, outDir) if File.file?(file)
  		} 
	end
end
