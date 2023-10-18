#!/usr/bin/env -S dotnet fsi
#r "nuget: Fake.Core.Target"
#r "nuget: Fake.IO.FileSystem"
#r "nuget: Fake.DotNet.Cli"
#r "nuget: Fake.Core.Target"
#r "nuget: Fake.Core.ReleaseNotes"
#r "nuget: Fake.Tools.Git"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.Tools
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open System
open System.IO

let gitName = "browser"
let gitOwner = "elmish"
let gitHome = sprintf "https://github.com/%s" gitOwner
let gitRepo = sprintf "git@github.com:%s/%s.git" gitOwner gitName

// Filesets
let projects  =
    !! "src/**.fsproj"
    ++ "netstandard/**.fsproj"

System.Environment.GetCommandLineArgs()
|> Array.skip 2 // fsi.exe; build.fsx
|> Array.toList
|> Context.FakeExecutionContext.Create false __SOURCE_FILE__
|> Context.RuntimeContext.Fake
|> Context.setExecutionContext

Target.create "Clean" (fun _ ->
    Shell.cleanDir "src/obj"
    Shell.cleanDir "src/bin"
    Shell.cleanDir "netstandard/obj"
    Shell.cleanDir "netstandard/bin"
)

Target.create "Restore" (fun _ ->
    projects
    |> Seq.iter (Path.GetDirectoryName >> DotNet.restore id)
)

Target.create "Build" (fun _ ->
    projects
    |> Seq.iter (Path.GetDirectoryName >> DotNet.build id)
)

Target.create "Test" (fun _ ->
    DotNet.test (fun a -> a.WithCommon id) "tests"
)

let release = ReleaseNotes.load "RELEASE_NOTES.md"

Target.create "Meta" (fun _ ->
    [ "<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">"
      "<ItemGroup>"
      "<None Include=\"../docs/static/img/logo.png\" Pack=\"true\" PackagePath=\"\\\"/>"
      "<PackageReference Include=\"Microsoft.SourceLink.GitHub\" Version=\"1.0.0\" PrivateAssets=\"All\"/>"
      "</ItemGroup>"
      "<PropertyGroup>"
      "<Description>Elmish extensions for Fable apps targeting web browsers</Description>"
      "<EmbedUntrackedSources>true</EmbedUntrackedSources>"
      "<AllowedOutputExtensionsInPackageBuildOutputFolder>$(AllowedOutputExtensionsInPackageBuildOutputFolder);.pdb</AllowedOutputExtensionsInPackageBuildOutputFolder>"
      sprintf "<PackageProjectUrl>http://%s.github.io/%s</PackageProjectUrl>" gitOwner gitName
      "<PackageLicenseExpression>Apache-2.0</PackageLicenseExpression>"
      "<PackageIconUrl>https://raw.githubusercontent.com/elmish/browser/master/docs/static/img/logo.png</PackageIconUrl>"
      "<PackageIcon>logo.png</PackageIcon>"
      sprintf "<RepositoryUrl>%s/%s</RepositoryUrl>" gitHome gitName
      "<PackageTags>fable;MVU;fsharp</PackageTags>"
      sprintf "<PackageReleaseNotes>%s</PackageReleaseNotes>" (List.head release.Notes)
      "<Authors>Eugene Tolmachev</Authors>"
      sprintf "<Version>%s</Version>" (string release.SemVer)
      "</PropertyGroup>"
      "</Project>"]
    |> File.write false "Directory.Build.props"
)

// --------------------------------------------------------------------------------------
// Build a NuGet package

Target.create "Package" (fun _ ->
    projects
    |> Seq.iter (Path.GetDirectoryName >> DotNet.pack id)
)

Target.create "PublishNuget" (fun _ ->
    let exec dir = DotNet.exec (DotNet.Options.withWorkingDirectory dir)

    let args = sprintf "push Fable.Elmish.Browser.%s.nupkg -s nuget.org -k %s" (string release.SemVer) (Environment.environVar "nugetkey")
    let result = exec "src/bin/Release" "nuget" args
    if (not result.OK) then failwithf "%A" result.Errors
)

// Generate the documentation
Target.create "GenerateDocs" (fun _ ->
    let res = Shell.Exec("npm", "run docs:build")

    if res <> 0 then
        failwithf "Failed to generate docs"
)

Target.create "WatchDocs" (fun _ ->
    let res = Shell.Exec("npm", "run docs:watch")

    if res <> 0 then
        failwithf "Failed to watch docs: %d" res
)

// --------------------------------------------------------------------------------------
// Release Scripts

Target.create "ReleaseDocs" (fun _ ->
    let res = Shell.Exec("npm", "run docs:publish")

    if res <> 0 then
        failwithf "Failed to publish docs: %d" res
)

Target.create "Publish" ignore

// Build order
"Clean"
  ==> "Meta"
  ==> "Restore"
  ==> "Build"
  ==> "Test"
  ==> "Package"
  ==> "PublishNuget"
  ==> "Publish"

// start build
Target.runOrDefault "Test"
