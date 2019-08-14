FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /build
COPY src/MultiplayerSudoku.sln ./
COPY src/MultiplayerSudoku.Application.Contract/*.csproj ./MultiplayerSudoku.Application.Contract/
COPY src/MultiplayerSudoku.Application/*.csproj ./MultiplayerSudoku.Application/
COPY src/MultiplayerSudoku.Host/*.csproj ./MultiplayerSudoku.Host/
COPY src/MultiplayerSudoku.Logic/*.csproj ./MultiplayerSudoku.Logic/
COPY src/MultiplayerSudoku.Logic.Tests/*.csproj ./MultiplayerSudoku.Logic.Tests/
RUN dotnet restore
COPY src/. ./
RUN dotnet build "MultiplayerSudoku.Host/MultiplayerSudoku.Host.csproj" -c Release -o /app

#FROM build AS test
#RUN dotnet test "MultiplayerSudoku.Logic.Tests/MultiplayerSudoku.Logic.Tests" -c Release

FROM build AS publish
RUN dotnet publish "MultiplayerSudoku.Host/MultiplayerSudoku.Host.csproj" -c Release -o /app

FROM base AS runtime
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MultiplayerSudoku.Host.dll"]