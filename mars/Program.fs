open System
open System.IO
open System.Net.Http
open FSharp.Data

let Host = "api.instagram.com"
let AuthorizePath = "oauth/authorize"
let AccessTokenPath = "oauth/access_token"
let RedirectUri = "https://localhost/"

[<EntryPoint>]
let main argv =
    let clientId = argv[0]
    let clientSecret = argv[1]

    let authorizeUrl = UriBuilder("https", Host)
    authorizeUrl.Path <- AuthorizePath

    let query =
        [ ("client_id", clientId)
          ("redirect_uri", RedirectUri)
          ("scope", "user_profile,user_media")
          ("response_type", "code") ]
        |> List.map (fun (k, v) -> $"%s{k}=%s{v}")
        |> String.concat "&"

    authorizeUrl.Query <- query

    printfn $"{authorizeUrl}"

    let code = Console.ReadLine()
    let request = new MultipartFormDataContent()

    [ ("client_id", clientId)
      ("client_secret", clientSecret)
      ("grant_type", "authorization_code")
      ("redirect_uri", RedirectUri)
      ("code", code) ]
    |> List.map (fun (k, v) -> (new StringContent(v), k))
    |> List.iter request.Add

    let client = new HttpClient()
    let accessTokenUrl = UriBuilder("https", Host)
    accessTokenUrl.Path <- AccessTokenPath

    let response =
        client.PostAsync(accessTokenUrl.ToString(), request)
        |> Async.AwaitTask
        |> Async.RunSynchronously

    let json =
        response.Content.ReadAsStringAsync()
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> JsonValue.Parse

    let token =
        json.TryGetProperty("access_token")
        |> (fun v ->
            match v with
            | None -> raise (InvalidDataException "Access token is not included.")
            | Some token -> token.AsString())

    printfn $"{token}"

    0
