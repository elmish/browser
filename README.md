Elmish-browser: browser extras for Elmish apps.
=======
[![Windows Build](https://ci.appveyor.com/api/projects/status/rrtg4fxbt7lqbayv?svg=true)](https://ci.appveyor.com/project/et1975/browser) [![Mono/OSX build](https://travis-ci.org/elmish/browser.svg?branch=master)](https://travis-ci.org/elmish/browser) [![NuGet version](https://badge.fury.io/nu/Fable.Elmish.Browser.svg)](https://badge.fury.io/nu/Fable.Elmish.Browser)

Elmish-browser implements routing and navigation for Fable apps targeting web browsers.

For more information see [the docs](https://elmish.github.io/browser).

## Installation

```shell
paket add Fable.Elmish.Browser
```

## Porting from previous version of the parser
In addition to providing query parsing capabilities, this port from Elm/url-parser makes a few changes to the API:
- `format` has been renamed `map`
- `Result` return type has been replaced with `Option`
- `parseHash` is already provided, just pass your parser as its arg
- new `parsePath` works with entire url, not just the hash portion

If you've been using `Result` type for other purposes, it is now available in F# 4.1 Core.
