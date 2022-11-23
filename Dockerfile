ARG dotnetversion=7.0
FROM mcr.microsoft.com/dotnet/sdk:${dotnetversion} as build
WORKDIR /build
RUN git clone --depth=1 https://github.com/Coflnet/HypixelSkyblock.git dev
WORKDIR /build/sky
COPY SkyChat.csproj SkyChat.csproj
RUN dotnet restore
COPY . .
RUN dotnet publish -c release

FROM mcr.microsoft.com/dotnet/aspnet:${dotnetversion}
WORKDIR /app

COPY --from=build /build/sky/bin/release/net${dotnetversion}/publish/ .

ENV ASPNETCORE_URLS=http://+:8000

RUN useradd --uid $(shuf -i 2000-65000 -n 1) app
USER app

ENTRYPOINT ["dotnet", "SkyChat.dll", "--hostBuilder:reloadConfigOnChange=false"]
