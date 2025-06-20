FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
COPY ./src /docker
WORKDIR /docker
RUN dotnet publish -c Release -o out 
CMD ["./out/Avalonix"]

