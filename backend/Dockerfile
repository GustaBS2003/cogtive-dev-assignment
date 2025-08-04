FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Cogtive.DevAssignment.Api.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
EXPOSE 443
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "Cogtive.DevAssignment.Api.dll"]