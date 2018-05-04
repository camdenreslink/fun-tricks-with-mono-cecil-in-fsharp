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

let FindAllTypesInNamespace namespace' (assembly: AssemblyDefinition) =
    assembly |> FilterTypes (fun t -> t.Namespace.StartsWith namespace')

let FindAllConcreteClassesInNamespace namespace' (assembly: AssemblyDefinition) =
    assembly
        |> FindAllTypesInNamespace namespace'
        |> Seq.filter (fun t -> t.IsClass && not t.IsAbstract)

let QueryMissingEFMappings (assemblies: AssemblyDefinition[]) =
    let efMapperClasses = 
        assemblies
            |> Array.toSeq
            |> Seq.collect (FilterTypes (fun t -> 
                match t.BaseType with
                | null -> false
                | baseType -> 
                    baseType.FullName.StartsWith 
                        "System.Data.Entity.ModelConfiguration.EntityTypeConfiguration`1"))

    let domainClassesMapped = 
        efMapperClasses
            |> Seq.choose (fun t -> 
                match t.BaseType with
                | null -> None
                | baseType -> 
                    match baseType with
                    | :? GenericInstanceType as genericInstance ->
                        let genericArgs = Seq.toList genericInstance.GenericArguments
                        match genericArgs with
                        | [t] -> Some t
                        | _ -> None
                    | _ -> None)
    
    let domainClasses =
        assemblies
            |> Array.toSeq
            |> Seq.collect (fun a -> a |> FindAllConcreteClassesInNamespace "SmartStore.Core.Domain")

    let domainClassesMissingMappers =
        domainClasses
            |> Seq.filter (fun t ->
                 domainClassesMapped 
                    |> Seq.exists (fun m -> m.FullName = t.FullName)
                    |> not)

    domainClassesMissingMappers

let FindTypeByFullName fullName (assembly: AssemblyDefinition) =
    assembly |> FilterTypes (fun t -> t.FullName = fullName)