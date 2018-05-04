open Mono.Cecil
open System
open DependencyGraph
open Newtonsoft.Json

[<EntryPoint>]
let main argv = 
    let prefix = "SmartStore"

    (* We want to exclude SmartStore.Licensing, because it's a weird
       obfuscated assembly that we don't have the source for, so it
       isn't relevant to our analysis. *)
    let assemblies = (AssemblyLoader.LoadAllAssembliesByPrefix prefix argv.[0])
                        |> Array.filter (fun a -> 
                            a.MainModule.Name <> "SmartStore.Licensing.dll")

    let type' = assemblies
                        |> Seq.collect (fun a -> 
                            AssemblyQueries.FindTypeByFullName "SmartStore.Core.Domain.Customers.Customer" a)
                        |> Seq.exactlyOne


    let graph = GenerateDependencyGraph type'

    let json = JsonConvert.SerializeObject(graph)

    0 (* return an integer exit code *)