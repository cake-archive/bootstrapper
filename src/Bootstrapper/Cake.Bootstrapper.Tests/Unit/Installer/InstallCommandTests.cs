using System;
using Cake.Bootstrapper.Installer;
using Cake.Bootstrapper.Tests.Fixtures;
using Cake.Core.IO;
using NSubstitute;
using Xunit;

namespace Cake.Bootstrapper.Tests.Unit.Installer
{
    public sealed class InstallCommandTests
    {
        public sealed class TheConstructor
        {
            [Fact]
            public void Should_Set_Source_To_Default_Value()
            {
                // Given, When
                var command = new InstallCommand(null, null, null, null, null, null, null);

                // Then
                Assert.Equal("https://raw.githubusercontent.com/cake-build/bootstrapper/master/res/", command.Source);
            }
        }

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
            public void Should_Download_NuGet_Executable()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.NuGetExecutable.Exists.Returns(false);
                var command = fixture.CreateCommand();

                // When
                command.Execute();

                // Then                
                fixture.Downloader.Received(1).Download(
                    Arg.Is<Uri>(p => p == new Uri("http://nuget.org/nuget.exe")),
                    Arg.Is<FilePath>(p => p.FullPath == "/Working/tools/nuget.exe"));
            }

            [Fact]
            public void Should_Not_Download_NuGet_Executable_If_It_Already_Exist_On_Disc()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.NuGetExecutable.Exists.Returns(true);
                var command = fixture.CreateCommand();

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
                fixture.ScriptCopier.Received(1).Copy("build.ps1");
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
                fixture.ScriptCopier.Received(0).Copy("build.ps1");
            }

            [Fact]
            public void Should_Copy_Build_Script()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.BuildScript.Exists.Returns(false);
                var command = fixture.CreateCommand();

                // When
                command.Execute();

                // Then                
                fixture.ScriptCopier.Received(1).Copy("build.cake");
            }

            [Fact]
            public void Should_Not_Copy_Build_Script_If_It_Already_Exist_On_Disc()
            {
                // Given
                var fixture = new InstallCommandFixture();
                fixture.BuildScript.Exists.Returns(true);
                var command = fixture.CreateCommand();

                // When
                command.Execute();

                // Then                
                fixture.ScriptCopier.Received(0).Copy("build.cake");
            }
        }
    }
}
