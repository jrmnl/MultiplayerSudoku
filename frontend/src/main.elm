import Browser
import List
import Array exposing (..)
import Html exposing (..)
import Html.Attributes exposing (..)
import Html.Events exposing (onInput, onClick)

main =
  Browser.sandbox { init = init, update = update, view = view }

-- MODEL

type alias Model =
  { username:String, page:Page}

type Page
  = SelectName
  | Loading
  | Game Board
  | Leaderboard (List (String, Int))

type alias Board = Array (Array (Maybe Int))
  
init = { username = "", page = SelectName }

-- UPDATE

type Msg
  = ChangeName String
  | StartGame
  | UpdateCell Int Int String
  | ViewLeaderboard
  
update msg model =
  case msg of
    ChangeName newName ->
      { username = newName, page = SelectName } 

    UpdateCell rowIndex columnIndex newValue ->
      case model.page of
        Game board ->
          { model | page =
            board
            |> Array.set rowIndex (
              board
              |> Array.get rowIndex
              |> Maybe.map (Array.set columnIndex (toValidSymbol newValue))
              |> Maybe.withDefault Array.empty)
            |> Game
          }
        _ -> { model | username = "wtf" }

    StartGame ->
      { model | page =
        [ Nothing, Just 1, Just 23, Nothing, Nothing, Just 33, Nothing, Nothing, Just 32 ]
        |> Array.fromList
        |> Array.repeat 9
        |> Game
      }
    
    ViewLeaderboard ->
      { model | page = Leaderboard [("Qwerty", 0)] }
      
toValidSymbol str =
  str
  |> String.toInt
  |> Maybe.andThen (\n ->
    if n < 1 || n > 9 then
      Nothing
    else Just n)


updateListAt i x list  =
  list |> mapListAt i (\elem -> elem) 
    
mapListAt i map list =
  case list of
    [] -> []
    head::tail -> 
      if i == 0 then
        (map head) :: tail
      else
        head :: (mapListAt (i - 1) map tail)

-- VIEW

view model =
  case model.page of
    SelectName -> 
      viewGreetings model.username

    Loading ->
      div [] [ text "Loading.." ]

    Game board ->
      div []
        [ table [] (board |> Array.toList |> List.indexedMap drawSudokuRaw)
        , button [ onClick (ChangeName model.username) ] [ text "Back" ]
        ]
      
    Leaderboard leaderboard ->
      div []
        [ div [] (List.map (\(u,a) -> div [] [text u]) leaderboard)
        , button [ onClick (ChangeName model.username) ] [ text "Back" ]
        ]
      

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