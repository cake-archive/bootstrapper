//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// VERSION
//////////////////////////////////////////////////////////////////////

var releaseNotes = ParseReleaseNotes("./ReleaseNotes.md");
var version = releaseNotes.Version.ToString();

//////////////////////////////////////////////////////////////////////
// DIRECTORIES
//////////////////////////////////////////////////////////////////////

var buildDirectory = "./build/" + "v" + version;
var buildBinDirectory = buildDirectory + "/bin";
var buildInstallerDirectory = buildDirectory + "/installer";
var buildCandleDirectory = buildDirectory + "/installer/wixobj";
var chocolateyRoot = buildDirectory + "/chocolatey";
var chocolateyToolsDirectory = chocolateyRoot + "/tools";

var binDirectory = "./src/Bootstrapper/Cake.Bootstrapper/bin/";
var outputDirectory = binDirectory + configuration;

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories(new[] { buildDirectory, buildBinDirectory,
    	buildInstallerDirectory, buildCandleDirectory, 
        chocolateyRoot, chocolateyToolsDirectory, outputDirectory });
});

Task("Update-Versions")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var file = "./src/SolutionInfo.cs";
    CreateAssemblyInfo(file, new AssemblyInfoSettings {
        Product = "Cake",
        Version = version,
        FileVersion = version,
        InformationalVersion = version,
        Copyright = "Copyright (c) Patrik Svensson 2014"
    });
}); 

Task("Restore-NuGet-Packages")
    .IsDependentOn("Update-Versions")
    .Does(() =>
{
    NuGetRestore("./src/Bootstrapper/Cake.Bootstrapper.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild("./src/Bootstrapper/Cake.Bootstrapper.sln", settings =>
        settings.SetConfiguration(configuration)
            .UseToolVersion(MSBuildToolVersion.NET45));
});

Task("Copy-Files")
    .IsDependentOn("Build")
    .Does(() =>
{   
    CopyFileToDirectory(outputDirectory + "/Cake.Bootstrapper.dll", buildBinDirectory);
    CopyFileToDirectory(outputDirectory + "/Cake.Bootstrapper.psd1", buildBinDirectory);
    CopyFileToDirectory(outputDirectory + "/Cake.Core.dll", buildBinDirectory);
    CopyFileToDirectory(outputDirectory + "/Autofac.dll", buildBinDirectory);
    CopyFileToDirectory(outputDirectory + "/NuGet.Core.dll", buildBinDirectory);
    CopyFileToDirectory("./res/cake.ico", buildBinDirectory);    
    CopyFileToDirectory("./res/scripts/build.cake", buildBinDirectory);
    CopyFileToDirectory("./res/scripts/build.ps1", buildBinDirectory);
});

Task("Build-Installer")
    .IsDependentOn("Copy-Files")
    .Does(() =>
{
    // Invoke Candle
    var files = GetFiles("./src/Installer/**/*.wxs");
    WiXCandle(files, new CandleSettings {
        OutputDirectory = buildCandleDirectory,
        Extensions = new List<string> { "WixUtilExtension" },
        Defines = new Dictionary<string, string> {
            { "BinDir", buildBinDirectory },
            { "BuildVersion", "1.0.0" }
        }
    });

    // Invoke Light
    var objFiles = GetFiles(buildCandleDirectory + "/*.wixobj");
    WiXLight(objFiles, new LightSettings {
        OutputFile = buildInstallerDirectory + "/Cake-Bootstrapper-v" + version + ".msi",
        Extensions = new List<string> { "WixUIExtension", "WixUtilExtension", "WixNetFxExtension" }
    });
});

Task("Build-Chocolatey")
    .IsDependentOn("Build-Installer")
    .Does(() =>
{
    // Create chocolateyInstall.ps1 in chocolatey tools output.
    var url = "https://github.com/cake-build/bootstrapper/releases/download/v" + version + "/Cake-Bootstrapper-v" + version + ".zip";
    string text = File.ReadAllText("./src/Chocolatey/tools/chocolateyInstall.ps1");
    text = text.Replace("%DOWNLOAD_URL%", url);
    File.WriteAllText(chocolateyToolsDirectory + "/chocolateyInstall.ps1", text);

    NuGetPack("./src/Chocolatey/Cake.Bootstrapper.nuspec", new NuGetPackSettings {
        Version = version,
        ReleaseNotes = releaseNotes.Notes.ToArray(),
        BasePath = chocolateyRoot,
        OutputDirectory = chocolateyRoot,
        NoPackageAnalysis = true,
    });
});

Task("Default")
    .IsDependentOn("Build-Chocolatey");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);