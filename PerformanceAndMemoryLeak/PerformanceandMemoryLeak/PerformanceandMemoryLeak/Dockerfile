# RUN dotnet restore
# RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim
WORKDIR /app

COPY bin/Release/net5.0/publish/ .

ENTRYPOINT ["dotnet", "PerformanceandMemoryLeak.dll"]