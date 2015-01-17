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