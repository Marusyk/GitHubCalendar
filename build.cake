//#addin Cake.Curl
#addin nuget:?package=Cake.Curl&version=3.0.0
#addin nuget:?package=Cake.Kudu.Client&version=0.6.0
//#addin nuget:?package=Cake.Git

//https://ghgraph.scm.azurewebsites.net/
//https://ghgraph.azurewebsites.net/
// .\build.ps1 -target setversion -ScriptArgs "--newVersion=1.0.0"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var newVersion = Argument<string>("newVersion", "");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var version = "0.1.0";
var artifactsDirectory = Directory("./artifacts");
var project = "./src/Web.csproj";
var kuduBaseUri = EnvironmentVariable("KUDU_CLIENT_BASEURI");
var deployUserName  = EnvironmentVariable("KUDU_CLIENT_USERNAME");
var deployPassword = EnvironmentVariable("KUDU_CLIENT_PASSWORD");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("clean")
    .Does(() =>
{
    CleanDirectory(artifactsDirectory);
});
    
Task("build")
    .Does(() => 
{
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = configuration
    };
    DotNetCoreBuild("./GitHubCalendar.sln", settings);
});

Task("test")
    .Does(() =>
{
    DotNetCoreTest("./test/Tests.csproj");
});

Task("version")
    .Does(() =>
{
    version = XmlPeek(
       project,
        "/Project/PropertyGroup/Version/text()"
    );

    Information($"Detected version: {version}");
});

Task("setVersion")
    .IsDependentOn("version")
    .Does(() =>
{
    if(!Version.TryParse(newVersion, out _))
    {
        Error($"Wrong version: {newVersion}");
        return;
    }
    
    XmlPoke(
        "./src/Web.csproj",
        "/Project/PropertyGroup/Version",
        newVersion
    );

    Information($"Set version: {newVersion}");
});

// Task("commit")
//     .Does(() =>
// {
//     GitAdd("./", project);

//     try
//     {
//         GitCommit("./", "Roman Marusyk", "romamarusyk@gmail.com", $"Cake commit. Set version to {newVersion}");
//     }
//     catch
//     {
//         Warning("No changes; nothing to commit.");
//     }
// });

Task("package")
    .IsDependentOn("clean")
    .IsDependentOn("build")
    .IsDependentOn("test")
    .IsDependentOn("version")
    .Does(() => 
{
    var settings = new DotNetCorePublishSettings
    {
        OutputDirectory = artifactsDirectory
    };

    DotNetCorePublish(project, settings);
});

Task("zip")
    .IsDependentOn("package")
    .Does(() =>
{
    Zip(artifactsDirectory, GetPackagePath());
});

Task("deployByCurl")
    .IsDependentOn("zip")
    .Does(() => 
{
    CurlUploadFile(
        GetPackagePath(),
        new Uri($"{kuduBaseUri}/api/zipdeploy"),
        new CurlSettings
        {
            RequestCommand = "POST",
            Username = deployUserName,
            Password = deployPassword,
            ArgumentCustomization = args => args.Append("--fail")
        });
});

Task("deployByKuduClient")
    .WithCriteria(!string.IsNullOrWhiteSpace(deployUserName) && !string.IsNullOrWhiteSpace(deployPassword))
    .IsDependentOn("package")
    .Does(() => 
{
    var kuduClient = KuduClient(kuduBaseUri, deployUserName, deployPassword);

    kuduClient.ZipDeployDirectory(artifactsDirectory);
});

Task("default")
    .IsDependentOn("build")
    .IsDependentOn("test");

Task("deploy")
    .IsDependentOn("deployByKuduClient");

///////////////////////////////////////////////////////////////////////////////
// HELPERS
///////////////////////////////////////////////////////////////////////////////

private string GetPackagePath()
{
    return $"{artifactsDirectory}/GitHubCalendar.{version}.zip";
}

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);