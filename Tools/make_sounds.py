"""Forest Adventure sound generator (own work, CC0).

Creates every sound effect and both music loops as WAV files:
    python Tools/make_sounds.py
Output: Assets/Resources/Audio/Sfx/*.wav (mono) and Assets/Resources/Audio/Music/*.wav (stereo).
Only the Python standard library is used. The random seed is fixed, so the files are the same on every run.
"""
import math
import os
import random
import struct
import wave

RATE = 44100
ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Assets", "Resources", "Audio")
TAU = 2 * math.pi


# ---------------------------------------------------------------- helpers
def midi(note):
    return 440.0 * 2 ** ((note - 69) / 12)


def env(t, length, attack=0.005, curve=1.6):
    """Fast attack, smooth decay to exactly 0 at the end (no click)."""
    if t < 0 or t >= length:
        return 0.0
    a = min(t / attack, 1.0) if attack > 0 else 1.0
    return a * (1 - t / length) ** curve


class Noise:
    def __init__(self, seed, smooth=0.0):
        self.rng = random.Random(seed)
        self.smooth = smooth
        self.last = 0.0

    def next(self):
        v = self.rng.uniform(-1, 1)
        self.last = self.last * self.smooth + v * (1 - self.smooth)
        return self.last


def sweep(length, f0, f1, wave_fn=math.sin, curve=1.6, attack=0.004, vibrato=0.0, vib_rate=0.0, shape="lin"):
    out = []
    phase = 0.0
    n = int(length * RATE)
    for i in range(n):
        t = i / RATE
        k = t / length
        f = f0 * (f1 / f0) ** k if shape == "exp" else f0 + (f1 - f0) * k
        if vibrato:
            f *= 1 + vibrato * math.sin(TAU * vib_rate * t)
        phase += TAU * f / RATE
        out.append(wave_fn(phase) * env(t, length, attack, curve))
    return out


def square(phase):
    return 0.6 if math.sin(phase) >= 0 else -0.6


def tri(phase):
    return 2 / math.pi * math.asin(math.sin(phase))


def mix(*tracks):
    n = max(len(t) for t in tracks)
    return [sum(t[i] for t in tracks if i < len(t)) for i in range(n)]


def gain(samples, g):
    return [s * g for s in samples]


def delay(samples, seconds):
    return [0.0] * int(seconds * RATE) + samples


def normalize(samples, peak):
    top = max(abs(s) for s in samples) or 1
    return [s * peak / top for s in samples]


def noise_burst(length, seed, smooth=0.0, curve=2.0, attack=0.002):
    nz = Noise(seed, smooth)
    return [nz.next() * env(i / RATE, length, attack, curve) for i in range(int(length * RATE))]


def tone_notes(notes, step, length, wave_fn=math.sin, curve=1.4, harm=0.25):
    """Short melody: each (midi, start_step) note is a plucked tone."""
    parts = []
    for note, start in notes:
        f = midi(note)
        part = [(wave_fn(TAU * f * i / RATE) + harm * math.sin(2 * TAU * f * i / RATE)) * env(i / RATE, length, 0.004, curve)
                for i in range(int(length * RATE))]
        parts.append(delay(part, start * step))
    return mix(*parts)


def write(name, samples, folder="Sfx", peak=0.71):
    path = os.path.join(ROOT, folder)
    os.makedirs(path, exist_ok=True)
    samples = normalize(samples, peak)
    with wave.open(os.path.join(path, name + ".wav"), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, s)) * 32767)) for s in samples))


def write_stereo(name, left, right, peak=0.6):
    path = os.path.join(ROOT, "Music")
    os.makedirs(path, exist_ok=True)
    top = max(max(abs(s) for s in left), max(abs(s) for s in right)) or 1
    k = peak / top
    with wave.open(os.path.join(path, name + ".wav"), "wb") as w:
        w.setnchannels(2)
        w.setsampwidth(2)
        w.setframerate(RATE)
        frames = bytearray()
        for l, r in zip(left, right):
            frames += struct.pack("<hh", int(l * k * 32767), int(r * k * 32767))
        w.writeframes(bytes(frames))


