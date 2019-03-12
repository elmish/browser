(** Custom navigation
------
This port of the Elm library is about treating the address bar as an input to your program.
It converts changes in the address bar into messages and provides functions for manipulation of the browser history.
*)
namespace Elmish.Browser.Navigation

open Browser
open Browser.Types
open Elmish

(**
#### Parser
A function to turn the string in the address bar into data that is easier for your app to handle.
*)
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
                  let ev = CustomEvent.Create(NavigatedEvent)
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

    module Internal =
        let mutable private onChangeRef : obj -> obj =
            fun _ ->
                failwith "`onChangeRef` has not been itialized.\nPlease make sure you used Elmish.Browser.Navigation.Program.Internal.subscribe"

        let subscribe (dispatch:Dispatch<_ Navigable>) =
            let mutable lastLocation = None
            let onChange _ =
                match lastLocation with
                | Some href when href = window.location.href -> ()
                | _ ->
                    lastLocation <- Some window.location.href
                    Change window.location |> dispatch
                |> box

            onChangeRef <- onChange

            window.addEventListener("popstate", unbox onChangeRef)
            window.addEventListener("hashchange", unbox onChangeRef)
            window.addEventListener(Navigation.NavigatedEvent, unbox onChangeRef)

        let unsubscribe () =
            window.removeEventListener("popstate", unbox onChangeRef)
            window.removeEventListener("hashchange", unbox onChangeRef)
            window.removeEventListener(Navigation.NavigatedEvent, unbox onChangeRef)

        let toNavigableWith (parser : Parser<'a>)
                            (urlUpdate : 'a->'model->('model * Cmd<'msg>))
                            (program : Program<'a,'model,'msg,'view>)
                            (onLocationChange : Dispatch<Navigable<'msg>> -> unit) =

            let map (model, cmd) =
                model, cmd |> Cmd.map UserMsg

            let update msg model =
                match msg with
                | Change location ->
                    urlUpdate (parser location) model
                | UserMsg userMsg ->
                    program.update userMsg model
                |> map

            let subs model =
                Cmd.batch
                  [ [ onLocationChange ]
                    program.subscribe model |> Cmd.map UserMsg ]

            let init () =
                program.init (parser window.location) |> map

            { init = init
              update = update
              subscribe = subs
              onError = program.onError
              setState = fun model dispatch -> program.setState model (UserMsg >> dispatch)
              view = fun model dispatch -> program.view model (UserMsg >> dispatch)
              syncDispatch = id }

    /// Add the navigation to a program made with `mkProgram` or `mkSimple`.
    /// urlUpdate: similar to `update` function, but receives parsed url instead of message as an input.
    let toNavigable (parser : Parser<'a>)
                    (urlUpdate : 'a->'model->('model * Cmd<'msg>))
                    (program : Program<'a,'model,'msg,'view>) =

        Internal.toNavigableWith parser urlUpdate program Internal.subscribe
