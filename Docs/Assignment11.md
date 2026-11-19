# Assignment 11 — Data and Saves

## Как устроен проект

- MainMenu показывает карту трёх локаций, открытых по сохранению.
- LevelLoader.Play выбирает LevelData и открывает общую сцену Game.
- LevelLoader создаёт worldPrefab и врагов из массива enemies, запускает таймер и проверяет цель.
- Assets/Resources/Levels содержит три LevelData. Там меняются позиции врагов, время, цель и награда.
- Assets/Data/Balance содержит параметры персонажа, врагов, снарядов, платформ и аптечек. Компоненты читают stats в Awake. Текущее здоровье хранится в компоненте.
- SaveManager хранит прогресс в памяти. SaveSystem записывает его через JsonUtility в Application.persistentDataPath/save.json.
- AdventureUI показывает карту, настройки, паузу, результат и подтверждение сброса.
- GameAudio использует громкость из SaveManager.

## Сохранение

В JSON находятся версия, лучший результат, результаты уровней, пройденные и открытые уровни, монеты и громкость. Монеты за повторное прохождение уже завершённого уровня повторно не начисляются. Лучший результат уровня обновляется.

Сохранение выполняется после победы, при изменении громкости, потере фокуса и OnApplicationPause(true). Игра сначала записывает save.tmp, затем заменяет save.json. При ошибке чтения используются начальные значения и появляется сообщение в меню. Настройки дублируются в собственных ключах PlayerPrefs Forest.Music и Forest.Effects. Источник прогресса и настроек при запуске — JSON.

Сброс требует подтверждения в игре. Он удаляет save.json и временный файл, свои ключи PlayerPrefs и заменяет данные в памяти. Следующее сохранение записывает начальные значения.

## Проверка перед сдачей

1. Открыть MainMenu. Доступен первый уровень. Пройти его, собрать все монеты и дойти до выхода.
2. Проверить открытие второго уровня. На третьем также победить босса.
3. Изменить громкость. Полностью закрыть установленную игру на телефоне и запустить снова. Проверить прогресс и звук. Unity Remote выполняет игру на компьютере, поэтому отдельно нужна проверка установленной мобильной версии.
4. Закрыть игру. Открыть save.json из persistentDataPath, заменить содержимое на broken, запустить игру. Должны восстановиться начальные значения.
5. Открыть настройки, нажать Reset Progress, затем Cancel. Проверить сохранение прогресса. Повторить и подтвердить Reset.
6. Для демонстрации баланса открыть asset PlayerControllerStats в Assets/Data/Balance, изменить moveSpeed и заново запустить уровень. Код изменять не требуется.
7. Проверить горизонтальную ориентацию, вырез экрана, одновременное удержание джойстика и прыжка, паузу и возврат из фона.

## Видео на английском, 2–3 минуты

Запишите собственный экран и голос. Покажите реальные действия и фактический результат.

“This is Forest Adventure. For Assignment Eleven, I moved the balance values into ScriptableObject assets and added persistent progress.

Here is the level selection screen. Completed levels unlock the next trail. Each trail has its own LevelData asset with a world prefab, enemy positions, a goal, a time limit and score rewards. The same LevelLoader reads all three assets.

Here are the player's stats. I will change the movement speed, then restart the level. The character now moves at the new speed without any code changes. Current health stays on the character, so damage does not change the shared asset.

Now I will complete a level and change the music volume. I will close the app completely on the phone and reopen it. [Show the actual result.] The JSON save stores progress, scores, unlocked levels and settings.

SaveManager decides when to save. SaveSystem writes a temporary JSON file and replaces the previous save. It saves after a level, when settings change and when the app enters the background.

Next I will damage the save file and restart the game. [Show the actual result.] Loading is protected by validation and try-catch. The game shows a message and starts with default values.

The reset button opens a confirmation panel. Cancel keeps the progress. Confirm clears the save, our PlayerPrefs keys and the data in memory.

The script flow is menu to LevelLoader, LevelLoader to LevelData and the gameplay components, and completed gameplay to SaveManager and SaveSystem. AdventureUI displays the result and the saved level access.”

Для сдачи нужны запись с вашим голосом, скриншот data asset в Inspector и текст фактического save.json. Для пользователей iPhone задание допускает короткую запись с телефона вместо APK. Коммиты и отправку в Canvas выполняет автор проекта.

## Выполненные проверки

Проверочная копия Unity 6000.6.1f1 прошла 14 PlayMode-тестов. Они проверяют JSON, повреждённый файл, открытие уровня, сохранение громкости, сброс, загрузку трёх LevelData, прежние проверки урона и управление двумя касаниями. В тесте мультитача проверяется положительная вертикальная скорость после прыжка и сохранение горизонтального движения при втором пальце. Отчёт находится в Assignment11Proof/PlayModeResults.xml.

save-after-reset.json скопирован из фактического сохранения проверочной копии после теста сброса. Это пример формата и начального состояния. Проверка установленной игры на физическом телефоне ещё требуется.

## Проверка основного проекта

При повторном запуске MainMenu загрузился фактический прогресс: три пройденных уровня, лучший результат 2170 и сохранённая громкость эффектов. Копия этого JSON находится в Assignment11Proof/save.json, скриншот Inspector — в Assignment11Proof/LevelData-Inspector.png.

APK пока не получен. На этом компьютере повторная Android-сборка остановилась на BuildPlayerDataGenerator: дочерний cmd.exe находится в состоянии Suspended. Причина внешней приостановки ещё уточняется. Проверка на физическом iPhone и видео с голосом автора остаются отдельными шагами сдачи.
