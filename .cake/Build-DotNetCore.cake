#load "Configuration.cake"

Task("Build:DotNetCore")
    .IsDependentOn("Restore")
    .IsDependeeOf("Build")
    .Does<Configuration>(config =>
{
    var buildSettings = new DotNetCoreBuildSettings() {
        NoRestore = true,
        Configuration = config.Solution.BuildConfiguration,
        NoIncremental = true
    };

    DotNetCoreBuild(config.Solution.Path.ToString(), buildSettings);
});