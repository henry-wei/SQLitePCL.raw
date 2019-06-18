﻿
open System
open System.Diagnostics
open System.IO
open System.Xml.Linq
open System.Linq

let exec = build.exec

[<EntryPoint>]
let main argv =
    let timer = Stopwatch.StartNew()

    let top =
        let cwd = Directory.GetCurrentDirectory()
        Path.GetFullPath(Path.Combine(cwd, ".."))
        // TODO maybe walk upward until we find the right directory

    exec "dotnet" "run .." (Path.Combine(top, "version_stamp"))

    exec "dotnet" "run .." (Path.Combine(top, "gen_nuspecs"))

    let dir_nupkgs = Path.Combine(top, "nupkgs")
    Directory.CreateDirectory(dir_nupkgs)
    for s in Directory.GetFiles(dir_nupkgs, "*.nupkg") do
        File.Delete(s)

    let dir_providers = Path.Combine(top, "src", "providers")
    exec "dotnet" "restore" dir_providers

    let gen_provider dir_basename (dllimport_name:string) (provider_basename:string) conv kind uwp =
        let dir_name = sprintf "SQLitePCLRaw.provider.%s" dir_basename
        let cs_name = sprintf "provider_%s.cs" (provider_basename.ToLower())
        let cs_path = Path.Combine(top, "src", dir_name, "Generated", cs_name)
        let dllimport_name_arg = 
            if kind = "dynamic" 
            then "" 
            else sprintf "-p:NAME_FOR_DLLIMPORT=%s" dllimport_name
        // TODO want to change this to the local tool
        let args = sprintf "-o %s -p:NAME=%s -p:CONV=%s -p:KIND=%s -p:UWP=%s %s provider.tt" cs_path provider_basename conv kind uwp dllimport_name_arg
        exec "t4" args dir_providers

    gen_provider "dynamic" null "Cdecl" "Cdecl" "dynamic" "false"
    gen_provider "dynamic" null "StdCall" "StdCall" "dynamic" "false"
    gen_provider "e_sqlite3" "e_sqlite3" "e_sqlite3" "Cdecl" "dllimport" "false"
    gen_provider "e_sqlcipher" "e_sqlcipher" "e_sqlcipher" "Cdecl" "dllimport" "false"
    gen_provider "sqlite3" "sqlite3" "sqlite3" "Cdecl" "dllimport" "false"
    gen_provider "sqlcipher" "sqlcipher" "sqlcipher" "Cdecl" "dllimport" "false"
    gen_provider "internal" "__Internal" "internal" "Cdecl" "dllimport" "false"

    gen_provider "winsqlite3" "winsqlite3" "winsqlite3" "StdCall" "dllimport" "true"

    gen_provider "e_sqlite3.uwp" "e_sqlite3" "e_sqlite3_uwp" "Cdecl" "dllimport" "true"
    gen_provider "e_sqlcipher.uwp" "e_sqlcipher" "e_sqlcipher_uwp" "Cdecl" "dllimport" "true"
    gen_provider "sqlcipher.uwp" "sqlcipher" "sqlcipher_uwp" "Cdecl" "dllimport" "true"

    exec "dotnet" "build -c Release" (Path.Combine(top, "src", "SQLitePCLRaw.nativelibrary"))

    let pack_dirs = [
        "SQLitePCLRaw.core"
        "SQLitePCLRaw.ugly" 
        "SQLitePCLRaw.provider.dynamic" 
        "SQLitePCLRaw.provider.internal" 
        "SQLitePCLRaw.provider.e_sqlite3" 
        "SQLitePCLRaw.provider.e_sqlite3.uwp" 
        "SQLitePCLRaw.provider.e_sqlcipher" 
        "SQLitePCLRaw.provider.e_sqlcipher.uwp" 
        "SQLitePCLRaw.provider.sqlite3" 
        "SQLitePCLRaw.provider.sqlcipher" 
        "SQLitePCLRaw.provider.sqlcipher.uwp" 
        "SQLitePCLRaw.provider.winsqlite3" 
    ]
    for s in pack_dirs do
        exec "dotnet" "pack -c Release" (Path.Combine(top, "src", s))

    let batteries_dirs = [
        "e_sqlite3.dllimport"
        "e_sqlite3.dllimport.uwp"
        "e_sqlite3.dynamic"
        "e_sqlcipher.dllimport"
        "e_sqlcipher.dllimport.uwp"
        "e_sqlcipher.dynamic"
        "sqlite3"
        "sqlcipher.dynamic"
        "sqlcipher.dllimport"
        "sqlcipher.dllimport.uwp"
        "winsqlite3"
        ]
    for s in batteries_dirs do
        let dir_name = sprintf "SQLitePCLRaw.batteries_v2.%s" s
        exec "dotnet" "build -c Release" (Path.Combine(top, "src", dir_name))

    let msbuild_dirs = [
        "lib.e_sqlite3.android"
        "lib.e_sqlite3.ios"
        "lib.e_sqlcipher.android"
        "lib.e_sqlcipher.ios"
        "lib.sqlcipher.ios.placeholder"
        "batteries_v2.e_sqlite3.internal.ios"
        "batteries_v2.e_sqlcipher.internal.ios"
        "batteries_v2.sqlcipher.internal.ios"
        ]
    for s in msbuild_dirs do
        let dir_name = sprintf "SQLitePCLRaw.%s" s
        let dir = (Path.Combine(top, "src", dir_name))
        exec "dotnet" "restore" dir
        exec "msbuild" "/p:Configuration=Release" dir

    let get_build_prop p =
        let path_xml = Path.Combine(top, "Directory.Build.props")
        let xml = XElement.Load(path_xml);
        let props = xml.Elements(XName.Get "PropertyGroup").First()
        let ver = props.Elements(XName.Get p).First()
        ver.Value

    let version = get_build_prop "Version"

    printfn "%s" version

    let nuspecs = [
        "lib.e_sqlite3"
        "lib.e_sqlcipher"
        "bundle_green"
        "bundle_e_sqlite3"
        "bundle_e_sqlcipher"
        "bundle_zetetic"
        "bundle_winsqlite3"
        ]
    for s in nuspecs do
        let name = sprintf "SQLitePCLRaw.%s" s
        let dir_proj = Path.Combine(top, "src", name)
        Directory.CreateDirectory(Path.Combine(dir_proj, "empty"))
        exec "dotnet" "pack" dir_proj

    exec "dotnet" "run" (Path.Combine(top, "test_nupkgs", "smoke"))

    exec "dotnet" "run" (Path.Combine(top, "test_nupkgs", "fsmoke"))

    exec "dotnet" "test" (Path.Combine(top, "test_nupkgs", "e_sqlite3", "real_xunit"))
    exec "dotnet" "test" (Path.Combine(top, "test_nupkgs", "e_sqlcipher", "real_xunit"))

    exec "dotnet" "run --framework=netcoreapp2.2" (Path.Combine(top, "test_nupkgs", "e_sqlite3", "fake_xunit"))
    exec "dotnet" "run --framework=netcoreapp2.2" (Path.Combine(top, "test_nupkgs", "e_sqlcipher", "fake_xunit"))

    exec "dotnet" "run --framework=net461" (Path.Combine(top, "test_nupkgs", "e_sqlite3", "fake_xunit"))
    exec "dotnet" "run --framework=net461" (Path.Combine(top, "test_nupkgs", "e_sqlcipher", "fake_xunit"))

    exec "dotnet" "run --framework=netcoreapp3.0" (Path.Combine(top, "test_nupkgs", "e_sqlite3", "fake_xunit"))
    exec "dotnet" "run --framework=netcoreapp3.0" (Path.Combine(top, "test_nupkgs", "e_sqlcipher", "fake_xunit"))

    timer.Stop()
    printfn "Total build time: %A milliseconds" timer.ElapsedMilliseconds

    0 // return an integer exit code

