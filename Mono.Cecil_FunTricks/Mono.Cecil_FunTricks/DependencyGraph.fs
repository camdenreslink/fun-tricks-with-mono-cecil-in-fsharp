module DependencyGraph

open Mono.Cecil

(* These objects mirror the Cytoscape.js api, and will be serialized to json *)
type GraphObjectData = {
    id: string;
    name: string;
    source: string;
    target: string;
}
type GraphObject = {
    group: string;
    data: GraphObjectData;
}

let GetProps (t: TypeDefinition) =
    t.Properties
        |> Seq.toList
        |> List.choose (fun p -> 
            if (p.FullName.StartsWith "SmartStore")
            then Some p
            else None)

let rec TraverseProps' fType acc (t: TypeDefinition) =
    let recurse = TraverseProps' fType
    let props = GetProps t
    match props with
    | [] -> fType acc t
    | ps ->
        let ts = ps |> List.map (fun p -> p.PropertyType.Resolve())
        let newAcc = fType acc t
        ts |> List.fold recurse newAcc 

(* This function is a fold over Type Properties *)
let rec TraverseProps fType acc (t: TypeDefinition, propName: string, id: string, parentId: string) =
    let recurse = TraverseProps fType
    let props = GetProps t
    match props with
    | [] -> fType acc (t, propName, id, parentId)
    | ps ->
        let ts = ps |> List.map (fun p -> p.PropertyType.Resolve(), p.Name, System.Guid.NewGuid().ToString(), id)
        let newAcc = fType acc (t, propName, id, parentId)
        ts |> List.fold recurse newAcc 

let GenerateDependencyGraph (rootNode: TypeDefinition) =
    (rootNode, "", System.Guid.NewGuid().ToString(), "") 
        |> TraverseProps (fun elements (t, propName, id, parentId) -> 
            let nodeData = {
                id = id;
                name = t.Name;
                source = "";
                target = "";
            }
            let node = {
                group = "nodes";
                data = nodeData;
            }
            let edgeData = {
                id = System.Guid.NewGuid().ToString();
                name = propName;
                source = parentId;
                target = id;
            }
            let edge = {
                group = "edges";
                data = edgeData;
            }
            if (t = rootNode)
            then node :: elements
            else node :: edge :: elements) []
