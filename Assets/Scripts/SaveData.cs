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
    public float musicVolume = 1f;
    public float effectsVolume = 1f;
}
