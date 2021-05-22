#addin nuget:?package=Cake.Kudu.Client&version=1.0.1

var target = Argument("target", "Default");
var kuduUri = EnvironmentVariable("KUDU_CLIENT_BASEURI");
var kuduUserName  = EnvironmentVariable("KUDU_CLIENT_USERNAME");
var kuduPassword = EnvironmentVariable("KUDU_CLIENT_PASSWORD");
var appUri = EnvironmentVariable("APP_URI");

var projectPath = "./src/Web.csproj";
var artifactsDirectory = Directory("./artifacts");

Task("Build")
    .Does(() =>
    {
        var settings = new DotNetCoreBuildSettings
        {
            Configuration = "Release"
        };
        DotNetCoreBuild(projectPath, settings);
    });

var testTask = Task("Test")
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

Task("Package")
    .IsDependentOn("Test")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        PrintVersion();
        var settings = new DotNetCorePublishSettings
        {
            OutputDirectory = artifactsDirectory
        };
        DotNetCorePublish(projectPath, settings);
    });

Task("Deploy")
    .IsDependentOn("Package")
    .WithCriteria(!string.IsNullOrWhiteSpace(kuduUserName) && !string.IsNullOrWhiteSpace(kuduPassword))
    .Does(() =>
    {
        var kuduClient = KuduClient(kuduUri, kuduUserName, kuduPassword);
        kuduClient.ZipDeployDirectory(artifactsDirectory);
        Information($"Deployed at: {appUri}");
    });

Task("Default")
    .IsDependentOn(testTask);

private void PrintVersion()
{
    var version = XmlPeek(projectPath, "/Project/PropertyGroup/Version/text()");
    Information($"Detected version: {version}");
}

RunTarget(target);