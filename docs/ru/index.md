---
layout: default
title: Xiletrade
lang: ru-RU
minidesc: Наложение POE и проверка цен
description: Инструмент для проверки цен и вспомогательный инструмент для игр Path Of Exile
strings:
  maintained: проект поддерживается
  generated: Эта страница была сгенерирована
  download: Загрузите
  repository: Просмотреть на GitHub
  footer: Бесплатный сервис, требующий постоянного обслуживания для правильной работы.
  issue: и используйте
  latest: последнюю версию
  support: Пожертвовать
---
{% include screenshot.html %}
## Один шаг, чтобы проверить цену

*Мгновенно найдите лучший ценовой диапазон.*  
*Настройте поведение на лету.*  
*Играйте плавнее, торгуйте быстрее!*  

## {% include youtube.svg %} &nbsp; [Посмотрите краткий обзор](https://youtu.be/NygCZvZyUX0)<br>

Работает эффективно с небольшими ресурсами ЦП.  
Использует макс ***250 МБ ОЗУ*** без скачков выделения памяти. 
***НЕТ ДАННЫХ*** пишется в фоновом режиме.  

* * *

# {% include pokeball.svg %} Основные функции

- Быстрая **проверка цен** для всех товаров с помощью определенного сочетания клавиш ***CTRL+D*** *по умолчанию*.
- Быстро откройте **Настройки** с помощью ***CTRL+R***, чтобы изменить поведение проверки цен.
- **Массовый обмен предметами** для быстрой шепотной торговли через POE.
- **Дополнения** для помощи в ценообразовании предметов и получения дополнительной информации о связанных предметах.
	- Мягкая связь с **poe.prices**, **poeninja**, **poewiki** и **poedb**.
- Проверяет наличие **новых обновлений** и обновляет пользовательские/официальные **фильтры** при запуске.
- Функции, соответствующие правилу **одиночного действия** из [Политики третьих сторон](https://www.pathofexile.com/developer/docs#policy):
	- Взаимодействуйте с **консолью чата**, отправляя предопределенные сообщения с помощью связанной **горячей клавиши**,
например, быстрое перемещение в **убежища** или действия **торговли**.
	- Привязка **колеса мыши** к **щелчку левой кнопки мыши** по требованию.
- Утилита **Regex manager**.
	- Управление определенным списком **регулярных выражений**.
	- Автоматическая вставка в панель выделения.  

{% include regex.html %}
<br>

## {% include install.svg %} Установка и настройка

1. [**Загрузите**]({% include download.html %}) последнюю версию и **извлеките** архив **`.7zip`** в нужный каталог.
Xiletrade — это **переносимое приложение**, которое не устанавливает ничего дополнительного при запуске.
2. **Щелкните правой кнопкой мыши** по значку в системном трее, чтобы настроить или закрыть программу.
3. Выберите нужную **версию игры**, **язык** и **лигу** в окне запуска или настроек.
4. **Наведите указатель мыши** на каждую функцию, пока не появится подсказка, чтобы получить подробное объяснение.  
<br>
{% include settings.html %}
<br>

* * *
> # {% include mouse.svg %} Как это работает
>
> Запустите игру в **оконном** или **полноэкранном режиме без рамок**, чтобы она работала правильно.
> Убедитесь, что **язык** и **лига** Xiletrade соответствуют настройкам игры.
> После запуска и настройки Xiletrade вы можете **проверить цены**, выполнив следующий процесс:
>   1. Наведите указатель мыши **на предмет** в игре и нажмите ***CTRL+D*** *(по умолчанию)*
>   2. Он скопирует **описания информации о предмете** из игры и откроет окно Xiletrade.
>   3. В окне будут отображаться **предполагаемые цены**, если поиск вернет соответствующие результаты.
>   4. Отображаемые цены основаны на **официальных торговых сайтах** [PoE 1](https://www.pathofexile.com/trade/search/) и [PoE 2](https://www.pathofexile.com/trade2/search/poe2/).
<br>

### {% include chip.svg %} Настраиваемые поведения

* **Перетащите главное окно** в любое место на экране.
* Поиск по **текущим** значениям элементов или по **минимальному значению** в уровне.
* Измените числовые значения с помощью **колесика мыши** (мин/макс)
	* Удерживайте клавишу **CTRL** или **SHIFT** для десятичных значений.
* Отрегулируйте **непрозрачность** и **автоматическое закрытие**, когда окно теряет фокус.
	* Нажмите на значок **верхнего левого угла** главного окна.
* **Выделите** **дорогие** и **опасные** моды** на картах (в файле конфигурации).
* **Автоматическая вставка** обмена шепотом в игре с внешних сайтов.

```
Разработано для соблюдения правил, установленных официальным сайтом торговли,
чтобы избежать злоупотреблений с ограниченным восстановлением данных и запросами с течением времени.
```
* * *

## {% include finger.svg %} FAQ

<p class="accordion"><b>Поддерживает ли приложение все языки?</b></p>
<div class="panel">
<b>Да</b>, оно было разработано для поддержки всех языков клиента Path of Exile.
</div>

<p class="accordion"><b>Безопасно ли загружать приложение с другого веб-сайта?</b></p>
<div class="panel">
По очевидным причинам безопасности настоятельно рекомендуется загружать Xiletrade из основного репозитория или связанного с ним веб-сайта github.io.
</div>

<p class="accordion"><b>Я знаю, что использование стороннего приложения может привести к блокировке игры. Так ли это в случае с Path of Exile с Xiletrade?</b></p>
<div class="panel">
<b>Нет</b>, Xiletrade был впервые выпущен в 2020 году, и пока вы используете версию, опубликованную на этом сайте, вы никогда не рискуете быть забаненным, если политика GGG допускает использование таких инструментов.
<br>Обратите внимание, что издатель игры никогда не гарантирует наличие стороннего инструмента.
Соответствие предлагаемого инструмента правилам лежит на организаторе проекта.
<br>Я предлагаю вам ознакомиться с <a target="_blank" rel="noopener noreferrer" href="https://www.pathofexile.com/developer/docs#policy">условиями использования</a>, чтобы сопоставить информацию.
</div>

<p class="accordion"><b>Почему программное обеспечение бесплатное?</b></p>
<div class="panel">
Xiletrade — это <b>отдельное программное обеспечение</b>, которое не предназначено для использования в целях получения прибыли путем интеграции рекламы, как это принято. Однако вы можете делать <a target="_blank" rel="noopener noreferrer" href="{{ site.github.paypal_url }}">пожертвования</a> по своему усмотрению. Мы будем признательны, если вам понравится проект, и спасибо вам за это 😊.
</div>