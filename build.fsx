// include Fake lib
#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open System
open System.IO

// Properties
let buildDir = Path.Combine(Environment.CurrentDirectory, "build")
let deployDir = Path.Combine(Environment.CurrentDirectory, "deploy")
let runTarget = getBuildParamOrDefault "target" "Default";

let semVer = "1.0"

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; deployDir]
)

open Fake.AssemblyInfoFile
let buildNo = 
  match TeamCityBuildNumber with
    |None -> "1"
    |Some(_) -> TeamCityBuildNumber.Value  

let version =
  match buildServer with
  | TeamCity -> String.Format("{0}.{1}", semVer, buildNo)
  | _ -> String.Format("{0}.0", semVer)


let restore pc = 
   RestorePackage (fun p ->
    { p with
             Sources = ["https://www.nuget.org/api/v2/"]
             OutputPath = "./packages"
             Retries = 4 } ) pc

Target "PackageRestore" (fun _ ->
   !! "./**/packages.config"
   |> Seq.iter restore
   )

Target "SetVersion" (fun _ ->
    CreateCSharpAssemblyInfo "./SemanticReleaseNotesParser.Core/Properties/AssemblyInfo.cs"
        [Attribute.Title "SemanticReleaseNotesParser.Core"
         Attribute.Description ""
         Attribute.Guid "589794ec-4420-4b76-9b88-924d15148d51"
         Attribute.Product "SemanticReleaseNotesParser.Core"
         Attribute.Version version
         Attribute.FileVersion version]
)

Target "Build" (fun _ ->
    !! "./SemanticReleaseNotesParser.Core/SemanticReleaseNotesParser.Core.csproj"
      |> MSBuildRelease buildDir "Build"
      |> Log "AppBuild-Output: "
)

Target "Pack" (fun _ ->(
       NuGet (fun p ->
                {p with
                    OutputPath = deployDir
                    Project ="SemanticReleaseNotesParser.Core"
                    Authors = ["Enoro"]
                    Description = ""
                    Summary = ""
                    WorkingDir = buildDir
                    Version = version
                    Publish = false
                    Files = [
                         (@"SemanticReleaseNotesParser.Core.dll", Some "lib", None)
                         (@"SemanticReleaseNotesParser.Core.XML", Some "lib", None)
                    ]
                    Dependencies = [
                        "CommonMark.NET", "0.8.3"
                        "DotLiquid", "1.8.0"
                        "Humanizer", "1.37.7"
                    ]
                })
            "./SemanticReleaseNotesParser.Core/SemanticReleaseNotesParser.Core.nuspec"
))

Target "Default" (fun _ ->
    trace "Build Completed!"
)

// Dependencies
"Clean"
  ==> "PackageRestore"
  ==> "SetVersion"
  ==> "Build"
  ==> "Pack"
  ==> "Default"

// start build
RunTargetOrDefault runTarget