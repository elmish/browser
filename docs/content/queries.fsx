(*** hide ***)
#I "../../src/bin/Debug/netstandard1.6"
#I "../../packages/Fable.Core/lib/netstandard1.6"
#I "../../packages/Fable.Elmish/lib/netstandard1.6"
#r "Fable.Core.dll"
#r "Fable.Elmish.dll"
#r "Fable.Elmish.Browser.dll"

(** Working with query parameters
---------
In addition to working with routes, the library also defines parser combinators for working with `?` queries:

- `<?>` combinator for query parameters,
- `stringParam` combinator to extract a string,
- `intParam` combinator to attempt to parse an `int`.

Some examples:
*)


open Elmish.Browser.UrlParser

type PersonQuery = 
    { name: string
      age: int }

type Route = 
    | Blog of int 
    | Search of string option 
    | Query of PersonQuery option
    with static member FromParams name age = 
        match name,age with
        | Some name, Some age -> Query (Some { name = name; age = age})
        | _ -> Query None


let route : Parser<Route->Route,_>=
    oneOf
        [ map Search (s "blog" <?> stringParam "search")
          map Blog (s "blog" </> i32)
          map (Route.FromParams) (top <?> stringParam "name" <?> intParam "age") ]

(**
Note that unlike route combinators, which fail to match the entire route if some combinator can not be parsed, query param parsers return `Option`.
It's up to you if you decide to accept a query parameter as `None`.

The parser above will resolve:
<pre>
    blog/              ==>  Some (Search None)
    blog?search=cats   ==>  Some (Search (Some "cats"))
    blog/42            ==>  Some (Blog 42)
    ?name=tom&age=42   ==>  Some (Query {name="tom"; age=42})
    ?name=tom          ==>  Some (Query None)
</pre>


#### Full vs hashbang URL queries
Normally, query parameters apply to the full URL and are available via `location.search` property of the browser, that is where `pathParser` will look.
This doesn't work very well with hashbang URLs and `hashParser` looks for `?` in `location.hash` instead.

Keep this difference in mind when formatting your routes.

*)
