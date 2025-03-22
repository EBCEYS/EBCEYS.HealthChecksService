FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /src

ARG PROJECT_PATH=EBCEYS.HealthChecksService
ARG PROJECT_FILE="EBCEYS.HealthChecksService.csproj"

ARG RUNTIME=linux-x64

COPY ./${PROJECT_PATH}/${PROJECT_FILE} ./

RUN dotnet restore ${PROJECT_FILE} --runtime ${RUNTIME}
COPY ./EBCEYS.HealthChecksService .
RUN dotnet publish ${PROJECT_FILE} -c Release -o /app/publish \
  --no-restore \
  /p:UseAppHost=true \
  --runtime ${RUNTIME} \
  --self-contained true \
  /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0

ARG HELATHCHECK_PORT=8080
ENV ASPNETCORE_URLS=http://*:$HELATHCHECK_PORT
ENV HEALTHCHECKS_STARTING_PORT=$HELATHCHECK_PORT

WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT [ "./EBCEYS.HealthChecksService" ]