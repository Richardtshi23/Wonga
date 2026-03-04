FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Wonga.Server.csproj", "./"]
RUN dotnet restore "./Wonga.Server.csproj"

COPY . .
RUN dotnet publish "Wonga.Server.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Wonga.Server.dll"]