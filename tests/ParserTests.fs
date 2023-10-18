module Elmish.ParserTests

open Swensen.Unquote
open NUnit.Framework
open UrlParser

let log (state: State<_>) : State<_> =
    NUnit.Framework.TestContext.WriteLine("UrlParser state:",
        {|
            unvisited = state.unvisited |> Array.ofList
            visited = state.visited |> Array.ofList
            queryParams = state.args |> Map.toArray
        |})
    state

/// Created using `document.location` on url:
/// https://github.com/elmish/src/parser.fs?key1=value1&key2&key3=value3#L16-L33
let testPathLocation =
    { new Browser.Types.Location with
            member this.assign(url: string): unit = raise (System.NotImplementedException())
            member this.hash
                with get (): string = "#L16-L33"
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.host
                with get (): string = "github.com"
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.hostname
                with get (): string = "github.com"
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.href
                with get (): string = "https://github.com/elmish/src/parser.fs?key1=value1&key2&key3=value3#L16-L33"
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.origin
                with get (): string = "https://github.com"
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.password
                with get (): string = raise (System.NotImplementedException())
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.pathname
                with get (): string = "/elmish/src/parser.fs"
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.port
                with get (): string = ""
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.protocol
                with get (): string = "https:"
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.reload(forcedReload: bool option): unit = raise (System.NotImplementedException())
            member this.replace(url: string): unit = raise (System.NotImplementedException())
            member this.search
                with get (): string = "?key1=value1&key2&key3=value3"
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.toString(): string = raise (System.NotImplementedException())
            member this.username
                with get (): string = raise (System.NotImplementedException())
                and set (v: string): unit = raise (System.NotImplementedException()) }

/// Created using `document.location` on url:
/// https://github.com/root/#/elmish/src/parser.fs?key1=value1&key2&key3=value3
let testHashLocation =
    { new Browser.Types.Location with
            member this.assign(url: string): unit = raise (System.NotImplementedException())
            member this.hash
                with get (): string = "#/elmish/src/parser.fs?key1=value1&key2&key3=value3"
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.host
                with get (): string = "github.com"
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.hostname
                with get (): string = "github.com"
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.href
                with get (): string = "https://github.com/root/#/elmish/src/parser.fs?key1=value1&key2&key3=value3"
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.origin
                with get (): string = "https://github.com"
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.password
                with get (): string = raise (System.NotImplementedException())
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.pathname
                with get (): string = "/root/"
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.port
                with get (): string = ""
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.protocol
                with get (): string = "https:"
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.reload(forcedReload: bool option): unit = raise (System.NotImplementedException())
            member this.replace(url: string): unit = raise (System.NotImplementedException())
            member this.search
                with get (): string = ""
                and set (v: string): unit = raise (System.NotImplementedException())
            member this.toString(): string = raise (System.NotImplementedException())
            member this.username
                with get (): string = raise (System.NotImplementedException())
                and set (v: string): unit = raise (System.NotImplementedException()) }

type GitHubUrl =
    | GitHubUrl of key1: string option * key3: string option

[<Test>]
let ``parseHash works``() =
    let parser : Parser<GitHubUrl -> GitHubUrl, GitHubUrl> =
        log >>
        oneOf [
            (s "elmish" </> s "src" </> s"parser.fs" <?> stringParam "key1" <?> stringParam "key3")
                |> map (fun key1 key3 -> GitHubUrl(key1, key3))
        ]

    let parsedHash = Elmish.UrlParser.parseHash parser testHashLocation

    parsedHash =! Some (GitHubUrl(Some "value1", Some "value3"))

[<Test>]
let ``parsePath works``() =
    let parser : Parser<GitHubUrl -> GitHubUrl, GitHubUrl> =
        log >>
        oneOf [
            (s "elmish" </> s "src" </> s"parser.fs" <?> stringParam "key1" <?> stringParam "key3")
                |> map (fun key1 key3 -> GitHubUrl(key1, key3))
        ]

    let parsedHash = Elmish.UrlParser.parsePath parser testPathLocation

    parsedHash =! Some (GitHubUrl(Some "value1", Some "value3"))
