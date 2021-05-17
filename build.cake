#addin nuget:?package=Cake.Kudu.Client&version=1.0.1

var target = Argument("target", "Test");

var projectPath = "./src/Web.csproj";
var artifactsDirectory = Directory("./artifacts");
var kuduUri = EnvironmentVariable("KUDU_CLIENT_BASEURI");
var kuduUserName  = EnvironmentVariable("KUDU_CLIENT_USERNAME");
var kuduPassword = EnvironmentVariable("KUDU_CLIENT_PASSWORD");

Task("Build")
    .Does(() =>
    {
        var settings = new DotNetCoreBuildSettings
        {
            Configuration = "Release"
        };
        DotNetCoreBuild(projectPath, settings);
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetCoreTest("./test/Tests.csproj");
    });

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(artifactsDirectory);
    });

var packageTask = Task("Package")
    .IsDependentOn("Test")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        GetVersion();
        var settings = new DotNetCorePublishSettings
        {
            OutputDirectory = artifactsDirectory
        };
        DotNetCorePublish(projectPath, settings);
    });

Task("Deploy")
    .IsDependentOn(packageTask)
    .WithCriteria(!string.IsNullOrWhiteSpace(kuduUserName) && !string.IsNullOrWhiteSpace(kuduPassword))
    .Does(() =>
    {
        var kuduClient = KuduClient(kuduUri, kuduUserName, kuduPassword);
        kuduClient.ZipDeployDirectory(artifactsDirectory);
        Information("Deployed");
    });

private void GetVersion()
{
    var version = XmlPeek(projectPath, "/Project/PropertyGroup/Version/text()");
    Information($"Detected version: {version}");
}

RunTarget(target);