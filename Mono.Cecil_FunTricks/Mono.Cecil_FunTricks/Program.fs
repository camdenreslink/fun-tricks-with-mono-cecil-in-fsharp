open Mono.Cecil

let openFile (filePath:string) =
    let assembly = AssemblyDefinition.ReadAssembly filePath
    assembly

[<EntryPoint>]
let main argv = 
    let assembly = openFile argv.[0]
    0 // return an integer exit code
