var target          = Argument("target", "Default");

var configuration   = Argument<string>("configuration", "Release");



///////////////////////////////////////////////////////////////////////////////

// GLOBAL VARIABLES

///////////////////////////////////////////////////////////////////////////////

var isLocalBuild        = !AppVeyor.IsRunningOnAppVeyor;

var packPath            = Directory("./src/Dkbe.CaptivePortal.MockServer");

var sourcePath          = Directory("./src");

var buildArtifacts      = Directory("./artifacts/packages");



Task("Build")

    .IsDependentOn("Clean")

    .IsDependentOn("Restore")

    .Does(() =>

{

	var projects = GetFiles("./**/project.json");



	foreach(var project in projects)

	{

        var settings = new DotNetCoreBuildSettings 

        {

            Configuration = configuration

        };



        DotNetCoreBuild(project.GetDirectory().FullPath, settings); 

    }

});



Task("Pack")

    .IsDependentOn("Restore")

    .IsDependentOn("Clean")

    .Does(() =>

{

    var settings = new DotNetCorePackSettings

    {

        Configuration = configuration,

        OutputDirectory = buildArtifacts,

    };



    // add build suffix for CI builds

    if(!isLocalBuild)

    {

        settings.VersionSuffix = "build" + AppVeyor.Environment.Build.Number.ToString().PadLeft(5,'0');

    }



    DotNetCorePack(packPath, settings);

});



Task("Clean")

    .Does(() =>

{

    CleanDirectories(new DirectoryPath[] { buildArtifacts });

});



Task("Restore")

    .Does(() =>

{

    var settings = new DotNetCoreRestoreSettings

    {

        Sources = new [] { "https://api.nuget.org/v3/index.json" }

    };



    DotNetCoreRestore(sourcePath, settings);
});



Task("Default")

  .IsDependentOn("Build")

  .IsDependentOn("Pack");



RunTarget(target);
