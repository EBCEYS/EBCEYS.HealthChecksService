# Конфигурационный файл *appsettings.json*

В конфигурационном файле описаны настройки логирования с использованием библиотеки [*NLog*](https://github.com/NLog/NLog).

Дополнительно поддерживаются все стандартные настройки *asp.net*.

```json
{
    "NLog": {
        "autoReload": true,
        "throwConfigExceptions": true,
        "internalLogLevel": "Info",
        "extensions": [
            { "assembly": "NLog.Extensions.Logging" },
            { "assembly": "NLog.Web.AspNetCore" }
        ],
        "variables": {
            "msg01": "${message} ${onexception:${newline}${exception:maxInnerExceptionLevel=10:format=tostring,data}}",
            "msg02": "${replace-newlines:replacement=\r\n\t:${msg01}}",
            "msg03": "[${date:format=MM.dd HH\\:mm\\:ss.fff} ${uppercase:${level}:padding=-1:fixedLength=true}<${pad:padCharacter=0:padding=2:fixedLength=true:inner=${threadid}}>] ${pad:padding=-40:fixedLength=true:inner=${callsite:className=False:fileName=True:includeSourcePath=False:methodName=True:cleanNamesOfAnonymousDelegates=False}} ${msg02}"
        },
        "targets": {
            "async": true,
            "own-console": {
                "type": "LimitingWrapper",
                "interval": "00:00:01",
                "messageLimit": 100,
                "target": {
                    "type": "ColoredConsole",
                    "layout": "${msg03}"
                }
            }
        },
        "rules": {
            "0": {
                "logger": "Microsoft.*",
                "maxLevel": "Info",
                "final": true
            },
            "1": {
                "logger": "*",
                "minLevel": "Debug",
                "writeTo": "own-console"
            }
        }
    }
}
```