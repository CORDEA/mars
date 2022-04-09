open System

let Host = "api.instagram.com"
let AuthorizePath = "oauth/authorize"
let AccessTokenPath = "oauth/access_token"
let RedirectUri = "https://localhost"

[<EntryPoint>]
let main argv =
    let clientId = argv[0]

    let builder = UriBuilder("https", Host)
    builder.Path <- AuthorizePath

    let query =
        [ ("client_id", clientId)
          ("redirect_uri", RedirectUri)
          ("scope", "user_profile,user_media")
          ("response_type", "code") ]
        |> List.map (fun (k, v) -> $"%s{k}=%s{v}")
        |> String.concat "&"

    builder.Query <- query

    printfn $"{builder}"

    0
