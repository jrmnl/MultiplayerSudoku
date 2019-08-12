port module WebSocket exposing (send, recieve)

port send : String -> Cmd msg
port recieve : (String -> msg) -> Sub msg