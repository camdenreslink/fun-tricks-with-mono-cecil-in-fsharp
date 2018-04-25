module AssemblyLoader

open Mono.Cecil
open System.IO

let LoadAssembly (assemblyPath: string) = 
    AssemblyDefinition.ReadAssembly assemblyPath

let LoadAllAssembliesByPrefix prefix binDirectoryPath =
    Directory.GetFiles (binDirectoryPath, sprintf "%s*" prefix) 
        |> Array.filter (fun f -> f.EndsWith ".dll" || f.EndsWith ".exe")
        |> Array.map LoadAssembly