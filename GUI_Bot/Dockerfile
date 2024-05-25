# Use the official ASP.NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

# Use the official ASP.NET Core SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["GUI_Bot.csproj", "./"]
RUN dotnet restore "GUI_Bot.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "GUI_Bot.csproj" -c Release -o /app/build

# Publish the app to the /app/publish directory
FROM build AS publish
RUN dotnet publish "GUI_Bot.csproj" -c Release -o /app/publish

# Use the base image to create the final runtime container
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GUI_Bot.dll"]
