// include Fake lib
#r @"packages/FAKE.4.41.8/tools/FakeLib.dll"
open Fake

// Directories
let buildDir  = "./build/"
let testDir   = "./test/"
let deployDir = "./deploy/"
let reportDir = "./report"
let packagesDir = "./packages/"

// Filesets
let appReferences  = 
    !! "src/**/*.csproj"
    -- "src/**/*Specs.csproj"
         
let testReferences = !! "src/**/*Specs.csproj"

// tools
let fxCopRoot = "./tools/FxCop/FxCopCmd.exe"

// Targets
Target "Clean" (fun _ -> 
    CleanDirs [buildDir; testDir; deployDir; reportDir]
    RestorePackages()
)

Target "BuildApp" (fun _ ->
    // compile all projects below src/app/
    MSBuildRelease buildDir "Build" appReferences
        |> Log "AppBuild-Output: "
)

Target "BuildTest" (fun _ ->
    MSBuildDebug testDir "Build" testReferences
        |> Log "TestBuild-Output: "
)

Target "Test" (fun _ ->
    let MSpecRunnerVersion = GetPackageVersion packagesDir "Machine.Specifications.Runner.Console"
    let mspecTool = sprintf @"%sMachine.Specifications.Runner.Console.%s\tools\mspec-clr4.exe" packagesDir MSpecRunnerVersion

    !! (testDir @@ "*Specs.dll")
        |> MSpec (fun p -> 
            {p with 
                ToolPath = mspecTool
                HtmlOutputDir = reportDir}) 
)

Target "FxCop" (fun _ ->
    !! (buildDir + "/**/*.dll") 
        ++ (buildDir + "/**/*.exe")
        |> FxCop (fun p -> 
            {p with                     
                ReportFileName = testDir + "FXCopResults.xml";
                ToolPath = fxCopRoot})
)

Target "Deploy" DoNothing

// build order
"Clean"
    ==> "BuildApp"
    ==> "BuildTest"
    ==> "FxCop"
    ==> "Test"
    ==> "Deploy"

// start build
RunTargetOrDefault "Deploy"