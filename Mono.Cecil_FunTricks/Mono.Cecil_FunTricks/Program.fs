open Mono.Cecil

[<EntryPoint>]
let main argv = 
    let prefix = "SmartStore"

    (* We want to exclude SmartStore.Licensing, because it's a weird
       obfuscated assembly that we don't have the source for, so it
       isn't relevant to our analysis. *)
    let assemblies = (AssemblyLoader.LoadAllAssembliesByPrefix prefix argv.[0])
                        |> Array.filter (fun a -> 
                            a.MainModule.Name <> "SmartStore.Licensing.dll")
    
    let disposableTypes = assemblies 
                            |> Seq.collect (AssemblyQueries.FindAllTypesImplementingInterface 
                                typeof<System.IDisposable>.FullName)
                            |> Seq.toArray

    0 (* return an integer exit code *)