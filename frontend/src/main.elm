module Main exposing (main)

import WebSocket
import Browser
import Array exposing (Array)
import Html exposing (Html, div, input, text, button, table, tr, td, h1)
import Html.Attributes exposing (..)
import Html.Events exposing (onInput, onClick)
import Http
import Json.Decode as Decode exposing (Decoder) 
import Json.Encode as Encode


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
  | Winner (Maybe String)
  | Leaderboard (List (String, Int))
  | ErrorPage String

type alias Board = Array (Array (Maybe Int))
  
init : () -> (Model, Cmd Msg)
init _ =
  ( { username = "", page = SelectName }
  , Cmd.none
  )


-- UPDATE


type GameUpdate
  = NewState Board
  | NewUpdate Int Int Int
  | EndOfGame (Maybe String)
  | Error String
  | ConnectionLost
  | NameConflict String

type Msg
  = ChangeName String
  | StartGame
  | GameUpdate GameUpdate
  | UpdateCell Int Int String
  | ViewLeaderboard
  | GotLeaderboard (Result Http.Error (List (String, Int)))

update msg model =
  case msg of
    ChangeName newName ->
      ( { username = newName, page = SelectName }
      , Cmd.none)

    UpdateCell row col val ->
      case String.toInt val of
        Just strVal ->
          ( model
          , updateCmd model.username row col strVal)
        Nothing ->
          ( model, Cmd.none)

    StartGame ->
      ( { model | page = Loading }
      , connectCmd model.username)
    
    ViewLeaderboard ->
      ( { model | page = Loading }
      , getLeaderboard)
      
    GotLeaderboard result ->
      case result of
        Ok leaders ->
          ( { model | page = Leaderboard leaders }
          , Cmd.none)
        Err httpError ->
          ( { model | page = httpError |> toErrorString |> ErrorPage }
          , Cmd.none)

    GameUpdate updateMsg -> 
      case (updateMsg, model.page) of
        (NewState board, Loading) ->
          ( {model | page = Game board}
          , Cmd.none)
        (NewUpdate row col newVal, Game board) ->
          ( {model | page = board |> updateBoard row col newVal |> Game }
          , Cmd.none)
        (EndOfGame winner, Game board) ->
          ( {model | page = Winner winner}
          , Cmd.none)
        (Error error, _) ->
          ( {model | page = ErrorPage error}
          , Cmd.none)
        (ConnectionLost, Game _) ->
          ( {model | page = ErrorPage "Connection lost"}
          , Cmd.none)
        (NameConflict name, Loading) ->
          ( {model | page = ErrorPage ("Name: '" ++ name  ++"' already in use")}
          , Cmd.none)
        _ ->
          (model, Cmd.none)
  
updateBoard rowIndex columnIndex newValue board =
  board
  |> Array.set rowIndex (
    board
    |> Array.get rowIndex
    |> Maybe.map (Array.set columnIndex (Just newValue))
    |> Maybe.withDefault Array.empty
    )
      
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

-- COMMANDS

updateCmd username row col val =
  encodeUpdate username row col val
  |> Encode.encode 0
  |> WebSocket.send 

connectCmd username =
  encodeConnect username
  |> Encode.encode 0
  |> WebSocket.send 


-- SUBSCRIPTIONS

gameUpdate str =
  str
  |> Decode.decodeString gameUpdateDecoder
  |> Result.mapError Decode.errorToString
  |> Result.withDefault (Error "smth broken")
  |> GameUpdate

subscriptions : Model -> Sub Msg
subscriptions model =
  WebSocket.recieve gameUpdate

-- VIEW

view model =
  case model.page of
    Winner winner -> 
      div [] 
        [ drawWinner winner
        , button [ onClick StartGame ] [ text "New game" ]
        , button [ onClick ViewLeaderboard ] [ text "Leaders" ]
        ]

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
      div [] [ table [] (board |> Array.toList |> List.indexedMap drawSudokuRaw) ]
      
    Leaderboard leaders ->
      div []
        [ div [] (drawLeaderboard leaders)
        , button [ onClick (ChangeName model.username) ] [ text "Back" ]
        ]
      
drawWinner winner =
  case winner of
    Just someWinner ->
      text (someWinner ++ " wins!")
    Nothing->
      text "No one wins! Impossible to continue game"

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
    , Html.Attributes.value (field |> Maybe.map String.fromInt |> Maybe.withDefault "")
    , readonly (field /= Nothing)
    ]
    []

viewGreetings username =
  div []
    [ div [] [ text "Enter your name to join game:" ]
    , input [ placeholder "Your name", Html.Attributes.value username, onInput ChangeName ] []
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


-- JSON ENCODE

encodeConnect username =
  Encode.object
    [ ( "type", Encode.string "connect")
    , ( "username", Encode.string username)
    ]

encodeUpdate username row col val =
  Encode.object
    [ ( "type", Encode.string "update")
    , ( "username", Encode.string username)
    , ( "row", Encode.int row)
    , ( "column", Encode.int col)
    , ( "value", Encode.int val)
    ]

-- JSON DECODE

leaderboardDecoder : Decoder (List (String, Int))
leaderboardDecoder =
  Decode.keyValuePairs Decode.int

gameUpdateDecoder : Decoder GameUpdate
gameUpdateDecoder =
  Decode.field "type" Decode.string
  |> Decode.andThen (\str ->
    case str of
      "state" ->
        Decode.map NewState
          (Decode.field "sudokuBoard" (Decode.array (Decode.array (Decode.maybe Decode.int))))
      "new" ->
        Decode.map3 NewUpdate
          (Decode.field "row" Decode.int)
          (Decode.field "column" Decode.int)
          (Decode.field "value" Decode.int)
      "end" -> 
        Decode.map EndOfGame
          (Decode.field "winner" (Decode.maybe Decode.string))
      "disconnected" -> 
        Decode.succeed ConnectionLost
      "nameconflict" -> 
        Decode.map NameConflict
          (Decode.field "name" Decode.string)
      _ ->
        Decode.map Error Decode.string
  )