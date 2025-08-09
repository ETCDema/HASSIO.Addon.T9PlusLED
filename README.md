# HASSIO Add-on управления подсветкой для Mini PC T9 Plus

Add-on сделан на платформе NET9 на основе [этого проекта](https://github.com/ju5tas/t9plus_led_control), для работы с Home Assistant Core API используется [проект ETCDema/HASSIO.Supervisor.API](https://github.com/ETCDema/HASSIO.Supervisor.API).

Что может этот add-on:
1. Выключать подсветку
1. Переключать режимы работы
1. Менять скорость и яркость для выбранного режима

## Установка

1. Скачать [пакет](/ETCDema/HASSIO.Addon.T9PlusLED/releases/download/v1/t9plus_led.zip) и распаковать на Home Assistant сервере в папку `/addons/t9plus_led`
1. Открыть магазин дополнений  
   [![Open your Home Assistant instance and show the add-on store.](https://my.home-assistant.io/badges/supervisor_store.svg)](https://my.home-assistant.io/redirect/supervisor_store/)
1. Запустить проверку наличия обновлений
1. В списке локальных дополнений открыть add-on  
   [![Open your Home Assistant instance and show the dashboard of an add-on.](https://my.home-assistant.io/badges/supervisor_addon.svg)](https://my.home-assistant.io/redirect/supervisor_addon/?addon=local_t9plus_led)
1. Установить add-on

## Настройка

Для начала нужно создать элементы управления

* Режим подсветки `input_select.server_led_mode`:
```YAML
options:
  - "Off"
  - Rainbow
  - Breathing
  - ColorCycle
  - Auto
editable: true
icon: mdi:led-strip-variant
friendly_name: Режим подсветки
```
* Яркость подсветки `input_number.server_led_brightness`:
```YAML
initial: 1
editable: true
min: 1
max: 5
step: 1
mode: slider
icon: mdi:brightness-6
friendly_name: Яркость подсветки
```
* Скорость изменения `input_number.server_led_speed`
```YAML
initial: 1
editable: true
min: 1
max: 5
step: 1
mode: slider
icon: mdi:play-speed
friendly_name: Скорость изменения
```

Далее в устройствах смотрим к какому порту подключена подсветка, ищем такие строки:
```
ttyUSB0
/dev/serial/by-id/usb-Itead_Sonoff_Zigbee_3.0_USB_Dongle_Plus_V2_e0f07a9ff74eef11841a51b3174bec31-if00-port0

ttyUSB1
/dev/serial/by-id/usb-1a86_USB_Serial-if00-port0
```
Нам нужна строка, содержащая `USB_Serial`, на моем устройстве используется ttyUSB1.

В конфигурации add-on указываем соответствующие значения:
```YAML
LogLevel: Information
T9PlusLED:
  port: /dev/ttyUSB1
  modeEntityID: input_select.server_led_mode
  brightnessEntityID: input_number.server_led_brightness
  speedEntityID: input_number.server_led_speed
```

## Готово

Теперь можно запустить расширение, попробовать поменять состояние созданных элементов управления и увидеть результат.

## Возможные проблемы

Для детальной диагностики проблем можно переключить уровень логирования на `Debug` или `Trace` - там будет много информации.

### Add-on не может подключиться к WebSocketAPI

Как правило проблема с токеном доступа, в логе будет строка `fail: Auth invalid: <сообщение>`.
В обычном режиме Home Assistant передает токен доступа в переменной окружения `SUPERVISOR_TOKEN`, если по каким-то причинам нет такой переменной,
то можно создать Long time access token и добавить в конфигурацию:
```YAML
API:longAccessToken: тут прописать созданный token
```

Если какие-то другие проблемы с подключением к WebSocketAPI, то это что-то совсем неправильно работает и нужно разбираться с доступностью `ws://supervisor/core/websocket` - лог уровня `Debug` или `Trace` может помочь.

### Add-on не менять состояние подсветки

Такие проблемы обычно сопровождаются ошибками записи в `SerialPort` и нужно попробовать изменить параметр `T9PlusLED:port`, возможно выбран неправильный.

Так же возможно отсутствие событий изменения состояния созданных элементов - нужно проверить соответствие идентификаторов в конфигурации и настройках элементов.
