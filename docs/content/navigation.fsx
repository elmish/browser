(*** hide ***)
#I "../../src/bin/Debug/netstandard1.6"
#I "../../packages/Fable.Core/lib/netstandard1.6"
#I "../../packages/Fable.Elmish/lib/netstandard1.6"
#r "Fable.Core.dll"
#r "Fable.Elmish.dll"
#r "Fable.Elmish.Browser.dll"

(**
*)
(** Custom navigation
------
This port of the Elm library is about treating the address bar as an input to your program. 
It converts changes in the address bar into messages and provides functions for manipulation of the browser history.
*) 
namespace Elmish.Browser.Navigation

open Fable.Import.Browser
open Elmish

/// Parser is a function to turn the string in the address bar into
/// data that is easier for your app to handle.
type Parser<'a> = Location -> 'a

type Navigable<'msg> = 
    | Change of Location
    | UserMsg of 'msg

(**
#### Direct history manipulation
*)
[<RequireQualifiedAccess>]
module Navigation =
    let [<Literal>] internal NavigatedEvent = "NavigatedEvent"

    /// Modify current location
    let modifyUrl (newUrl:string):Cmd<_> =
        [fun _ -> history.replaceState((), "", newUrl)]

    /// Push new location into history and navigate there
    let newUrl (newUrl:string):Cmd<_> =
        [fun _ -> history.pushState((), "", newUrl)
                  let ev = document.createEvent_CustomEvent()
                  ev.initCustomEvent (NavigatedEvent, true, true, obj())
                  window.dispatchEvent ev
                  |> ignore ]

    /// Jump to some point in history (positve=forward, nagative=backward)
    let jump (n:int):Cmd<_> =
        [fun _ -> history.go n]


(**
#### Program extensions
Treat user's program as a child component, by wrapping it and handling navigation events.
*)
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module Program =
  /// Add the navigation to a program made with `mkProgram` or `mkSimple`.
  /// urlUpdate: similar to `update` function, but receives parsed url instead of message as an input.
  let toNavigable (parser : Parser<'a>) 
                  (urlUpdate : 'a->'model->('model * Cmd<'msg>)) 
                  (program : Program<'a,'model,'msg,'view>) =
    let map (model, cmd) = 
        model, cmd |> Cmd.map UserMsg
    
    let update msg model =
        match msg with
        | Change location ->
            urlUpdate (parser location) model
        | UserMsg userMsg ->
            program.update userMsg model
        |> map

    let locationChanges (dispatch:Dispatch<_ Navigable>) = 
        let mutable lastLocation = None
        let onChange _ =
            match lastLocation with
            | Some href when href = window.location.href -> ()
            | _ ->
                lastLocation <- Some window.location.href
                Change window.location |> dispatch
            |> box
                
        window.addEventListener_popstate(unbox onChange)
        window.addEventListener_hashchange(unbox onChange)
        window.addEventListener(Navigation.NavigatedEvent, unbox onChange)
    
    let subs model =
        Cmd.batch
          [ [locationChanges]
            program.subscribe model |> Cmd.map UserMsg ]
    
    let init () = 
        program.init (parser window.location) |> map
    
    { init = init 
      update = update
      subscribe = subs
      onError = program.onError
      setState = fun model dispatch -> program.setState model (UserMsg >> dispatch) 
      view = fun model dispatch -> program.view model (UserMsg >> dispatch) }
    
    

