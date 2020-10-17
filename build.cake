var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var workingDir = MakeAbsolute(Directory("./"));
string artifactsDirName = "Artifacts";
var artifactsDirectory = MakeAbsolute(Directory($"./{artifactsDirName}"));
string solutionFile = "./Illusion.Common.sln";
string projectFile = "./Illusion.Common/Illusion.Common.csproj";
string netCoreString = "netcoreapp3.1";

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
    .Does(() => {
        NuGetRestore("./Illusion.Common.sln", new NuGetRestoreSettings {
            Verbosity = NuGetVerbosity.Quiet
        });
        
        var buildSettings = new MSBuildSettings()
            .SetConfiguration(configuration)
            .SetPlatformTarget(PlatformTarget.MSIL)
            .SetVerbosity(Verbosity.Minimal)
            .UseToolVersion(MSBuildToolVersion.VS2019);
        MSBuild(solutionFile, buildSettings);
    });

Task("Pack")
    .WithCriteria(string.IsNullOrEmpty(pr))
    .IsDependentOn("Build")
    .Does(() => {
        var nuGetPackSettings = new NuGetPackSettings
        {
            OutputDirectory = artifactsDirectory,
            IncludeReferencedProjects = true,
            Properties = new Dictionary<string, string>
            {
                { "Configuration", "Release" },
                { "Platform", "AnyCPU" }
            },
            Version = version,
            Symbols                 = false,
            NoPackageAnalysis       = true,
            BasePath                = "./Illusion.Common/bin/Release/netcoreapp3.1",
        };

        NuGetPack(projectFile, nuGetPackSettings);
    });

Task("Push")
    .IsDependentOn("Pack")
    .Does(() => {
        var nugetSourceSettings = new NuGetSourcesSettings
                             {
                                 UserName = EnvironmentVariable("PRIVATE_FEED_USERNAME"),
                                 Password = EnvironmentVariable("PRIVATE_FEED_PASSWORD"),
                                 IsSensitiveSource = true,
                                 Verbosity = NuGetVerbosity.Detailed
                             };

        var feed = new
                    {
                        Name = "ArtifactoryV3",
                        Source = "https://tangredon.jfrog.io/artifactory/api/nuget/v3/illusion"
                    };

        NuGetAddSource(
            name: feed.Name,
            source: feed.Source,
            settings: nugetSourceSettings
        );
    });

Task("Appveyor-Artifacts")
    .WithCriteria(string.IsNullOrEmpty(pr))
    .IsDependentOn("Pack")
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

Task("Windows")
	.IsDependentOn("Info")
	.IsDependentOn("Clean")
	.IsDependentOn("Build")
	.IsDependentOn("Pack")
	.IsDependentOn("Push")
    .IsDependentOn("Appveyor-Artifacts")
    .Does(() => {
        Information("Windows Completed");
    });

Task("Default")
    .Does(() => {
        Information("Default Task Completed");
    });

RunTarget(target);
