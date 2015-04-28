using System;
using Cake.Bootstrapper.Installer;
using Cake.Bootstrapper.Tests.Fixtures;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using NSubstitute;
using Xunit;

namespace Cake.Bootstrapper.Tests.Unit.Installer
{
    public sealed class InstallCommandTests
    {
        public sealed class TheExecuteMethod
        {
            [Fact]
            public void Should_Create_Tools_Directory()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.ToolsDirectory.Exists.Returns(false);
                var command = fixture.CreateCommand();

                // When
                command.Execute();

                // Then
                fixture.ToolsDirectory.Received(1).Create();
            }

            [Fact]
            public void Should_Not_Create_Tools_Directory_If_It_Already_Exist_On_Disc()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.ToolsDirectory.Exists.Returns(true);
                var command = fixture.CreateCommand();

                // When
                command.Execute();

                // Then
                fixture.ToolsDirectory.Received(0).Create();
            }

            [Fact]
            public void Should_Download_NuGet_Executable_If_InstallNuGet_Is_Specified()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.NuGetExecutable.Exists.Returns(false);
                var command = fixture.CreateCommand();
                command.InstallNuGet = true;

                // When
                command.Execute();

                // Then
                fixture.Downloader.Received(1).Download(
                    Arg.Is<Uri>(p => p == new Uri("http://nuget.org/nuget.exe")),
                    Arg.Is<FilePath>(p => p.FullPath == "/Working/tools/nuget.exe"));
            }

            [Fact]
            public void Should_Not_Download_NuGet_Executable_If_InstallNuGet_Is_Not_Specified()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.NuGetExecutable.Exists.Returns(false);
                var command = fixture.CreateCommand();
                command.InstallNuGet = false;

                // When
                command.Execute();

                // Then
                fixture.Downloader.Received(0).Download(
                    Arg.Is<Uri>(p => p == new Uri("http://nuget.org/nuget.exe")),
                    Arg.Is<FilePath>(p => p.FullPath == "/Working/tools/nuget.exe"));
            }

            [Fact]
            public void Should_Not_Download_NuGet_Executable_If_It_Already_Exist_On_Disc_And_InstallNuGet_Is_Specified()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.NuGetExecutable.Exists.Returns(true);
                var command = fixture.CreateCommand();
                command.InstallNuGet = true;

                // When
                command.Execute();

                // Then
                fixture.Downloader.Received(0).Download(
                    Arg.Is<Uri>(p => p == new Uri("http://nuget.org/nuget.exe")),
                    Arg.Is<FilePath>(p => p.FullPath == "/Working/tools/nuget.exe"));
            }

            [Fact]
            public void Should_Create_NuGet_Package_Configuration()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.PackagesConfiguration.Exists.Returns(false);
                var command = fixture.CreateCommand();

                // When
                command.Execute();

                // Then
                fixture.PackageConfigurationCreator.Received(1).Generate(
                    Arg.Is<DirectoryPath>(p => p.FullPath == "/Working/tools"));
            }

            [Fact]
            public void Should_Not_Create_NuGet_Package_Configuration_If_It_Already_Exist_On_Disc()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.PackagesConfiguration.Exists.Returns(true);
                var command = fixture.CreateCommand();

                // When
                command.Execute();

                // Then
                fixture.PackageConfigurationCreator.Received(0).Generate(
                    Arg.Is<DirectoryPath>(p => p.FullPath == "/Working/tools"));
            }

            [Fact]
            public void Should_Copy_Bootstrapper_Script()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.BootstrapperScript.Exists.Returns(false);
                var command = fixture.CreateCommand();

                // When
                command.Execute();

                // Then
                fixture.FileCopier.Received(1).CopyBootstrapperScript();
            }

            [Fact]
            public void Should_Not_Copy_Bootstrapper_Script_If_It_Already_Exist_On_Disc()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.BootstrapperScript.Exists.Returns(true);
                var command = fixture.CreateCommand();

                // When
                command.Execute();

                // Then
                fixture.FileCopier.Received(0).CopyBootstrapperScript();
            }

            [Fact]
            public void Should_Copy_Convention_Based_Build_Script_If_Empty_Not_Set()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.BuildScript.Exists.Returns(false);
                var command = fixture.CreateCommand();
                command.Empty = false;

                // When
                command.Execute();

                // Then
                fixture.FileCopier.Received(1).CopyConventionBasedCakeScript();
            }

            [Fact]
            public void Should_Not_Copy_Build_Script_If_It_Already_Exist_On_Disc_And_Empty_Not_Set()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.BuildScript.Exists.Returns(true);
                var command = fixture.CreateCommand();
                command.Empty = false;

                // When
                command.Execute();

                // Then
                fixture.FileCopier.Received(0).CopyConventionBasedCakeScript();
            }

            [Fact]
            public void Should_Copy_Empty_Build_Script_If_Empty_Is_Set()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.BuildScript.Exists.Returns(false);
                var command = fixture.CreateCommand();
                command.Empty = true;

                // When
                command.Execute();

                // Then
                fixture.FileCopier.Received(1).CopyEmptyCakeScript();
            }

            [Fact]
            public void Should_Not_Copy_Build_Script_If_It_Already_Exist_On_Disc_And_Empty_Is_Set()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.BuildScript.Exists.Returns(true);
                var command = fixture.CreateCommand();
                command.Empty = true;

                // When
                command.Execute();

                // Then
                fixture.FileCopier.Received(0).CopyEmptyCakeScript();
            }

            [Fact]
            public void Should_Patch_GitIgnore_If_Flag_Is_Set()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.GitIgnore.Exists.Returns(true);
                var command = fixture.CreateCommand();
                command.GitIgnore = true;

                // When
                command.Execute();

                // Then
                fixture.GitIgnorePatcher.Received(1).Patch(
                    Arg.Is<FilePath>(p => p.FullPath == "/Working/.gitignore"));
            }

            [Fact]
            public void Should_Not_Patch_GitIgnore_If_Flag_Is_Not_Set()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.GitIgnore.Exists.Returns(true);
                var command = fixture.CreateCommand();
                command.GitIgnore = false;

                // When
                command.Execute();

                // Then
                fixture.GitIgnorePatcher.Received(0).Patch(
                    Arg.Is<FilePath>(p => p.FullPath == "/Working/.gitignore"));
            }

            [Fact]
            public void Should_Write_To_Log_If_GitIgnore_Was_Patched()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.GitIgnore.Exists.Returns(true);
                fixture.GitIgnorePatcher.Patch(Arg.Any<FilePath>()).Returns(true);
                var command = fixture.CreateCommand();
                command.GitIgnore = true;

                // When
                command.Execute();

                // Then
                fixture.Log.Received(1).Write(
                    Verbosity.Normal, LogLevel.Information, " -> Patched .gitignore.");
            }

            [Fact]
            public void Should_Not_Patch_GitIgnore_If_It_Does_Not_Exist_On_Disc()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.GitIgnore.Exists.Returns(false);
                var command = fixture.CreateCommand();
                command.GitIgnore = true;

                // When
                command.Execute();

                // Then
                fixture.GitIgnorePatcher.Received(0).Patch(
                    Arg.Is<FilePath>(p => p.FullPath == "/Working/.gitignore"));
            }

            [Fact]
            public void Should_Copy_AppVeyor_Configuration_Script_If_Flag_Is_Set()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.AppVeyorConfiguration.Exists.Returns(false);
                var command = fixture.CreateCommand();
                command.AppVeyor = true;

                // When
                command.Execute();

                // Then
                fixture.FileCopier.Received(1).CopyAppVeyorConfiguration();
            }

            [Fact]
            public void Should_Not_Copy_AppVeyor_Configuration_Script_If_Flag_Is_Not_Set()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.AppVeyorConfiguration.Exists.Returns(false);
                var command = fixture.CreateCommand();
                command.AppVeyor = false;

                // When
                command.Execute();

                // Then
                fixture.FileCopier.Received(0).CopyAppVeyorConfiguration();
            }

            [Fact]
            public void Should_Not_Copy_AppVeyor_Configuration_If_It_Already_Exist_On_Disc()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.AppVeyorConfiguration.Exists.Returns(true);
                var command = fixture.CreateCommand();
                command.AppVeyor = true;

                // When
                command.Execute();

                // Then
                fixture.FileCopier.Received(0).CopyAppVeyorConfiguration();
            }
        }
    }
}
