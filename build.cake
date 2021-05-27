#addin nuget:?package=Cake.Kudu.Client&version=1.0.1

var target = Argument("target", "Default");
var kuduUri = EnvironmentVariable("KUDU_CLIENT_BASEURI");
var kuduUserName  = EnvironmentVariable("KUDU_CLIENT_USERNAME");
var kuduPassword = EnvironmentVariable("KUDU_CLIENT_PASSWORD");

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
    .IsDependentOn("Clean")
    .IsDependentOn(testTask)
    .Does(() =>
    {
        PrintVersion();
        var settings = new DotNetCorePublishSettings
        {
            OutputDirectory = artifactsDirectory,
            NoBuild = true
        };
        DotNetCorePublish(projectPath, settings);
    });

Task("Deploy")
    .WithCriteria(
        !string.IsNullOrWhiteSpace(kuduUserName) && 
        !string.IsNullOrWhiteSpace(kuduPassword) &&
        !string.IsNullOrWhiteSpace(kuduUri))
    .IsDependentOn("Package")
    .Does(() =>
    {
        var client = KuduClient(kuduUri, kuduUserName, kuduPassword);
        client.ZipDeployDirectory(artifactsDirectory);
        Information($"Deployed at: {EnvironmentVariable("APP_URI")}");
    });

Task("Default")
    .IsDependentOn(testTask);

private void PrintVersion()
{
    var vesrion = XmlPeek(projectPath, "/Project/PropertyGroup/Version/text()");
    Information($"Detected version: {vesrion}");
}
RunTarget(target);