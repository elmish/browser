[<RequireQualifiedAccess>]
module Elmish.Tracers

open System
open Fable.Core

let getMsgNameAndFields (t: Type) (x: 'Msg) : string * obj =

    let getLongName (t: Type) = sprintf "%s.%s"t.Namespace t.Name

    let isOption (t: Type) = getLongName t = "Microsoft.FSharp.Core.FSharpOption`1"

    let isUnion (t: Type) = isOption t || Reflection.FSharpType.IsUnion t

    let getUnionFields x (t: Type) =
        if isOption t then
            // Options are special-cased because they are erased by Fable,
            // and thus do not return true for IsUnion().
            // (IsUnion() returns true for options on platforms other than Fable)
            let value = box x
            if isNull value then
                {|
                    CaseName = "None"
                    FieldsValues = [||]
                    FieldsTypes = [||]
                |}
            else
                let value =
                    #if FABLE_COMPILER
                    value
                    #else
                    let _, field = FSharp.Reflection.FSharpValue.GetUnionFields(value, t)
                    field[0]
                    #endif
                let valueType = t.GenericTypeArguments[0]
                {|
                    CaseName = "Some"
                    FieldsValues = [| value |]
                    FieldsTypes = [| {| Type = valueType; FieldName = "Value"; IsUnion = isUnion valueType |} |]

                |}
        else // Not a Union:
            let uci, ucFields = Reflection.FSharpValue.GetUnionFields(x, t)
            {|
                CaseName = uci.Name
                FieldsValues = ucFields
                FieldsTypes =
                    uci.GetFields()
                    |> Array.map (fun x ->{|
                        Type = x.PropertyType
                        FieldName = x.Name
                        IsUnion = isUnion x.PropertyType |})
            |}
    
    let rec getCaseName (t: Type) (acc: string list) (x: obj) =
        let ucInfo = getUnionFields x t
        let acc = ucInfo.CaseName :: acc

        match ucInfo.FieldsTypes with
        | [| fieldInfo |] when fieldInfo.IsUnion -> getCaseName fieldInfo.Type acc ucInfo.FieldsValues[0]
        | fieldsTypes ->
            // Case names are intentionally left reverted so we see
            // the most meaningful message first
            let msgName = acc |> String.concat "/"

            let fields =
                (fieldsTypes, ucInfo.FieldsValues)
                ||> Array.zip
                |> Array.map (fun (fi, v) -> fi.FieldName, v)
                #if FABLE_COMPILER
                |> JsInterop.createObj
                #else
                |> box
                #endif

            msgName, fields

    if isUnion t then
        getCaseName t [] x
    else
        "Msg", box x

/// Use it when initializing your Elmish app like this
/// |> Program.withTrace Tracers.console
let inline console (msg: 'Msg) (state: 'State) (subId: SubId list) =
    let msg, fields = getMsgNameAndFields typeof<'Msg> msg
    JS.console.log (msg, fields)
    JS.console.log ("State:", state)
    if subId |> List.isEmpty |> not then
        JS.console.log ("Subscriptions:", subId)
