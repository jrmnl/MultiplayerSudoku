#addin nuget:?package=Cake.Npm&version=0.17.0

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = Argument("version", "1.2.3.4");



////////////////////////////////////
// Common
////////////////////////////////////
var publishDir = Directory("./publish");

Task("Clean-Output")
    .Does(() => {
        CleanDirectory(publishDir);
    });
    


////////////////////////////////////
// Backend
////////////////////////////////////
var projectPath = "../backend/src";
var backendPublishDir = $"{publishDir}/backend";

Task("Backend-Clean")
    .Does(() => {
        var settings = new DotNetCoreCleanSettings {
            Configuration = configuration
        };
        DotNetCoreClean(projectPath, settings);
    });

Task("Backend-Restore")
    .IsDependentOn("Backend-Clean")
    .Does(() => {
        DotNetCoreRestore(projectPath);
    });

Task("Backend-Build")
    .IsDependentOn("Backend-Restore")
    .IsDependentOn("Clean-Output")
    .Does(() => {
        var msBuildSettings = new DotNetCoreMSBuildSettings();
        msBuildSettings.SetVersion(version);
        var settings = new DotNetCoreBuildSettings {
            Configuration = configuration,
            NoRestore = true,
            MSBuildSettings = msBuildSettings
        };
        DotNetCoreBuild(projectPath, settings);
    });

Task("Backend-Tests")
    .IsDependentOn("Backend-Build")
    .Does(() => {
        var settings = new DotNetCoreTestSettings {
            Configuration = configuration,
            NoBuild = true,
        };
        var projectFiles = GetFiles($"{projectPath}/**/*.Tests.csproj");
        foreach(var file in projectFiles) {
            DotNetCoreTest(file.FullPath, settings);
        }
    });

Task("Backend-Publish")
    .IsDependentOn("Backend-Tests")
    .Does(() => {
        var settings = new DotNetCorePublishSettings {
            Configuration = configuration,
            NoBuild = true,
            OutputDirectory = backendPublishDir
        };
        DotNetCorePublish($"{projectPath}/MultiplayerSudoku.Application/MultiplayerSudoku.Application.csproj", settings);
    });



////////////////////////////////////
// Frontend (requires npm to be pre-installed)
////////////////////////////////////
var frontendPath = "../frontend";
var frontendPublish = $"{publishDir}/frontend";
var resultingElmFile = "main.js";

Task("Install-Elm")
    .Does(() => {
        var settings = new NpmInstallSettings
        {
            Global = true,
            Production = false,
            LogLevel = NpmLogLevel.Default
        };
        settings.AddPackage("elm");
        NpmInstall(settings);
    });

Task("Elm-Make")
    .IsDependentOn("Clean-Output")
    .IsDependentOn("Install-Elm")
    .Does(() => {
        var processSettings = new ProcessSettings{
            WorkingDirectory = frontendPath,
            Arguments = $"make src/main.elm --output={resultingElmFile}"
        };
        StartProcess("elm", processSettings);
    });;

Task("Frontend-Publish")
    .IsDependentOn("Elm-Make")
    .Does(() => {
        CreateDirectory(frontendPublish);
        CopyFileToDirectory($"{frontendPath}/{resultingElmFile}", frontendPublish);
        CopyFileToDirectory($"{frontendPath}/index.html", frontendPublish);
    });



////////////////////////////////////
// Composite
////////////////////////////////////

Task("Default")
    .IsDependentOn("Backend-Publish")
    .IsDependentOn("Frontend-Publish");

RunTarget(target);
