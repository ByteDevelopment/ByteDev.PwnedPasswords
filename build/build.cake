#addin "nuget:?package=Cake.Incubator&version=3.0.0"
#tool "nuget:?package=NUnit.Runners&version=2.6.4"
#tool "nuget:?package=GitVersion.CommandLine"
#load "ByteDev.Utilities.cake"

var nugetSources = new[] {"https://api.nuget.org/v3/index.json"};

var target = Argument("target", "Default");

var solutionFilePath = "../src/ByteDev.PwnedPasswords.sln";

var artifactsDirectory = Directory("../artifacts");
var nugetDirectory = artifactsDirectory + Directory("NuGet");
	
var configuration = GetBuildConfiguration();
	
Information("Configurtion: " + configuration);


Task("Clean")
    .Does(() =>
	{
		CleanDirectory(artifactsDirectory);
	
		CleanBinDirectories();
		CleanObjDirectories();
	});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
		var settings = new NuGetRestoreSettings
		{
			Source = nugetSources
		};

		NuGetRestore(solutionFilePath, settings);
    });

Task("Build")
	.IsDependentOn("Restore")
    .Does(() =>
	{	
		var settings = new DotNetCoreBuildSettings()
        {
            Configuration = configuration
        };

        DotNetCoreBuild(solutionFilePath, settings);
	});

Task("UnitTests")
    .IsDependentOn("Build")
    .Does(() =>
	{
		var settings = new DotNetCoreTestSettings()
		{
			Configuration = configuration,
			NoBuild = true
		};

		DotNetCoreUnitTests(settings);
	});
	
Task("IntegrationTests")
    .IsDependentOn("UnitTests")
    .Does(() =>
	{
		var settings = new DotNetCoreTestSettings()
		{
			Configuration = configuration,
			NoBuild = true
		};

		DotNetCoreIntTests(settings);
	});
	
Task("CreateNuGetPackages")
    .IsDependentOn("IntegrationTests")
    .Does(() =>
    {
		var nugetVersion = GetNuGetVersion();

        var settings = new DotNetCorePackSettings()
		{
			ArgumentCustomization = args => args.Append("/p:Version=" + nugetVersion),
			Configuration = configuration,
			OutputDirectory = nugetDirectory
		};
                
		DotNetCorePack("../src/ByteDev.PwnedPasswords/ByteDev.PwnedPasswords.csproj", settings);
    });

   
Task("Default")
    .IsDependentOn("CreateNuGetPackages");

RunTarget(target);