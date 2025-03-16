module Elmish.TracersTests

open Swensen.Unquote
open NUnit.Framework

type ChildMsg =
    | ChildMsgNoField
    | ChildMsgOneIntField of int
    | ChildMsgTwoStringAndIntField of childStr: string * childInt: int

type RootMsg =
    | RootMsgNoField
    | RootMsgOneIntField of int
    | RootMsgTwoStringAndIntField of str: string * int: int
    | RootMsgOneChildField of ChildMsg
    | RootMsgOneIntOptionField of int option
    | RootMsgOneChildOptionField of ChildMsg option
    | RootMsgIntOptionOption of int option option

let getTraceForMsg (msg: 'Msg) =
    let actualName, actualValues = Tracers.getMsgNameAndFields typeof<'Msg> msg
    let actualValues = actualValues :?> (string * obj) list
    actualName, actualValues

[<Test>]
let ``getMsgNameAndFields for RootMsg.RootMsgNoField`` () =
    getTraceForMsg (RootMsg.RootMsgNoField)
    =! (nameof (RootMsg.RootMsgNoField), 
        [])

[<Test>]
let ``getMsgNameAndFields for RootMsg.RootMsgOneIntField`` () =
    getTraceForMsg (RootMsg.RootMsgOneIntField 42)
    =! (nameof (RootMsg.RootMsgOneIntField),
        [ ("Item", 42) ])

[<Test>]
let ``getMsgNameAndFields for RootMsg.RootMsgTwoStringAndIntField`` () =
    getTraceForMsg (RootMsg.RootMsgTwoStringAndIntField("test", 42))
    =! (nameof (RootMsg.RootMsgTwoStringAndIntField),
        [ ("str", "test"); ("int", 42) ])

[<Test>]
let ``getMsgNameAndFields for RootMsg.RootMsgOneChildField, ChildMsg.ChildMsgNoField`` () =
    getTraceForMsg (RootMsg.RootMsgOneChildField(ChildMsg.ChildMsgNoField))
    =! ($"{nameof ChildMsg.ChildMsgNoField}/{nameof (RootMsg.RootMsgOneChildField)}", 
        [])

[<Test>]
let ``getMsgNameAndFields for RootMsg.RootMsgOneChildField, ChildMsg.ChildMsgOneIntField`` () =
    getTraceForMsg (RootMsg.RootMsgOneChildField(ChildMsg.ChildMsgOneIntField(42)))
    =! ($"{nameof ChildMsg.ChildMsgOneIntField}/{nameof (RootMsg.RootMsgOneChildField)}",
        [ ("Item", 42) ])

[<Test>]
let ``getMsgNameAndFields for RootMsg.RootMsgOneChildField, ChildMsg.ChildMsgTwoStringAndIntField`` () =
    getTraceForMsg (RootMsg.RootMsgOneChildField(ChildMsg.ChildMsgTwoStringAndIntField("test", 42)))
    =! ($"{nameof ChildMsg.ChildMsgTwoStringAndIntField}/{nameof (RootMsg.RootMsgOneChildField)}", 
        [ ("childStr", "test"); ("childInt", 42) ])

[<Test>]
let ``getMsgNameAndFields for RootMsg.RootMsgOneIntOptionField, None`` () =
    getTraceForMsg (RootMsg.RootMsgOneIntOptionField(None))
    =! ($"None/{nameof (RootMsg.RootMsgOneIntOptionField)}",
        [])

[<Test>]
let ``getMsgNameAndFields for RootMsg.RootMsgOneIntOptionField, Some`` () =
    getTraceForMsg (RootMsg.RootMsgOneIntOptionField(Some 42))
    =! ($"Some/{nameof (RootMsg.RootMsgOneIntOptionField)}",
        [ ("Value", 42) ])

[<Test>]
let ``getMsgNameAndFields for RootMsg.RootMsgOneChildOptionField, None`` () =
    getTraceForMsg (RootMsg.RootMsgOneChildOptionField(None))
    =! ($"None/{nameof (RootMsg.RootMsgOneChildOptionField)}",
        [])

[<Test>]
let ``getMsgNameAndFields for RootMsg.RootMsgOneChildOptionField, Some ChildMsgNoField`` () =
    getTraceForMsg (RootMsg.RootMsgOneChildOptionField(Some(ChildMsgNoField)))
    =! ($"{nameof (ChildMsg.ChildMsgNoField)}/Some/{nameof (RootMsg.RootMsgOneChildOptionField)}",
        [])

[<Test>]
let ``getMsgNameAndFields for RootMsg.RootMsgOneChildOptionField, Some ChildMsgOneIntField`` () =
    getTraceForMsg (RootMsg.RootMsgOneChildOptionField(Some(ChildMsgOneIntField 42)))
    =! ($"{nameof (ChildMsg.ChildMsgOneIntField)}/Some/{nameof (RootMsg.RootMsgOneChildOptionField)}",
        [ ("Item", 42) ])

[<Test>]
let ``getMsgNameAndFields for RootMsg.RootMsgOneChildOptionField, Some ChildMsgTwoStringAndIntField`` () =
    getTraceForMsg (RootMsg.RootMsgOneChildOptionField(Some(ChildMsgTwoStringAndIntField("test", 42))))
    =! ($"{nameof (ChildMsg.ChildMsgTwoStringAndIntField)}/Some/{nameof (RootMsg.RootMsgOneChildOptionField)}",
        [ ("childStr", "test"); ("childInt", 42) ])

[<Test>]
let ``getMsgNameAndFields for RootMsg.RootMsgIntOptionOption, Some, None`` () =
    getTraceForMsg (RootMsg.RootMsgIntOptionOption(Some(None)))
    =! ($"None/Some/{nameof (RootMsg.RootMsgIntOptionOption)}",
        [])

[<Test>]
let ``getMsgNameAndFields for RootMsg.RootMsgIntOptionOption, Some, Some`` () =
    getTraceForMsg (RootMsg.RootMsgIntOptionOption(Some(Some 42)))
    =! ($"Some/Some/{nameof (RootMsg.RootMsgIntOptionOption)}",
        [ ("Value", 42) ])
