(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../src/bin/Debug/netstandard1.6"
#I "../../packages/Fable.Elmish/lib/netstandard1.6"
#r "Fable.Elmish.dll"
#r "Fable.Elmish.Browser.dll"
open Elmish

(**
Elmish-browser: browser extras for Elmish apps
=======

Elmish-browser implements routing and navigation for Fable apps targeting the web browsers.
This library helps you turn URLs into nicely structured data, handle browser address change events and manipulate browser history directly.


## Installation

```shell
paket add nuget Fable.Elmish.Browser
```

## Routing: Combinators for parsing browser url into a route
Usage:
*)

open Elmish.Browser.Navigation

Program.mkProgram init update view
|> Program.toNavigable parser urlUpdate
|> Program.run

(**
## Navigation: Manipulate the browser's navigation and history
Usage:
*)

open Elmish.Browser.Navigation

let update model msg =
    model, Navigation.newUrl "some_other_location"
