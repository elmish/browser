(*** hide ***)
#I "../../src/bin/Debug/netstandard1.6"
#I "../../packages/Fable.Core/lib/netstandard1.6"
#I "../../packages/Fable.Elmish/lib/netstandard1.6"
#r "Fable.Core.dll"
#r "Fable.Elmish.dll"

(** Parsing routes
---------
The library defines building blocks for processing URLs in a way that assigns types to the values extracted.
The idea is to define a parser in type-safe fashion, using combinators:

- `s` combinator for a static string we expect to find in the URL,
- `</>` combinator to capture a slash in the URL,
- `str` combinator to extract a string,
- `i32` combinator to attempt to parse an `int`.
- `top` combinator that takes nothing.

Some examples:
*)

#r "Fable.Elmish.Browser.dll"

open Elmish.UrlParser

parse (s "blog" </> i32) "blog/42" // Some 42

parse (s "blog" </> i32) "blog/13" //  Some 13

parse (s "blog" </> i32) "blog/hello" // None

parse (s "search" </> str) "search/dogs" // Some "dogs"

parse (s "search" </> str) "search/13" // Some "13"

parse (s "search" </> str) "search" // None

(**

Normally you want to put many of these parsers together to handle all possible routes. The following parser works on URLs like `/blog/42` and `/search/badger`:

*)

type Route = Blog of int | Search of string | Home

let route =
    oneOf
        [ map Blog (s "blog" </> i32)
          map Search (s "search" </> str) ]

(**

Here we are turning URLs into nice union types, so we can use case expressions to work with them in a nice way.
`oneOf` will try the listed parsers one by one until it finds one that returns `Some`. The `map` function in this example passes the outputs from the parser into the the case constructors.

*)

parse route "blog/58"    // Some (Blog 58)
parse route "search/cat" // Some (Search "cat")
parse route "search/31"  // Some (Search "31")
parse route "blog/cat"   // None
parse route "blog"       // None

(**

Note that F# case constructors take all of the arguments as a tuple, while the parser will apply the arguments individually, so we may need to adapt the signature: 

*)


type Route2 = BlogDouble of int * string // needs arguments in tupled form

let curry f x y = f (x,y) // convert tupled form function of two arguments into curried form

let route2 state =
    map (curry BlogDouble) (s "blog" </> i32 </> str) state

(**
 
Now the compiler is happy for the two arguments to be passed individually to the curried case constructor.

### Handling URL updates in your application

Now that we've parsed the route into a nice data structure, we need to handle the updates. 
For that we'll define `urlUpdate` function that takes the current state and the parsed output, for example:

*)

type Model =
  { route : Route
    query : string }

open Elmish.Navigation

let urlUpdate (result:Option<Route>) model =
  match result with
  | Some (Search query) ->
      { model with route = result.Value; query = query }, [] // Issue some search Cmd instead

  | Some page ->
      { model with route = page; query = "" }, []

  | None ->
      ( model, Navigation.modifyUrl "#" ) // no matching route - go home
    
(**
It looks like `update` function but instead of the message it handles the route changes.
If the URL is valid, we just update our model or issue a command, otherwise we modify the URL to whatever makes sense.

We will also need to modify our init function so that it takes the route as an argument; usually it defaults to `unit` if routing is not used.
*)
open Elmish

let init (initialRoute:Option<Route>) =
  ({ route = initialRoute |> Option.defaultValue Home; query = "" }, Cmd.Empty)

(**
Now we augument our program instance with Navigation capabilities, passing the parser and `urlUpdate`:
*)

Program.mkProgram init update view
|> Program.toNavigable (parseHash route) urlUpdate
|> Program.run

(**

### Working with full (HTML5) and hash-based URLs
`parseHash` function works with "hashbang" URLs, i.e. everything after the '#' symbol, while `parsePath` works with the entire location.
The query for a hashbang URL never leaves the browser and can appear in traditional tags like `<a>`. 
Working with full URL on the other hand means you have to be careful about which requests you want forwarded to the server and which ones should be handled locally.
To keep the request local you have to use `Navigation` module for all the URL transitions.

Make sure you understand which one you need.

*)

