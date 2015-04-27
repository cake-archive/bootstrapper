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

var buildNumber = AppVeyor.Environment.Build.Number;
var buildSuffix = BuildSystem.IsLocalBuild ? "-local" : string.Concat("-build-", buildNumber);
var buildVersion = version + buildSuffix;

//////////////////////////////////////////////////////////////////////
// DIRECTORIES
//////////////////////////////////////////////////////////////////////

var sourcePath = Directory("./src/Bootstrapper");
var outputPath = sourcePath + Directory("Cake.Bootstrapper/bin") + Directory(configuration);

var buildPath = Directory("./build/v" + buildVersion);
var buildBinPath = buildPath + Directory("bin");
var buildInstallerPath = buildPath + Directory("installer");
var buildCandlePath = buildPath + Directory("installer/wixobj");
var testResultPath = buildPath + Directory("test-results");
var chocolateyRootPath = buildPath + Directory("chocolatey");
var chocolateyToolsPath = chocolateyRootPath + Directory("tools");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] { buildPath, buildBinPath,
        buildInstallerPath, buildCandlePath, testResultPath,
        chocolateyRootPath, chocolateyToolsPath, outputPath });
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
        InformationalVersion = buildVersion,
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
        OutputDirectory = testResultPath,
        XmlReportV1 = true
    }); 
});

Task("Copy-Files")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{   
    var files = new FilePath[] 
    {
        // Binaries
        outputPath + File("Cake.Bootstrapper.dll"),
        outputPath + File("Cake.Core.dll"),
        outputPath + File("Autofac.dll"),
        outputPath + File("NuGet.Core.dll"),

        // PowerShell module manifest
        sourcePath + File("Cake.Bootstrapper.psd1"),

        // Icon used by add/remove programs
        "./res/cake.ico",

        // Scripts
        "./res/scripts/build.cake",
        "./res/scripts/build.ps1",

        // AppVeyor configuration
        "./res/appveyor.yml"
    };

    // Copy all files to the bin directory.
    CopyFiles(files, buildBinPath);
});

Task("Set-Module-Manifest-Version")
    .IsDependentOn("Copy-Files")
    .Does(() =>
{
    // Replace module version.
    var path = buildBinPath + File("Cake.Bootstrapper.psd1");
    TransformTextFile(path)
        .WithToken("MODULE_VERSION", version)
        .Save(path); 
});

Task("Candle")
    .IsDependentOn("Set-Module-Manifest-Version")
    .Does(() =>
{
    // Invoke Candle (WiX compiler).
    var files = GetFiles("./src/Installer/**/*.wxs");
    WiXCandle(files, new CandleSettings {
        OutputDirectory = buildCandlePath,
        Extensions = new List<string> { "WixUtilExtension" },
        Defines = new Dictionary<string, string> {
            { "BinDir", buildBinPath },
            { "BuildVersion", version }
        }
    });
});

Task("Light")
    .IsDependentOn("Candle")
    .Does(() =>
{
    // Invoke Light (WiX linker).
    var objFiles = GetFiles(buildCandlePath.Path + "/*.wixobj");
    WiXLight(objFiles, new LightSettings {
        OutputFile = buildInstallerPath.Path + "/Cake-Bootstrapper-v" + buildVersion + ".msi",
        Extensions = new List<string> { "WixUIExtension", "WixUtilExtension", "WixNetFxExtension" }
    });    
});

Task("Build-Installer")
    .IsDependentOn("Candle")
    .IsDependentOn("Light");

Task("Build-Chocolatey")
    .IsDependentOn("Build-Installer")
    .Does(() =>
{
    // Create chocolateyInstall.ps1 in chocolatey tools output.
    var url = "https://github.com/cake-build/bootstrapper/releases/download/v" + buildVersion + "/Cake-Bootstrapper-v" + buildVersion + ".msi";
    TransformTextFile("./src/Chocolatey/tools/chocolateyInstall.ps1")
        .WithToken("DOWNLOAD_URL", url)
        .Save(chocolateyToolsPath + File("chocolateyInstall.ps1"));

    // Create the nuget package.
    NuGetPack("./src/Chocolatey/Cake.Bootstrapper.nuspec", new NuGetPackSettings {
        Version = buildVersion,
        ReleaseNotes = releaseNotes.Notes.ToArray(),
        BasePath = chocolateyRootPath,
        OutputDirectory = chocolateyRootPath,
        NoPackageAnalysis = true,
    });
});

Task("Publish-To-MyGet")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .WithCriteria(() => !AppVeyor.Environment.PullRequest.IsPullRequest)
    .IsDependentOn("Build-Chocolatey")
    .Does(() =>
{
    // Resolve the API key.
    var apiKey = EnvironmentVariable("MYGET_API_KEY");
    if(string.IsNullOrEmpty(apiKey)) {
        throw new InvalidOperationException("Could not resolve MyGet API key.");
    }

    // Get the path to the package.
    var package = chocolateyRootPath + File("cake-bootstrapper." + buildVersion + ".nupkg");

    // Push the package.
    NuGetPush(package, new NuGetPushSettings {
        Source = "https://www.myget.org/F/cake-bootstrapper/api/v2/package",
        ApiKey = apiKey
    });    
});

Task("Set-AppVeyor-Build-Version")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
{
    StartProcess("appveyor", new ProcessSettings {
        Arguments = string.Concat("UpdateBuild -Version \"", buildVersion, "\"")
    });
});

Task("Upload-AppVeyor-Artifact")
    .IsDependentOn("Build-Installer")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
{
    var artifactFile = File("Cake-Bootstrapper-v" + buildVersion + ".msi");
	var artifactPath = new FilePath(buildInstallerPath + artifactFile);

    Information("Uploading AppVeyor artifact {0}...", artifactPath.GetFilename());
    AppVeyor.UploadArtifact(artifactPath);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build-Chocolatey");

Task("AppVeyor")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .IsDependentOn("Publish-To-MyGet")
    .IsDependentOn("Set-AppVeyor-Build-Version")
    .IsDependentOn("Upload-AppVeyor-Artifact");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);