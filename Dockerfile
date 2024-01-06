FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final

WORKDIR /app

COPY --from=build /app/out .




EXPOSE 80
ENTRYPOINT ["dotnet", "ESD_EDI_BE.dll"]



#run the image
#docker run -d -p 5000:80 -v /path/to/host/folder:/app/wwwroot -v /path/to/host/logs:/app/logs file-server

#cac buoc de chay:
#build the image
#docker build -t esd_be .

#run redis:
#docker run -d --name redis_database -p 6379:6379 redis:alpine3.16

#run app container:
#docker run -d -p 81:80 --name esd_be --link redis_database:redis -e Cache__RedisConnectionString=redis_database:6379 evilhero/esd_be

