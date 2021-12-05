(**
---
layout: standard
title: Handling URL
toc: false
---
*)

(*** hide ***)

#load "../prelude.fsx"

open Elmish
open Elmish.UrlParser

(**

Handling URL updates in your application
------
*)

(**
Using [elmish-urlParser](https://elmish.github.io/urlParser) let us define the type that's going to represent our routes
and the parser to convert the browser URLs into instances of this type.

*)

type Route =
    | Blog of int
    | Search of string

let route =
    oneOf
        [ map Blog (s "blog" </> i32)
          map Search (s "search" </> str) ]

(**
To execute the parser we've defined and pass the route as a nice data structure into the update loop
we'll define `urlUpdate` function that takes the current state and the parsed output, for example:

*)

type Model =
    {
        route : Route
        query : string
    }

open Elmish.Navigation

let urlUpdate (result:Option<Route>) model =
    match result with
    | Some (Search query) ->
        { model with
            route = result.Value
            query = query
        }
        , Cmd.none

    | Some page ->
        { model with
            route = page
            query = ""
        }
        , Cmd.none

    | None ->
        model
        , Navigation.modifyUrl "#" // no matching route - go home

(**
It looks like `update` function but instead of the message it handles the route changes.
If the URL is valid, we just update our model or issue a command, otherwise we modify the URL to whatever makes sense.

Now we augument our program instance with Navigation capabilities, passing the parser and `urlUpdate`:
*)

open Elmish

Program.mkProgram init update view
|> Program.toNavigable (parseHash route) urlUpdate
|> Program.run

(**
:::info
Note that the type of the `init` function has changed - it will now receive the intial URL passed to our SPA!
:::

### Working with full (HTML5) and hash-based URLs

`parseHash` function works with "hashbang" URLs, i.e. everything after the '#' symbol, while `parsePath` works with the entire location.

The query for a hashbang URL never leaves the browser and can appear in traditional tags like `<a>`.

Working with full URL on the other hand means you have to be careful about which requests you want forwarded to the server and which ones should be handled locally.

To keep the request local you have to use `Navigation` module for all the URL transitions.

Make sure you understand which one you need.

*)
