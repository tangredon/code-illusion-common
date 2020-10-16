#addin nuget:?package=Cake.Json&version=5.2.0
#module nuget:?package=Cake.Parallel.Module&version=0.21.0
#addin nuget:?package=Cake.Yarn&version=0.4.6
#module nuget:?package=Cake.DotNetTool.Module&version=0.4.0
#addin nuget:?package=Cake.Coverlet&version=2.4.2
#tool nuget:?package=xunit.runner.console&version=2.4.1
#tool nuget:?package=Codecov&version=1.11.1
#addin nuget:?package=Cake.Codecov&version=0.8.0

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var workingDir = MakeAbsolute(Directory("./"));
string artifactsDirName = "Artifacts";
var artifactsDirectory = MakeAbsolute(Directory($"./{artifactsDirName}"));
string solutionFile = "./Illusion.sln";
string projectFile = "./Illusion/Illusion.csproj";
string testResultsFile = "test_results.xml";
string netFrameworkString = "net461";
string netCoreString = "netcoreapp3.1";
var packageFilePath = Directory("./Illusion/ClientApp");
string version = EnvironmentVariable("GitVersion_SemVer") ?? "0.0.0";
string shortSha = EnvironmentVariable("GitVersion_ShortSha") ?? "0000000";
string pr = EnvironmentVariable("APPVEYOR_PULL_REQUEST_NUMBER") ?? "";
string branchName = EnvironmentVariable("APPVEYOR_REPO_BRANCH") ?? "unknown";

Setup(context =>
{
    Information($"Version: {version}");
});

Task("Info")
    .Does(() =>
    {
        Information(@"Cake build script starting...");
        
        Information(@"Requires C:\msys64 to be present for packaging (Pre-installed on AppVeyor) on Windows");
        Information(@"Working directory is: " + workingDir);
        Information($"Branch: {branchName}");
        Information($"Pull Request: {string.IsNullOrEmpty(pr) == false}");
        if (IsRunningOnWindows())
        {
            Information("Platform is Windows");
        }
        else
        {
            Information("Platform is Linux, Windows builds will be skipped");
        }

        SerializeJsonToPrettyFile("Illusion/ClientApp/buildData.json", new {
            version = version,
            shortSha = shortSha
        });
    });

Task("Clean")
    .IsDependentOn("Info")
    .Does(() =>
    {
        CleanDirectories("./src/**/obj");
        CleanDirectories("./src/**/bin");
        CleanDirectories("./BuildOutput");
        CleanDirectories("./" + artifactsDirName);
        CreateDirectory("./" + artifactsDirName);
        Information("Clean completed");
    });

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Build-Backend")
    .IsDependentOn("Build-Frontend")
    .Does(() => {
    });

Task("Build-Backend")
    .Does(() => {
        NuGetRestore("./Illusion.sln", new NuGetRestoreSettings {
            Verbosity = NuGetVerbosity.Quiet
        });
        
        var buildSettings = new MSBuildSettings()
            .SetConfiguration(configuration)
            .SetVerbosity(Verbosity.Minimal)
            .UseToolVersion(MSBuildToolVersion.VS2019);
        MSBuild(solutionFile, buildSettings);
    });

Task("Build-Frontend")
    .Does(() => {
        Yarn.FromPath(packageFilePath).Install();
        Yarn.FromPath(packageFilePath).RunScript("build:ci");
    });

Task("Unit-Tests")
    .IsDependentOn("Build")
    .Does(() => {
        var projects = GetFiles("./UnitTests/**/*.csproj");
        var testSettings = new DotNetCoreTestSettings {
            Configuration = "Debug",
            NoBuild = false,
            ArgumentCustomization = args => args.Append($"--logger trx;LogFileName=\"{testResultsFile}\"")
        };
        var coverletSettings = new CoverletSettings {
            CollectCoverage = true,
            CoverletOutputFormat = CoverletOutputFormat.cobertura,
            // CoverletOutputDirectory = artifactsDirectory,
            CoverletOutputName = "coverage.xml"
        };
        foreach(var project in projects)
        {
            DotNetCoreTest(project.FullPath, testSettings, coverletSettings);
        }
        
        if (AppVeyor.IsRunningOnAppVeyor)
        {
            
            var testRestults = GetFiles($"./UnitTests/**/{testResultsFile}");
            foreach(var testResult in testRestults)
            {
                BuildSystem.AppVeyor.UploadTestResults(testResult, AppVeyorTestResultsType.XUnit);
            }
        }
    });

