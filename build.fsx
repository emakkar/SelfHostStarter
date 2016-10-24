// include Fake lib
#r @"packages/FAKE/tools/FakeLib.dll"
open Fake

RestorePackages()

// Properties
let buildDir = "./build/"
let testDir  = "./test/"
let deployDir = "./deploy/"

// version info
let version = "0.2"  // or retrieve from CI server

// Target
Target "Clean" (fun _ ->
  CleanDirs [buildDir; testDir; deployDir]
)

Target "Build" (fun _ ->
    !! "Src/**/*.csproj"
    |> MSBuildRelease "" "Build"
    |> Log "AppBuild-Output: "
)

// Copies binaries from default VS location to expected bin folder
// But keeps a subdirectory structure for each project in the
// src folder to support multiple project outputs
Target "CopyBinaries" (fun _ ->
    !! "src/**/*.??proj"
    |>  Seq.map (fun f -> ((System.IO.Path.GetDirectoryName f) </> "bin/Release", buildDir </> (System.IO.Path.GetFileNameWithoutExtension f)))
    |>  Seq.iter (fun (fromDir, toDir) -> CopyDir toDir fromDir (fun _ -> true))
)

Target "Test" (fun _ ->
    !! "Src/**/*Specs.csproj"
    |> MSBuildDebug testDir "Build"
    |> Log "TestBuild-Output: "
)

Target "Zip" (fun _ ->
    !! (buildDir + "/**/*.*")
    -- "*.zip"
    |> Zip buildDir (deployDir + "Calculator." + version + ".zip")
)

Target "Default" (fun _ ->
    trace "Hello World from FAKE"
)

// Dependencies
"Clean"
    ==> "Build"
    //==> "CopyBinaries"
    ==> "FxCop"
    ==> "Test"
    ==> "Zip"
    ==> "Default"
  
// start build
RunTargetOrDefault "Default"