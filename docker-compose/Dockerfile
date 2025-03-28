FROM docker.io/ebceys/app-starter:1.0.0 AS starter

FROM docker.io/ebceys/healthchecks:1.0.0 AS healthchecks

FROM ubuntu

RUN apt-get update && \
    apt-get install libicu-dev -y && \
    rm -rf /var/lib/apt/lists/*

ARG APPSTARTER_DIR=/appstarter
ARG APPSTARTER_EXECFILE=${APPSTARTER_DIR}/EBCEYS.Container-AppStarter
ARG HEALTHCHECK_PORT=8080

COPY --from=starter ${APPSTARTER_DIR} ${APPSTARTER_DIR}
COPY --from=healthchecks /app /app

ENV ASPNETCORE_URLS=http://*:${HEALTHCHECK_PORT}

ENV CONFIGURATION_BREAK_START_IF_NO_CONFIGS=true
ENV APP_STARTER_EXECUTION_FILE=/app/EBCEYS.HealthChecksService
ENV APP_STARTER_RESTART_APP_ON_CONFIG_UPDATE=true
ENV APP_STARTER_ENABLE_HEALTHCHECKS=false
ENV CONFIGURATION_CONTAINER_TYPE_NAME=healthchecks
ENV HEALTHCHECKS_ENABLE=true
ENV HEALTHCHECKS_STARTING_PORT=${HEALTHCHECK_PORT}
ENV CONFIGURATION_REQUEST_RETRIES=3
ENV CONFIGURATION_REQUEST_DELAY=00:00:05

ENTRYPOINT [ "/appstarter/EBCEYS.Container-AppStarter" ]