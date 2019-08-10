import Browser
import List
import Array exposing (..)
import Html exposing (..)
import Html.Attributes exposing (..)
import Html.Events exposing (onInput, onClick)
import Http
import Json.Decode exposing (Decoder, keyValuePairs, int)

-- MAIN


main =
  Browser.element
    { init = init
    , update = update
    , subscriptions = subscriptions
    , view = view
    }


-- MODEL



type alias Model =
  { username:String, page:Page}

type Page
  = SelectName
  | Loading
  | Game Board
  | Leaderboard (List (String, Int))
  | ErrorPage String

type alias Board = Array (Array (Maybe Int))
  
init : () -> (Model, Cmd Msg)
init _ =
  ( { username = "", page = SelectName }
  , Cmd.none
  )


-- UPDATE



type Msg
  = ChangeName String
  | StartGame
  | UpdateCell Int Int String
  | ViewLeaderboard
  | GotLeaderboard (Result Http.Error (List (String, Int)))

update msg model =
  case msg of
    ChangeName newName ->
      ( { username = newName, page = SelectName } , Cmd.none)

    UpdateCell rowIndex columnIndex newValue ->
      case model.page of
        Game board ->
          ( { model | page = board |> updateBoard rowIndex columnIndex newValue |> Game }
          , Cmd.none
          )
        _ ->
          (model, Cmd.none)

    StartGame ->
      ( { model | page = initGame |> Game }
      , Cmd.none
      )
    
    ViewLeaderboard ->
      ({ model | page = Loading } , getLeaderboard)
      
    GotLeaderboard result ->
      case result of
        Ok leaders ->
          ( { model | page = Leaderboard leaders }, Cmd.none)
        Err httpError ->
          ( { model | page = httpError |> toErrorString |> ErrorPage } , Cmd.none)
  
updateBoard rowIndex columnIndex newValue board =
  board
  |> Array.set rowIndex (
    board
    |> Array.get rowIndex
    |> Maybe.map (Array.set columnIndex (toValidSymbol newValue))
    |> Maybe.withDefault Array.empty
    )

initGame =
  [ Nothing, Just 1, Just 23, Nothing, Nothing, Just 33, Nothing, Nothing, Just 32 ]
  |> Array.fromList
  |> Array.repeat 9
      
toValidSymbol str =
  str
  |> String.toInt
  |> Maybe.andThen (\n ->
    if n < 1 || n > 9 then
      Nothing
    else Just n)
    
toErrorString httpError =
  case httpError of
    Http.BadUrl str -> "Bad url: " ++ str
    Http.Timeout -> "Timeout"
    Http.NetworkError -> "Network error"
    Http.BadStatus code -> "Status code: " ++ String.fromInt code
    Http.BadBody str -> "Bad Body: " ++ str


-- SUBSCRIPTIONS


subscriptions model =
  Sub.none


-- VIEW



view model =
  case model.page of
    ErrorPage error ->
      div []
        [ div [] [ text error ]
        , button [ onClick (ChangeName model.username) ] [ text "Back" ]
        ]

    SelectName -> 
      viewGreetings model.username

    Loading ->
      div [] [ text "Loading.." ]

    Game board ->
      div []
        [ table [] (board |> Array.toList |> List.indexedMap drawSudokuRaw)
        , button [ onClick (ChangeName model.username) ] [ text "Back" ]
        ]
      
    Leaderboard leaders ->
      div []
        [ div [] (drawLeaderboard leaders)
        , button [ onClick (ChangeName model.username) ] [ text "Back" ]
        ]
      
drawLeaderboard leaders =
  leaders
  |> List.sortBy (\(_,wins) -> wins)
  |> List.map (\(name,wins) -> [text (name ++ " " ++ (String.fromInt wins))])
  |> List.map (div [])

drawSudokuRaw index cells =
  tr [] (cells |> Array.toList |> List.indexedMap (drawSudokuCell index))

drawSudokuCell rowIndex colIndex cell =
  td [] [div [style "width" "40px", style "height" "40px"] [drawCellContent rowIndex colIndex cell] ]

drawCellContent rowIndex colIndex field =
  input 
    [ placeholder "__"
    , style "text-align" "center"
    , style "border" "none"
    , onInput (UpdateCell rowIndex colIndex)
    , maxlength 1
    , size 1
    , value (field |> Maybe.map String.fromInt |> Maybe.withDefault "")
    , readonly (field /= Nothing)
    ]
    []

viewGreetings username =
  div []
    [ div [] [ text "Enter your name to join game:" ]
    , input [ placeholder "Your name", value username, onInput ChangeName ] []
    , button [ hidden (username == ""), onClick StartGame ] [ text "Join game" ]
    , button [ onClick ViewLeaderboard ] [ text "Leaderboard" ]
    ]


-- HTTP


getLeaderboard : Cmd Msg
getLeaderboard =
  Http.get
    { url = "http://localhost:8001/api/leaderboard"
    , expect = Http.expectJson GotLeaderboard leaderboardDecoder
    }

leaderboardDecoder : Decoder (List (String, Int))
leaderboardDecoder =
  keyValuePairs int