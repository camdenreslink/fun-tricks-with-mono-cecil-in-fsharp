module AssemblyQueries

open Mono.Cecil

let FilterTypes predicate (assembly: AssemblyDefinition) =
    assembly.Modules
    |> Seq.collect (fun m -> m.Types)
    |> Seq.filter predicate

let FindAllTypesImplementingInterface interfaceFullName (assembly: AssemblyDefinition) =
    let findMatchingInterfaces (t: TypeDefinition) = 
        t.Interfaces 
            |> Seq.exists (fun i -> 
                i.FullName = interfaceFullName)
    assembly |> FilterTypes findMatchingInterfaces