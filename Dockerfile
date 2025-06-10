FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
COPY ./src /docker
WORKDIR /docker
RUN dotnet publish -c Release -o out --self-contained

FROM scratch AS export
COPY --from=build /docker/out /
