(** UrlParser
------
Parsers designed to be used with `Navigation` module to help folks create
single-page applications (SPAs) where you manage browser navigation yourself.
*)

module Elmish.UrlParser
open Browser.Types
open Elmish.UrlParser

(**
#### Parsers
Parse based on `location.pathname` and `location.search`. This parser
ignores the hash entirely.
*)
let parsePath (parser: Parser<_,_>) (location: Location) =
    let path, search =
        let path =
                if location.pathname.Length > 1 then location.pathname.Substring 1
                else ""
        if path.Contains("?") then
                let p = path.Substring(0, path.IndexOf("?"))
                p, path.Substring(p.Length)
            else
                path, "?"
    parse parser path (parseParams search)

(** Parse based on `location.hash`. This parser ignores the normal
path entirely.
*)
let parseHash (parser: Parser<_,_>) (location: Location) =
    let hash, search =
        let hash =
            if location.hash.Length > 1 then location.hash.Substring 1
            else ""
        if hash.Contains("?") then
            let h = hash.Substring(0, hash.IndexOf("?"))
            h, hash.Substring(h.Length)
        else
            hash, "?"

    parse parser hash (parseParams search)
