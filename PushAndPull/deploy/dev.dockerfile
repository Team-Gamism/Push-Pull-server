FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /src
COPY . .
WORKDIR /src/PushAndPull
RUN dotnet restore
ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://+:8080"]
