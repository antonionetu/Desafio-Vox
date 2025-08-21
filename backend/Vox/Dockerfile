FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Vox/*.csproj Vox/
COPY Vox.Tests/*.csproj Vox.Tests/

RUN dotnet restore Vox/Vox.csproj

COPY . .

RUN dotnet publish Vox/Vox.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Vox.dll"]
