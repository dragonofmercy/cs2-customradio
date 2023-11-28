using System;
using System.Collections.Generic;
using System.IO;
using Game.Audio.Radio;
using CustomRadio.Models;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomRadio.MonoBehaviours;

public class MusicLoader : MonoBehaviour
{
    public const string DefaultChannel = "The Second Moon";

    private readonly string AssemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
    private readonly List<AudioSong> Songs = new List<AudioSong>();
    private int PreviousIndex;
    public static Radio RadioInstance { set; get; }

    private void Start()
    {
        DontDestroyOnLoad(this);

        string musicDirectory = Path.Combine(Path.GetDirectoryName(AssemblyLocation), "Music");

        if(Directory.Exists(musicDirectory))
            LoadAllAudioClips(musicDirectory);
    }

    private void LoadAllAudioClips(string path)
    {
        string[] musicFiles = Directory.GetFiles(path, "*.ogg");

        foreach(string musicFile in musicFiles)
        {
            AudioSong song = new AudioSong(musicFile);
            Songs.Add(song);
        }
    }

    public int CountFiles()
    {
        return Songs.Count;
    }

    [CanBeNull]
    public AudioSong GetRandomSong()
    {
        int randomIndex;

        if(CountFiles() <= 0) return null;

        do
        {
            randomIndex = UnityEngine.Random.Range(0, CountFiles());
        }
        while(PreviousIndex == randomIndex);

        PreviousIndex = randomIndex;

        return Songs[randomIndex];
    }

    [CanBeNull]
    public AudioSong GetCurrentSong()
    {
        try
        {
            return Songs[PreviousIndex];
        }
        catch(IndexOutOfRangeException)
        {
            return null;
        }
    }
}