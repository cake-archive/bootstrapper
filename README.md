#Cake Bootstrapper

The Cake Bootstrapper is a Powershell cmdlet that helps you set up a new [Cake](https://github.com/cake-build/cake) build by downloading dependencies, setting up the bootstrapper script and creating a Cake build script.

##What is Cake?

Cake (C# Make) is a build automation system with a C# DSL to do things like compiling code, copy files/folders, running unit tests, compress files and build NuGet packages.
For more information about the Cake project, see [https://github.com/cake-build/cake](https://github.com/cake-build/cake).

##Usage

Start by either cloning and building this repository (by executing `build.ps1`) and executing the MSI artifact, or install the Powershell cmdlet via Chocolatey:

```dos
PS C:\> choco install cake-bootstrapper
```

When everything is installed, close the Powershell window and open a new one to make sure that everything been properly setup.
Now, go to the project where you want to setup a new Cake build and write `Install-Cake`.

```dos
PS C:\MyProject> Install-Cake
 -> Downloaded NuGet executable.
 -> Generated NuGet package configuration.
 -> Copied bootstrapper script.
 -> Copied build script.
```

This will result in the following directory structure:

* tools
  * nuget.exe
  * packages.config
* build.cake
* build.ps1

##Executing a build

To run your cake build, start the bootstrapper script `build.ps1`.

```dos
PS C:\MyProject> .\build.ps1
```

The bootstrapper script is responsible for installing the dependencies specified in tools/packages.config such as Cake or unit test runners. You can also pass arguments to the bootstrapper script to affect execution of the build script.

```dos
PS C:\MyProject> .\build.ps1 -Target "Build" -Configuration "Debug"
```
