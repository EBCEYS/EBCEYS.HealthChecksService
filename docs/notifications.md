# Оповещения

Сервис может отправлять уведомления о "нездоровых сервисах".

На данный момент реализована отправка по Telegram.

## Telegram

Для включения функционала необходимо указать переменную окружения *NOTIFICATIONS_TELEGRAM_APIKEY* и указать путь до базы данных в *NOTIFICATIONS_TELEGRAM_DB_PATH* (либо использовать путь по умолчанию).
Подробнее см [тут](environment.md#notifications_).

Команды бота:
```
/start - Use to subscribe on health checks notifications
/unsub - unsubscribe from notifications
/getallsubs - gets all subscribers
/help - Displays the available commands
```
* */help* - вывод подсказки со всеми возможными командами.
* */start* - подписка на получение уведомлений.
* */unsub* - отписаться от получения уведомлений.
* */getallsubs* - получение списка всех подписанных пользователей.