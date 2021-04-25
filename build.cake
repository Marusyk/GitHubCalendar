#addin nuget:?package=Cake.Kudu.Client&version=1.0.1

var target = Argument("target", "Default");

var artifactsDirectory = Directory("./artifacts");
var project = "./src/Web.csproj";
var kuduBaseUri = EnvironmentVariable("KUDU_CLIENT_BASEURI");
var deployUserName  = EnvironmentVariable("KUDU_CLIENT_USERNAME");
var deployPassword = EnvironmentVariable("KUDU_CLIENT_PASSWORD");

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(artifactsDirectory);
    });

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        var settings = new DotNetCoreBuildSettings
        {
            Configuration = "Release"
        };
        DotNetCoreBuild("./GitHubCalendar.sln", settings);
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetCoreTest("./test/Tests.csproj");
    });

var packageTask = Task("Package")
    .IsDependentOn("Test")
    .Does(() =>
    {
        GetVersion();
        var settings = new DotNetCorePublishSettings
        {
            OutputDirectory = artifactsDirectory
        };
        DotNetCorePublish(project, settings);
    });

Task("Deploy")
    .WithCriteria(!string.IsNullOrWhiteSpace(deployUserName) && !string.IsNullOrWhiteSpace(deployPassword))
    .IsDependentOn("Default")
    .Does(() =>
    {
        var kuduClient = KuduClient(kuduBaseUri, deployUserName, deployPassword);

        kuduClient.ZipDeployDirectory(artifactsDirectory);
        Information("Deployed");
    });

Task("Default")
    .IsDependentOn(packageTask);

private void GetVersion()
{
    var version = XmlPeek(project, "/Project/PropertyGroup/Version/text()");
    Information($"Detected version: {version}");
}

RunTarget(target);