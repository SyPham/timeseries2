#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TimeSeries2/TimeSeries2.csproj", "TimeSeries2/"]
RUN dotnet restore "TimeSeries2/TimeSeries2.csproj"
COPY . .
WORKDIR "/src/TimeSeries2"
RUN dotnet build "TimeSeries2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TimeSeries2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TimeSeries2.dll"]