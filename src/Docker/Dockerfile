FROM microsoft/aspnetcore:2.0 AS python
WORKDIR /app
RUN apt update
RUN apt install -y apt-utils
RUN apt install -y nginx
RUN apt install python-pip -y
RUN pip install psutil
EXPOSE 80
EXPOSE 5000

FROM microsoft/aspnetcore-build:2.0 AS allbuild
WORKDIR /src
COPY *.sln ./
COPY Frontend/Frontend.csproj Frontend/
COPY Common/DTO/DTO.csproj Common/DTO/
COPY Common/Utilities/Utilities.csproj Common/Utilities/
COPY Services/Common/ServicesCommon.csproj Services/Common/
COPY Services/JobDispatcher/JobDispatcher.csproj Services/JobDispatcher/
COPY Services/NodeAgent/NodeAgent.csproj Services/NodeAgent/
COPY Bootstrap/Bootstrap.csproj Bootstrap/
RUN dotnet restore
COPY . .
WORKDIR /src/Frontend
RUN dotnet publish -c Release -o /app/Frontend
WORKDIR /src/Bootstrap
RUN dotnet publish -c Release -o /app/Bootstrap
WORKDIR /src/Services/JobDispatcher
RUN dotnet publish -c Release -o /app/JobDispatcher
WORKDIR /src/Services/NodeAgent
RUN dotnet publish -c Release -o /app/NodeAgent

FROM ubuntu AS angular
RUN apt update
RUN apt install -y apt-utils
RUN apt install -y curl
RUN curl -sL https://deb.nodesource.com/setup_8.x | bash -
RUN apt-get install -y nodejs
RUN npm install --unsafe-perm -g @angular/cli

FROM angular AS portalbuild
WORKDIR /src
COPY portal/ portal/
WORKDIR /src/portal
RUN npm install
RUN ng build --environment=prod

FROM python AS final
WORKDIR /app
COPY --from=portalbuild /src/portal/dist /app/portal
COPY --from=allbuild /app .
RUN echo 'server { \n\
	root /app/portal; \n\
	listen 8080; \n\
}' > /etc/nginx/conf.d/hpcacm.conf
ENTRYPOINT ["dotnet", "Bootstrap/Bootstrap.dll"]
