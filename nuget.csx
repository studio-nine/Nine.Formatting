#load "Common/scripts/build.csx"

BuildTestPublishPreRelease(
    projects: new []
    {
        "src/Nine.Formatting.Abstractions",
        "src/Nine.Formatting",
    }, 
    testProjects: new []
    {
        "test/Nine.Formatting.Test",
    });