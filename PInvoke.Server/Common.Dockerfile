FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["PInvoke.Server/PInvoke.Server.csproj", "PInvoke.Server/"]
RUN dotnet restore "PInvoke.Server/PInvoke.Server.csproj"
COPY . .
WORKDIR "/src/PInvoke.Server"
RUN dotnet build "PInvoke.Server.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "PInvoke.Server.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
CMD ["dotnet", "PInvoke.Server.dll"]