# Assignment 12 — Sound Pass

## Как устроен звук (script map)

```
Gameplay scripts ──Play(Sfx.X)──▶ SoundManager ──▶ 8 AudioSource (Output = SFX) ─┐
AdventureUI buttons ─UiClick───▶ SoundManager ──▶ 1 AudioSource (Output = UI)  ─┤
SceneManager.sceneLoaded ──────▶ MusicManager ──▶ 2 AudioSource (Output = Tracks)┤──▶ GameMix ──▶ Master
AdventureUI pause/resume ─SetPaused─▶ MusicManager ──▶ snapshot Normal / Paused   │
Settings sliders ─▶ SaveManager.SetVolume ─▶ MusicManager.ApplyVolumes ─▶ MasterVol, MusicVol, SFXVol
                     └─▶ save.json + PlayerPrefs (через 0.5 с, по Back, при сворачивании)
```

- **SoundManager** создаётся до первой сцены (`RuntimeInitializeOnLoadMethod`) и живёт всю игру (`DontDestroyOnLoad`). Клипы загружаются из `Resources/Audio/Sfx`, файлы `jump_1`, `jump_2` становятся версиями события `Sfx.Jump`.
- **Вариация.** Случайная высота 0.92–1.08, случайная версия клипа, одна и та же версия два раза подряд не играет. Джинглы Win, Lose и BossDeath играют без изменения высоты.
- **Замахи.** `ForestGuard`, `ForestCharger`, `ForestShooter` запускают звук в `SetState` в тот же кадр, что и красный цвет. `BossAttack` играет `windup_boss`, ждёт 0.6 с и только потом стреляет. Длина каждого звука равна времени замаха врага.
- **MusicManager.** Две дорожки, `menu_loop` в MainMenu и `level_loop` во всех уровнях. Кроссфейд 1 с на unscaled time. Если тот же трек уже играет, `Play` сразу выходит, поэтому Retry не перезапускает музыку. Вторая копия уничтожает себя в `Awake`.
- **Микшер.** Громкость из настроек управляет группой Music, а снапшот паузы — дочерней группой Tracks. После `SetFloat` снапшот уже не может менять Music, поэтому эффекты разделены по разным группам.
- **Импорт** (`Assets/Editor/AudioImportRules.cs`). Эффекты: Force To Mono, Decompress On Load, ADPCM. Музыка: стерео, Streaming, Vorbis.

## Список звуков

| Событие | Звук | Группа |
|---|---|---|
| Прыжок / приземление | короткий восходящий сигнал / мягкий удар | SFX |
| Выстрел | искра, 2 версии | SFX |
| Урон игроку | шум и низкий тон, 3 версии | SFX |
| Попадание по боссу / смерть босса | хруст, 2 версии / долгий взрыв | SFX |
| Монета / аптечка | две ноты / три восходящие ноты | SFX |
| Замах скелета, слизня, гриба, босса | треск, бульканье, нарастающий писк, низкий гул | SFX |
| Смерть, победа, поражение | нисходящий звук, мажорный и минорный джинглы | SFX |
| Кнопки | короткий щелчок | UI |
| Меню / уровни | спокойный луп 76 BPM / быстрый луп 118 BPM | Music → Tracks |

## Проверка перед сдачей

1. **Editor:** меню **Forest Adventure → Validate Audio**. В Console должно появиться `AUDIO_VALIDATION: passed`.
2. **Тесты:** Window → General → Test Runner → PlayMode → Run All. Должно пройти 22 теста.
3. **Миксер:** Window → Audio → Audio Mixer, выбрать GameMix. Сделать скриншот: видны группы Master, Music, Tracks, SFX, UI и снапшоты Normal и Paused (в списке Snapshots слева).
4. **На телефоне:**
   - Подойти к скелету и услышать треск до удара.
   - Нажать паузу: музыка становится тише и глуше.
   - Нажать Retry: музыка не начинается заново.
   - Выйти в меню: музыка плавно меняется.
5. **Тест настроек:** Settings → Music примерно на −20 dB. Полностью закрыть приложение (убрать из недавних), открыть снова, зайти в Settings. Значение должно сохраниться.
6. **Сдача:** собрать APK (File → Build Profiles → Android → Build).

## Видео на английском, 2–3 минуты

Включите звук игры и говорите своим голосом. Показывайте только то, что действительно происходит на экране.

**0:00 — intro.** “This is Forest Adventure. For Assignment Twelve I added a full sound layer: sounds for every action and every enemy attack, two music tracks with a crossfade, and an audio mixer with saved volume settings.”

**0:15 — eight events.** Jump, shoot, collect a coin, take damage, use the pause button and finish or fail a level. “Every action answers with a sound. Repeated sounds like hits use a random pitch between 0.92 and 1.08 and two or three versions, so they do not sound like a machine gun.”

**0:45 — ears test.** Walk to an enemy. “Listen: the skeleton rattles before it swings. The wind-up sound starts on the same frame as the warning colour, so I can hear the attack before it lands. The slime, the mushroom and the boss each have their own warning sound.”

**1:10 — crossfade.** Go from a level back to the trail map, then press Retry. “Music changes with a one-second crossfade. On Retry the level music keeps playing: the MusicManager survives scene loads and does nothing if the same track is already playing.”

**1:30 — pause.** “In the pause menu the Paused snapshot lowers the music by six decibels and closes a lowpass filter, so it sounds muffled.”

**1:45 — settings test.** Change the music volume, close the app completely, open it again and show the same value. “Sliders are converted to decibels with Log10 times twenty, so half the slider is minus six decibels, not near silence. The values are saved in my JSON save file.”

**2:10 — script map.** Show `Docs/Assignment12.md` or the mixer window. “Gameplay scripts call SoundManager. It plays every effect through a fixed pool of sources routed to the SFX group. MusicManager owns the two music sources on the Tracks group and the pause snapshot. The settings sliders go through SaveManager to the exposed mixer parameters. All sounds are my own work, generated by a script, and listed with their license in the README.”
