FROM mcr.microsoft.com/dotnet/sdk:9.0 AS runtime
WORKDIR /source

COPY ../NewsBackOffice.sln ./
COPY ./NewsBackOffice/*.csproj ./NewsBackOffice/
RUN dotnet restore

COPY ./NewsBackOffice/. ./NewsBackOffice/

WORKDIR /source/NewsBackOffice
# Publish
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy van build stage
COPY --from=runtime /app ./
EXPOSE 5000
ENTRYPOINT ["dotnet", "NewsBackOffice.dll"]

