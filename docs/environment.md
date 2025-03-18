# Переменные окружения:

## *LOG_SAVER_\**

* *LOG_SAVER_BASE_PATH* - *DirectoryString*. Директория для сохранения логов контейнера.

## *HEALTHCHECKS_\**

Часть переменных окружения взято [из](https://github.com/EBCEYS/EBCEYS.ContainersEnvironment?tab=readme-ov-file#healthchecks).

* *HEALTHCHECKS_LABEL_ENABLED_NAME* - *string*. Имя ключа лейбла для проверки ведения проверок здоровья. По умолчанию *healthchecks.enabled*.
* *HEALTHCHECKS_LABEL_PORT* - *string*. Имя ключа лейбла, в котором указывается порт проверок здоровья. По умолчанию *healthchecks.port*.
* *HEALTHCHECKS_LABEL_RESTART_ON_UNHEALTHY* - *string*. Имя ключа лейбла, в котором указывается необходимость рестарта контейнера в случае "нездоровья". По умолчанию *healthchecks.restart*.
* *HEALTHCHECKS_LABEL_IS_EBCEYS* - *string*. Имя ключа **опционального** лейбла, в котором указывается, что хелсчеки берутся из [библиотеки](https://github.com/EBCEYS/EBCEYS.ContainersEnvironment). По умолчанию *healthchecks.isebceys*.
* *HEALTHCHECKS_LABEL_HOSTNAME* - *string*. Имя ключа лейбла, в котором указывается имя хоста или *ip* адрес контейнера. По умолчанию *healthchecks.hostname*.
* *HEALTHCHECKS_SAVE_LOGS_ON_UNHEALTHY* - *true/false*. Сохранять логи контейнера если он "нездоров". По умолчанию *true*.
* *HEALTHCHECKS_PROCESSOR_PERIOD* - *Timespan*. Период работы сервиса опроса проверок здоровья. По умолчанию *00:00:05*.
* *HEALTHCHECKS_USE_PING* - *true/false*. Необходимость добавления проверок здоровья через *ping*. **!!!ВАЖНО!!!** контейнер должен содержать *ping*. По умолчанию *false*.
* *HEALTHCHECKS_SERVICE_CONFIG_FILE* - *FileString*. Путь до файла конфигурации проверок здоровья сервисов. По умолчанию *null*.
* *HEALTHCHECKS_PROCESS_ONLY_IF_CONTAINER_RUNNING* - *true/false*. Запускать процессы обработки контейнера только если его статус *running*. По умолчанию *true*.

## *DOCKER_CONNECTION_\**

* *DOCKER_CONNECTION_USE_DEFAULT* - *true/false*. По умолчанию *true*. Устанавливать связь с докером с путем по умолчанию (*unix:///var/run/docker.sock* - для linux).
* *DOCKER_CONNECTION_URL* - URL для подключения к *docker*. По умолчанию *unix:///var/run/docker.sock*. Игнорируется если *DOCKER_CONNECTION_USE_DEFAULT=true*.
* *DOCKER_CONNECTION_DEFAULT_TIMEOUT* - таймаут подключения к *docker*. По умолчанию *00:00:10*.