//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var releaseNotes = ParseReleaseNotes("./ReleaseNotes.md");
var version = releaseNotes.Version.ToString();

var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
var isLocalBuild = !isRunningOnAppVeyor;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

var buildNumber = AppVeyor.Environment.Build.Number;
var semVersion = isLocalBuild ? version : (version + string.Concat("-build-", buildNumber));

//////////////////////////////////////////////////////////////////////
// DIRECTORIES
//////////////////////////////////////////////////////////////////////

var buildDirectory = "./build/v" + semVersion;
var buildBinDirectory = buildDirectory + "/bin";
var testResultDirectory = buildDirectory + "/test-results";
var buildInstallerDirectory = buildDirectory + "/installer";
var buildCandleDirectory = buildDirectory + "/installer/wixobj";
var chocolateyRoot = buildDirectory + "/chocolatey";
var chocolateyToolsDirectory = chocolateyRoot + "/tools";

var sourceDirectory = "./src/Bootstrapper";
var binDirectory = sourceDirectory + "/Cake.Bootstrapper/bin/";
var outputDirectory = binDirectory + configuration;

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories(new[] { buildDirectory, buildBinDirectory,
        buildInstallerDirectory, buildCandleDirectory, testResultDirectory,
        chocolateyRoot, chocolateyToolsDirectory, outputDirectory });
});

Task("Update-Versions")
    .IsDependentOn("Clean")
    .Does(() =>
{
    // Update the shared assembly info.
    var file = "./src/SolutionInfo.cs";
    CreateAssemblyInfo(file, new AssemblyInfoSettings {
        Product = "Cake",
        Version = version,
        FileVersion = version,
        InformationalVersion = semVersion,
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

Task("Run-Unit-Tests")
    .Description("Runs unit tests.")
    .IsDependentOn("Build")
    .Does(() =>
{
    // Run unit tests.
    XUnit2("./src/**/bin/" + configuration + "/*.Tests.dll", new XUnit2Settings {
        OutputDirectory = testResultDirectory,
        XmlReportV1 = true
    }); 
});

Task("Copy-Files")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{   
    // Copy binaries.
    CopyFileToDirectory(outputDirectory + "/Cake.Bootstrapper.dll", buildBinDirectory);    
    CopyFileToDirectory(outputDirectory + "/Cake.Core.dll", buildBinDirectory);
    CopyFileToDirectory(outputDirectory + "/Autofac.dll", buildBinDirectory);
    CopyFileToDirectory(outputDirectory + "/NuGet.Core.dll", buildBinDirectory);

    // Copy PowerShell module manifest.
    CopyFileToDirectory(sourceDirectory + "/Cake.Bootstrapper.psd1", buildBinDirectory);

    // Copy icon (used by add/remove programs).
    CopyFileToDirectory("./res/cake.ico", buildBinDirectory);    

    // Copy scripts.
    CopyFileToDirectory("./res/scripts/build.cake", buildBinDirectory);
    CopyFileToDirectory("./res/scripts/build.ps1", buildBinDirectory);

    // Copy AppVeyor configuration.
    CopyFileToDirectory("./res/appveyor.yml", buildBinDirectory);
});

Task("Set-Module-Manifest-Version")
    .IsDependentOn("Copy-Files")
    .Does(() =>
{
    // Replace module version.
    var path = buildBinDirectory + "/Cake.Bootstrapper.psd1";
    string text = System.IO.File.ReadAllText(path);
    text = text.Replace("%MODULE_VERSION%", version);
    System.IO.File.WriteAllText(path, text);    
});

Task("Build-Installer")
    .IsDependentOn("Set-Module-Manifest-Version")
    .Does(() =>
{
    // Invoke Candle (WiX compiler).
    var files = GetFiles("./src/Installer/**/*.wxs");
    WiXCandle(files, new CandleSettings {
        OutputDirectory = buildCandleDirectory,
        Extensions = new List<string> { "WixUtilExtension" },
        Defines = new Dictionary<string, string> {
            { "BinDir", buildBinDirectory },
            { "BuildVersion", version }
        }
    });

    // Invoke Light (WiX linker).
    var objFiles = GetFiles(buildCandleDirectory + "/*.wixobj");
    WiXLight(objFiles, new LightSettings {
        OutputFile = buildInstallerDirectory + "/Cake-Bootstrapper-v" + semVersion + ".msi",
        Extensions = new List<string> { "WixUIExtension", "WixUtilExtension", "WixNetFxExtension" }
    });
});

Task("Build-Chocolatey")
    .IsDependentOn("Build-Installer")
    .Does(() =>
{
    // Create chocolateyInstall.ps1 in chocolatey tools output.
    var url = "https://github.com/cake-build/bootstrapper/releases/download/v" + semVersion + "/Cake-Bootstrapper-v" + semVersion + ".msi";
    string text = System.IO.File.ReadAllText("./src/Chocolatey/tools/chocolateyInstall.ps1");
    text = text.Replace("%DOWNLOAD_URL%", url);
    System.IO.File.WriteAllText(chocolateyToolsDirectory + "/chocolateyInstall.ps1", text);

    // Create the nuget package.
    NuGetPack("./src/Chocolatey/Cake.Bootstrapper.nuspec", new NuGetPackSettings {
        Version = semVersion,
        ReleaseNotes = releaseNotes.Notes.ToArray(),
        BasePath = chocolateyRoot,
        OutputDirectory = chocolateyRoot,
        NoPackageAnalysis = true,
    });
});

Task("Publish-To-MyGet")
    .WithCriteria(() => isRunningOnAppVeyor)
    .WithCriteria(() => !isPullRequest)
    .IsDependentOn("Build-Chocolatey")
    .Does(() =>
{
    // Resolve the API key.
    var apiKey = EnvironmentVariable("MYGET_API_KEY");
    if(string.IsNullOrEmpty(apiKey)) {
        throw new InvalidOperationException("Could not resolve MyGet API key.");
    }

    // Get the path to the package.
    var package = chocolateyRoot + "/cake-bootstrapper." + semVersion + ".nupkg";

    // Push the package.
    NuGetPush(package, new NuGetPushSettings {
        Source = "https://www.myget.org/F/cake-bootstrapper/api/v2/package",
        ApiKey = apiKey
    });    
});

Task("Set-AppVeyor-Build-Version")
    .WithCriteria(() => isRunningOnAppVeyor)
    .Does(() =>
{
    StartProcess("appveyor", new ProcessSettings {
        Arguments = string.Concat("UpdateBuild -Version \"", semVersion, "\"")
    });
});

Task("Upload-AppVeyor-Artifact")
    .IsDependentOn("Build-Installer")
    .WithCriteria(() => isRunningOnAppVeyor)
    .Does(() =>
{
	var artifact = new FilePath(buildInstallerDirectory + "/Cake-Bootstrapper-v" + semVersion + ".msi");
    Information("Uploading AppVeyor artifact {0}...", artifact.GetFilename());
    AppVeyor.UploadArtifact(artifact);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build-Chocolatey");

Task("AppVeyor")
    .WithCriteria(() => isRunningOnAppVeyor)
    .IsDependentOn("Publish-To-MyGet")
    .IsDependentOn("Set-AppVeyor-Build-Version")
    .IsDependentOn("Upload-AppVeyor-Artifact");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);