# ---------------------------------------------------------------- sound effects
def make_sfx():
    # Jump: light rising blip, two versions.
    write("jump_1", mix(sweep(0.17, 320, 760, square, 1.8, shape="exp"), gain(sweep(0.17, 640, 1520, math.sin, 2.2, shape="exp"), 0.25)))
    write("jump_2", mix(sweep(0.16, 290, 820, square, 1.8, shape="exp"), gain(sweep(0.16, 580, 1640, math.sin, 2.2, shape="exp"), 0.25)))
    # Land: soft low thud.
    write("land", mix(sweep(0.13, 140, 55, math.sin, 2.2), gain(noise_burst(0.07, 11, 0.85), 0.5)), peak=0.5)
    # Shoot: magic spark zap.
    write("shoot_1", mix(sweep(0.14, 1400, 380, square, 2.0, shape="exp"), gain(noise_burst(0.05, 21, 0.3), 0.25)))
    write("shoot_2", mix(sweep(0.13, 1250, 340, square, 2.0, shape="exp"), gain(noise_burst(0.05, 22, 0.3), 0.25)))
    # Player hurt: noisy hit + short low tone, three versions.
    for i, (f0, f1) in enumerate([(240, 90), (220, 80), (260, 100)], 1):
        write("hurt_%d" % i, mix(gain(noise_burst(0.12, 30 + i, 0.55), 0.8), sweep(0.28, f0, f1, tri, 1.5)))
    # Enemy hit (boss takes a shot).
    for i, (f0, f1) in enumerate([(420, 150), (380, 130)], 1):
        write("enemy_hit_%d" % i, mix(gain(noise_burst(0.09, 40 + i, 0.2), 0.7), sweep(0.16, f0, f1, square, 1.8)))
    # Coin: bright two-note chime.
    write("coin_1", tone_notes([(83, 0), (88, 1)], 0.07, 0.24, curve=1.8))
    write("coin_2", tone_notes([(84, 0), (89, 1)], 0.07, 0.24, curve=1.8))
    # Heal: rising three-note sparkle.
    write("heal", tone_notes([(76, 0), (81, 1), (88, 2)], 0.08, 0.3, tri, 1.6))

    # ---- enemy wind-ups: start exactly when the warning starts, last as long as the wind-up
    # Skeleton guard (0.55 s): bony rattle that rises into a swing.
    rattle = Noise(51, 0.4)
    guard = []
    n = int(0.55 * RATE)
    phase = 0.0
    for i in range(n):
        t = i / RATE
        k = t / 0.55
        click = 1.0 if (i // int(RATE / (14 + 26 * k))) % 2 == 0 else 0.2  # rattle gets faster
        phase += TAU * (160 + 260 * k * k) / RATE
        guard.append((rattle.next() * 0.5 * click + 0.6 * square(phase) * k) * min(t / 0.02, 1) * (1 - k) ** 0.3)
    write("windup_guard", guard)
    # Slime charger (0.7 s): wobbling squelch that winds up.
    write("windup_charger", mix(sweep(0.7, 140, 420, tri, 0.4, 0.03, vibrato=0.25, vib_rate=11, shape="exp"),
                                gain(noise_burst(0.7, 61, 0.9, 0.5, 0.05), 0.3)))
    # Mushroom shooter (0.65 s): charging whine with growing tremolo.
    whine = []
    phase = 0.0
    n = int(0.65 * RATE)
    for i in range(n):
        t = i / RATE
        k = t / 0.65
        phase += TAU * (380 * (1500 / 380) ** k) / RATE
        trem = 0.6 + 0.4 * math.sin(TAU * (6 + 20 * k) * t)
        whine.append(math.sin(phase) * trem * min(t / 0.03, 1) * (1 - k) ** 0.25)
    write("windup_shooter", whine)
    # Boss guardian (0.6 s): deep rumble that swells before the shot.
    rumble = Noise(71, 0.97)
    boss = []
    phase = 0.0
    n = int(0.6 * RATE)
    for i in range(n):
        t = i / RATE
        k = t / 0.6
        phase += TAU * (55 + 60 * k) / RATE
        boss.append((0.7 * math.sin(phase) + 0.25 * square(phase * 2) + 2.5 * rumble.next()) * (k ** 0.8) * (1 - k) ** 0.15)
    write("windup_boss", boss, peak=0.8)

    # Death and boss defeat.
    write("death", mix(sweep(0.9, 620, 70, square, 1.3, vibrato=0.06, vib_rate=9, shape="exp"), gain(noise_burst(0.3, 81, 0.6), 0.3)))
    write("boss_death", mix(gain(noise_burst(1.3, 91, 0.92, 1.4), 2.2), sweep(1.3, 180, 35, tri, 1.2, shape="exp")), peak=0.8)
    # UI click: short soft tick.
    write("ui_click", mix(sweep(0.05, 1800, 1100, math.sin, 3.0, 0.001), gain(noise_burst(0.012, 101, 0.1), 0.4)), peak=0.55)
    # Win and lose jingles.
    write("win", tone_notes([(72, 0), (76, 1), (79, 2), (84, 3), (88, 4.5)], 0.11, 0.55, square, 1.5, 0.15))
    write("lose", tone_notes([(67, 0), (63, 1.4), (60, 2.8), (55, 4.2)], 0.16, 0.6, tri, 1.3, 0.2))


# ---------------------------------------------------------------- music (seamless loops)
class Loop:
    """Stereo buffer where every note tail wraps around to the start: the end flows into the start."""

    def __init__(self, seconds):
        self.n = int(seconds * RATE)
        self.l = [0.0] * self.n
        self.r = [0.0] * self.n

    def note(self, start, length, f, vol, pan=0.0, wave_fn=math.sin, attack=0.01, curve=1.5, harm=0.0, detune=0.0):
        s0 = int(start * RATE)
        lv, rv = vol * (1 - pan) / 2 * 2 ** 0.5, vol * (1 + pan) / 2 * 2 ** 0.5
        for i in range(int(length * RATE)):
            t = i / RATE
            e = env(t, length, attack, curve)
            v = wave_fn(TAU * f * t) + harm * math.sin(2 * TAU * f * t)
            if detune:
                v = 0.5 * (v + wave_fn(TAU * f * (1 + detune) * t))
            idx = (s0 + i) % self.n
            self.l[idx] += v * e * lv
            self.r[idx] += v * e * rv

    def hit(self, start, length, seed, vol, smooth, pan=0.0, curve=3.0, tone=0.0, tone_f=0.0):
        nz = Noise(seed, smooth)
        s0 = int(start * RATE)
        lv, rv = vol * (1 - pan), vol * (1 + pan)
        phase = 0.0
        for i in range(int(length * RATE)):
            t = i / RATE
            e = env(t, length, 0.001, curve)
            v = nz.next()
            if tone:
                phase += TAU * tone_f * (1 - 0.6 * t / length) / RATE
                v = v * (1 - tone) + math.sin(phase) * tone
            idx = (s0 + i) % self.n
            self.l[idx] += v * e * lv
            self.r[idx] += v * e * rv


A_MINOR = {"Am": [57, 60, 64], "F": [53, 57, 60], "C": [48, 52, 55], "G": [55, 59, 62], "Em": [52, 55, 59], "Dm": [50, 53, 57]}


def make_menu_music():
    # Calm forest loop: 76 BPM, 8 bars, Am - F - C - G.
    beat = 60 / 76
    bars = ["Am", "F", "C", "G", "Am", "F", "Dm", "G"]
    song = Loop(beat * 4 * len(bars))
    melody = [76, 72, 74, 71, 72, 69, 71, 67]
    for b, name in enumerate(bars):
        t0 = b * 4 * beat
        chord = A_MINOR[name]
        for j, n in enumerate(chord):  # soft pad
            song.note(t0, 4.6 * beat, midi(n), 0.09, pan=(j - 1) * 0.5, attack=0.6, curve=1.2, detune=0.004)
        song.note(t0, 3.8 * beat, midi(chord[0] - 12), 0.22, attack=0.05, curve=1.3)  # bass
        for k in range(8):  # gentle arpeggio
            n = chord[[0, 1, 2, 1, 0, 2, 1, 2][k]] + 12
            song.note(t0 + k * beat / 2, 1.2 * beat, midi(n), 0.07, pan=0.35 if k % 2 else -0.35, wave_fn=tri, curve=2.4)
        song.note(t0 + 2 * beat, 2.2 * beat, midi(melody[b]), 0.12, pan=0.1, attack=0.03, curve=1.8, harm=0.2)  # flute-ish
    write_stereo("menu_loop", song.l, song.r, 0.55)


def make_level_music():
    # Adventure loop: 118 BPM, 8 bars, Am - F - G - Em.
    beat = 60 / 118
    bars = ["Am", "F", "G", "Em", "Am", "F", "G", "G"]
    song = Loop(beat * 4 * len(bars))
    lead = [[76, 0, 79, 0, 81, 79, 76, 74], [72, 0, 74, 0, 76, 74, 72, 69], [71, 0, 74, 0, 79, 78, 74, 71], [71, 0, 67, 0, 71, 74, 76, 0],
            [76, 0, 79, 0, 81, 83, 84, 83], [81, 0, 79, 0, 77, 76, 74, 72], [74, 0, 71, 0, 74, 79, 78, 79], [83, 0, 81, 0, 79, 0, 78, 0]]
    for b, name in enumerate(bars):
        t0 = b * 4 * beat
        chord = A_MINOR[name]
        for k in range(8):  # driving bass eighths
            n = chord[0] - 12 + (12 if k % 4 == 3 else 0)
            song.note(t0 + k * beat / 2, 0.45 * beat, midi(n), 0.2, wave_fn=square, curve=1.6)
        for k, n in enumerate(lead[b]):  # lead melody
            if n:
                song.note(t0 + k * beat / 2, 0.9 * beat, midi(n), 0.11, pan=0.15, wave_fn=square, curve=2.0, harm=0.0)
        for j, n in enumerate(chord):  # off-beat chord stabs
            for k in (1, 3):
                song.note(t0 + k * beat, 0.35 * beat, midi(n + 12), 0.045, pan=(j - 1) * 0.6, wave_fn=tri, curve=2.0)
        for k in range(4):  # drums
            song.hit(t0 + k * beat, 0.18, 200 + b * 8 + k, 0.55, 0.95, curve=4.0, tone=0.85, tone_f=95)  # kick
            if k % 2 == 1:
                song.hit(t0 + k * beat, 0.16, 300 + b * 8 + k, 0.28, 0.35, curve=3.0)  # snare
            for h in range(2):
                song.hit(t0 + k * beat + h * beat / 2, 0.04, 400 + b * 16 + k * 2 + h, 0.12, 0.0, pan=0.4, curve=3.0)  # hat
    write_stereo("level_loop", song.l, song.r, 0.6)


if __name__ == "__main__":
    make_sfx()
    make_menu_music()
    make_level_music()
    print("Sounds written to", os.path.normpath(ROOT))
