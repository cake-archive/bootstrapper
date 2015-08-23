### New in 0.1.0 (Released 2015/08/23)
* Added dry run parameter.
* Added parameter for experimental script engine.
* Added Get-Help.
* Added parameter for Mono script engine.
* Added support for PowerShell Verbose flag.
* Added option to skip tool package restore.

### New in 0.0.6 (Released 2015/04/28)
* Added support for nightly build of Roslyn.
* Added option to install empty cake script.
* Made NuGet installation optional from bootstrapper.

### New in 0.0.5 (Released 2015/01/17)
* Added support for missing tools and package.config.
* Bootstrapper script will now download Nuget if it doesn't exist.
* Changed appveyor.yml to launch build.ps1 via PowerShell native.
* Made .gitignore patching optional.
* Replace Start-Process with Invoke-Command in bootstrapper script.

### New in 0.0.4 (Released 2014/12/10)
* Added patching of .gitignore.
* Added AppVeyor configuration file.
* Replaced Start-Process with Invoke-Expression in bootstrapper script template.
* Now returns exit code from bootstrapper script template.

### New in 0.0.3 (Released 2014/11/29)
* Added default build script template functionality.
* Added icon for Chocolatey package.
* Renamed Chocolatey ID to conform to the naming rules.
* Added better description for Chocolatey package.
* Removed copyright notice from bootstrapper script.

### New in 0.0.2 (Released 2014/11/29)
* Updated Chocolatey build to point to the correct install file.

### New in 0.0.1 (Released 2014/11/29)
* First version of the Cake bootstrapper.