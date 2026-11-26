using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int version = 1;
    public int bestScore;
    public int totalCoins;
    public int lastLevel = 1;
    public List<int> unlockedLevels = new List<int> { 1 };
    public List<int> completedLevels = new List<int>();
    public List<int> levelScores = new List<int>();
    // Slider values 0..1; MusicManager turns them into decibels for the mixer.
    public float masterVolume = 1f;
    public float musicVolume = 0.8f;
    public float effectsVolume = 1f;
}
