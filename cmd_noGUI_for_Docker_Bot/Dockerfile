# Use the official .NET SDK image as a build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GUI_Bot.csproj", "./"]
RUN dotnet restore "GUI_Bot.csproj"
COPY . .
RUN dotnet build "GUI_Bot.csproj" -c Release -o /app/build
RUN dotnet publish "GUI_Bot.csproj" -c Release -o /app/publish

# Use the official .NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GUI_Bot.dll"]
