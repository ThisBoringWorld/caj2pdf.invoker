#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0-focal AS base
WORKDIR /app

RUN echo 'deb http://mirrors.aliyun.com/ubuntu/ focal main restricted universe multiverse\n\
deb-src http://mirrors.aliyun.com/ubuntu/ focal main restricted universe multiverse\n\
deb http://mirrors.aliyun.com/ubuntu/ focal-security main restricted universe multiverse\n\
deb-src http://mirrors.aliyun.com/ubuntu/ focal-security main restricted universe multiverse\n\
deb http://mirrors.aliyun.com/ubuntu/ focal-updates main restricted universe multiverse\n\
deb-src http://mirrors.aliyun.com/ubuntu/ focal-updates main restricted universe multiverse\n\
deb http://mirrors.aliyun.com/ubuntu/ focal-proposed main restricted universe multiverse\n\
deb-src http://mirrors.aliyun.com/ubuntu/ focal-proposed main restricted universe multiverse\n\
deb http://mirrors.aliyun.com/ubuntu/ focal-backports main restricted universe multiverse\n\
deb-src http://mirrors.aliyun.com/ubuntu/ focal-backports main restricted universe multiverse\n'\
> /etc/apt/sources.list && apt update && apt install -y python3 --no-install-recommends --no-install-suggests && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["caj2pdf.testconsole/caj2pdf.testconsole.csproj", "caj2pdf.testconsole/"]
COPY ["caj2pdf.invoker/caj2pdf.invoker.csproj", "caj2pdf.invoker/"]
COPY ["caj2pdf.runtime.ubuntu20.x64/caj2pdf.runtime.ubuntu20.x64.csproj", "caj2pdf.runtime.ubuntu20.x64/"]
RUN dotnet restore "caj2pdf.testconsole/caj2pdf.testconsole.csproj"
COPY . .
WORKDIR "/src/caj2pdf.testconsole"
RUN dotnet build "caj2pdf.testconsole.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "caj2pdf.testconsole.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "caj2pdf.testconsole.dll"]