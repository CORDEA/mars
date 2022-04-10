open System
open System.IO
open System.Net.Http
open FSharp.Data
open FSharpx.Control

let AuthorizeHost = "api.instagram.com"
let AuthorizePath = "oauth/authorize"
let AccessTokenPath = "oauth/access_token"
let RedirectUri = "https://localhost/"
let GraphApiHost = "graph.instagram.com"
let UserPath = "me"

let buildQuery (values: (string * string) list) : string =
    values
    |> List.map (fun (k, v) -> $"%s{k}=%s{v}")
    |> String.concat "&"

[<EntryPoint>]
let main argv =
    let clientId = argv[0]
    let clientSecret = argv[1]

    let authorizeUrl =
        UriBuilder(Uri.UriSchemeHttps, AuthorizeHost)

    authorizeUrl.Path <- AuthorizePath

    let authorizeQuery =
        [ ("client_id", clientId)
          ("redirect_uri", RedirectUri)
          ("scope", "user_profile,user_media")
          ("response_type", "code") ]
        |> buildQuery

    authorizeUrl.Query <- authorizeQuery

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

    let accessTokenUrl =
        UriBuilder(Uri.UriSchemeHttps, AuthorizeHost)

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


    let meUrl =
        UriBuilder(Uri.UriSchemeHttps, GraphApiHost)

    meUrl.Path <- UserPath

    meUrl.Query <-
        [ ("fields", "username")
          ("access_token", token) ]
        |> buildQuery

    let user =
        client.GetAsync(meUrl.ToString())
        |> Async.AwaitTask
        |> Async.bind (fun v -> v.Content.ReadAsStringAsync() |> Async.AwaitTask)
        |> Async.map JsonValue.Parse
        |> Async.RunSynchronously

    let name =
        user.TryGetProperty("username")
        |> (fun v ->
            match v with
            | None -> raise (InvalidDataException "Username is not included.")
            | Some name -> name.AsString())

    printfn $"Hello {name}!"

    0
