# MultiplayerSudoku

It's multiplayer sudoku game. Rules are the same but wins who faster.

- Frontend: Elm
- Backend: C#, .NET Core

# How to run

## Docker (tested on linux containers)
1) Make sure you have already installed both [Docker Engine](https://docs.docker.com/install/) and [Docker Compose](https://docs.docker.com/compose/install/).
2) open repository root in terminal
3) run 'docker-compose up'
4) open 'http://localhost:8000/index.html'

## Debug
1) configure debug profile as console application and change 'applicationUrl' in 'Properties/launchSettings.json' to 'localhost:8001'
1) build and run 'backend/src/MultiplayerSudoku.Host/MultiplayerSudoku.Host.csproj'
2) open 'frontend/index.html'