Task("Upload-Results")
    .IsDependentOn("Unit-Tests")
    .IsDependentOn("Codecov")
    .IsDependentOn("Codacy")
    .Does(() => {
    });

Task("Codecov")
    .Does(() => {
        var coverageFiles = GetFiles("./UnitTests/**/coverage.xml");
        foreach (var coverageFile in coverageFiles)
        {
            Codecov(coverageFile.FullPath);
        }
    });

Task("Codacy")
    .Does(() => {
        if (EnvironmentVariable("CODACY_PROJECT_TOKEN") == null)
        {
            throw new Exception("CODACY_PROJECT_TOKEN environment variable not set");
        }
        var response = DownloadFile("https://dl.bintray.com/codacy/Binaries/11.3.7/codacy-coverage-reporter-assembly.jar");
        
        var coverageFiles = GetFiles("./UnitTests/**/coverage.xml");
        IEnumerable<string> redirectedStandardOutput;
        IEnumerable<string> redirectedErrorOutput;
        foreach (var coverageFile in coverageFiles)
        {
            var exitCodeWithArgument =
                StartProcess(
                    "java",
                    new ProcessSettings {
                        Arguments = $"-jar {response} report -l CSharp -r \"{coverageFile.FullPath}\" --partial",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    out redirectedStandardOutput,
                    out redirectedErrorOutput
                );
            Information("Codacy Uploaded: {0}", coverageFile.FullPath);
        }
        var finalExitCode = StartProcess(
            "java",
            new ProcessSettings {
                Arguments = $"-jar {response} final",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            },
            out redirectedStandardOutput,
            out redirectedErrorOutput
        );
        if (finalExitCode != 0)
        {
            throw new Exception($"Codacy Exit Code {finalExitCode}");
        }
    });

Task("Publish")
    .WithCriteria(string.IsNullOrEmpty(pr))
    .IsDependentOn("Build")
    .Does(() => {
        string buildOutputDir = "./BuildOutput/Illusion";
        DotNetCorePublish(projectFile, netCoreString, "linux-x64", buildOutputDir);
        Zip(buildOutputDir, $"./{artifactsDirName}/Illusion.{version}.zip");
    });

Task("Appveyor-Artifacts")
    .WithCriteria(string.IsNullOrEmpty(pr))
    .IsDependentOn("Clean")
    .Does(() =>
    {
        if (AppVeyor.IsRunningOnAppVeyor)
        {
            foreach (var file in GetFiles(workingDir + $"/{artifactsDirName}/*"))
            {
                AppVeyor.UploadArtifact(file.FullPath);
            }
        }
        else
        {
            Information(@"Skipping artifact push as not running in AppVeyor Windows Environment");
        }
    });

private void DotNetCorePublish(string projectPath, string framework, string runtime, string outputPath)
{
    // bool publishSingleFile = false;
    if (framework != netFrameworkString)
    {
        var settings = new DotNetCorePublishSettings
        {
            Framework = framework,
            Runtime = runtime,
            OutputDirectory = outputPath,
            ArgumentCustomization = args => args.Append("/p:PublishSingleFile=false")
        };
        DotNetCorePublish(projectPath, settings);
    }
    else
    {
        var settings = new DotNetCorePublishSettings
        {
            Framework = framework,
            Runtime = runtime,
            OutputDirectory = outputPath
        };
        DotNetCorePublish(projectPath, settings);
    }
}

private void RunMsysCommand(string utility, string utilityArguments)
{
    var msysDir = @"C:\msys64\usr\bin\";
    var utilityProcess = msysDir + utility + ".exe";
    Information("MSYS2 Utility: " + utility);
    Information("MSYS2 Directory: " + msysDir);
    Information("Utility Location: " + utilityProcess);
    Information("Utility Arguments: " + utilityArguments);
    IEnumerable<string> redirectedStandardOutput;
    IEnumerable<string> redirectedErrorOutput;
    var exitCodeWithArgument =
        StartProcess(
            utilityProcess,
            new ProcessSettings {
                Arguments = utilityArguments,
                WorkingDirectory = msysDir,
                RedirectStandardOutput = true
            },
            out redirectedStandardOutput,
            out redirectedErrorOutput
        );
    Information(utility + " output:" + Environment.NewLine + string.Join(Environment.NewLine, redirectedStandardOutput.ToArray()));
    // Throw exception if anything was written to the standard error.
    if (redirectedErrorOutput != null && redirectedErrorOutput.Any())
    {
        throw new Exception(
            string.Format(
                utility + " Errors ocurred: {0}",
                string.Join(", ", redirectedErrorOutput)));
    }
    Information(utility + " Exit code: {0}", exitCodeWithArgument);
}

private string RelativeWinPathToFullPath(string relativePath)
{
    return (workingDir + relativePath.TrimStart('.'));
}

private void RunLinuxCommand(string file, string arg)
{
    var startInfo = new System.Diagnostics.ProcessStartInfo()
    {
        Arguments = arg,
        FileName = file,
        UseShellExecute = true
    };
    var process = System.Diagnostics.Process.Start(startInfo);
    process.WaitForExit();
}

private void Gzip(string sourceFolder, string outputDirectory, string tarCdirectoryOption, string outputFileName)
{
    var tarFileName = outputFileName.Remove(outputFileName.Length - 3, 3);
    
    if (IsRunningOnWindows())
    {
        var fullSourcePath = RelativeWinPathToFullPath(sourceFolder);
        var tarArguments = @"--force-local -cvf " + fullSourcePath + "/" + tarFileName + " -C " + fullSourcePath + $" {tarCdirectoryOption} --mode ='755'";
        var gzipArguments = @"-k " + fullSourcePath + "/" + tarFileName;
        RunMsysCommand("tar", tarArguments);
        RunMsysCommand("gzip", gzipArguments);
        MoveFile($"{sourceFolder}/{tarFileName}.gz", $"{outputDirectory}/{tarFileName}.gz");
    }
    else
    {
        RunLinuxCommand("find",  MakeAbsolute(Directory(sourceFolder)) + @" -type d -exec chmod 755 {} \;");
        RunLinuxCommand("find",  MakeAbsolute(Directory(sourceFolder)) + @" -type f -exec chmod 644 {} \;");
        RunLinuxCommand("tar",  $"-C {sourceFolder} -zcvf {outputDirectory}/{tarFileName}.gz {tarCdirectoryOption}");
    }	
}

private void CheckForGzipAndTar()
{
    if (FileExists(@"C:\msys64\usr\bin\tar.exe") && FileExists(@"C:\msys64\usr\bin\gzip.exe"))
    {
        Information("tar.exe and gzip.exe were found");
    }
    else
    {
        throw new Exception("tar.exe and gzip.exe were NOT found");   
    }
}

Task("Dev")
    .Does(() => {
        Information("Dev Completed");
    });

Task("Linux")
    .Does(() => {
        Information("Linux Completed");
    });

Task("Windows")
	.IsDependentOn("Build")
    .IsDependentOn("Unit-Tests")
    .IsDependentOn("Upload-Results")
	.IsDependentOn("Publish")
    .IsDependentOn("Appveyor-Artifacts")
    .Does(() => {
        Information("Windows Completed");
    });

Task("Default")
    .IsDependentOn("Dev")
    .Does(() => {
        Information("Default Task Completed");
    });

RunTarget(target);
