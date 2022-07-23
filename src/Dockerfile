FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY BicepBuild/BicepBuild.csproj .
RUN dotnet restore BicepBuild.csproj
COPY ./BicepBuild .
RUN dotnet build BicepBuild.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /out ./
ENTRYPOINT ["dotnet", "BicepBuild.dll"]