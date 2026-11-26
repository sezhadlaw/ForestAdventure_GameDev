# Forest Adventure

Учебный игровой проект на Unity. Проект разрабатывается с учётом мобильного интерфейса и адаптации под разные размеры экранов.

## Запуск проекта

1. Скачайте или клонируйте репозиторий.
2. В Unity Hub нажмите **Add → Add project from disk** и выберите папку проекта.
3. Используйте версию Unity, указанную в `ProjectSettings/ProjectVersion.txt`.
4. Дождитесь завершения импорта ресурсов.
5. Откройте нужную сцену из папки `Assets` и нажмите **Play**.

## Структура репозитория

- `Assets/` — ресурсы, сцены и скрипты игры.
- `Packages/` — зависимости проекта.
- `ProjectSettings/` — настройки Unity.
- `Docs/` — документация проекта.

## Требования к интерфейсу

- Три экрана с работающими переходами.
- Адаптация под три целевых устройства.
- Настройка Canvas Scaler, якорей и Safe Area.
- Удобное расположение кнопок и читаемый текст.
- Единый визуальный стиль.

## Контроль версий

В репозитории используется Unity `.gitignore`. Временные папки Unity и файлы сборок исключены из отслеживания.

Файлы `.meta` необходимо сохранять вместе с соответствующими ресурсами.

## Assignment 12 — Sound Pass

Начинайте со сцены `Assets/MainMenu.unity`. Звук устроен так:

- `Assets/Resources/Audio/GameMix.mixer` — AudioMixer: Master → Music → Tracks, Master → SFX, Master → UI. Открытые параметры `MasterVol`, `MusicVol`, `SFXVol`. Снапшоты Normal и Paused (Tracks −6 dB и Lowpass Simple 800 Hz).
- `SoundManager` — все эффекты через 8 AudioSource с Output = SFX и один с Output = UI. Случайная высота 0.92–1.08 и 2–3 версии клипа.
- `MusicManager` — две музыкальные дорожки с кроссфейдом 1 с, громкость в dB (Log10 × 20), снапшот паузы.
- Громкость Master, Music и Effects хранится в `save.json` вместе с прогрессом.

Подробности, проверка и текст для видео — в `Docs/Assignment12.md`.

## Sounds and music (sources and licenses)

Все звуки и обе мелодии — собственная работа автора проекта. Они синтезированы скриптом `Tools/make_sounds.py`: только стандартная библиотека Python, без сэмплов, записей и чужих паков. Лицензия — **CC0 1.0** (public domain). Повторная генерация: `python Tools/make_sounds.py`.

| Файл (`Assets/Resources/Audio/`) | Событие | Источник | Лицензия |
|---|---|---|---|
| `Sfx/jump_1.wav`, `Sfx/jump_2.wav` | прыжок | own work, `Tools/make_sounds.py` | CC0 |
| `Sfx/land.wav` | приземление | own work, `Tools/make_sounds.py` | CC0 |
| `Sfx/shoot_1.wav`, `Sfx/shoot_2.wav` | выстрел игрока | own work, `Tools/make_sounds.py` | CC0 |
| `Sfx/hurt_1-3.wav` | урон игроку | own work, `Tools/make_sounds.py` | CC0 |
| `Sfx/enemy_hit_1.wav`, `Sfx/enemy_hit_2.wav` | попадание по боссу | own work, `Tools/make_sounds.py` | CC0 |
| `Sfx/coin_1.wav`, `Sfx/coin_2.wav` | монета | own work, `Tools/make_sounds.py` | CC0 |
| `Sfx/heal.wav` | аптечка | own work, `Tools/make_sounds.py` | CC0 |
| `Sfx/windup_guard.wav` | замах скелета (0.55 с) | own work, `Tools/make_sounds.py` | CC0 |
| `Sfx/windup_charger.wav` | замах слизня (0.7 с) | own work, `Tools/make_sounds.py` | CC0 |
| `Sfx/windup_shooter.wav` | прицеливание гриба (0.65 с) | own work, `Tools/make_sounds.py` | CC0 |
| `Sfx/windup_boss.wav` | замах босса (0.6 с) | own work, `Tools/make_sounds.py` | CC0 |
| `Sfx/death.wav` | смерть игрока | own work, `Tools/make_sounds.py` | CC0 |
| `Sfx/boss_death.wav` | победа над боссом | own work, `Tools/make_sounds.py` | CC0 |
| `Sfx/ui_click.wav` | нажатие кнопки | own work, `Tools/make_sounds.py` | CC0 |
| `Sfx/win.wav` | уровень пройден | own work, `Tools/make_sounds.py` | CC0 |
| `Sfx/lose.wav` | поражение | own work, `Tools/make_sounds.py` | CC0 |
| `Music/menu_loop.wav` | музыка меню, 76 BPM, бесшовный луп | own work, `Tools/make_sounds.py` | CC0 |
| `Music/level_loop.wav` | музыка уровней, 118 BPM, бесшовный луп | own work, `Tools/make_sounds.py` | CC0 |
