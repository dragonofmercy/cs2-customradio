using System.IO;
using System.Linq;
using System.Collections.Generic;

using ATL;
using HarmonyLib;
using UnityEngine;

using Colossal.IO.AssetDatabase;

namespace CustomRadio.MonoBehaviours;

public class MusicLoader : MonoBehaviour
{
    public const string BASE_DIRECTORY = "Radios";
    public const string BASE_NETWORK = "User Radios";

    private readonly Dictionary<string, Dictionary<string, AudioAsset>> AudioAssets = new Dictionary<string, Dictionary<string, AudioAsset>>();
    private int PreviousIndex = -1;
    private int CurrentIndex;

    public MusicLoader()
    {
        string sNetworkDirectory = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), BASE_DIRECTORY);
        if(Directory.Exists(sNetworkDirectory))
            Preload(sNetworkDirectory);
    }

    private void Preload(string sPath)
    {
        foreach(string sRadioDirectory in Directory.GetDirectories(sPath))
        {
            string sRadioName = new DirectoryInfo(sRadioDirectory).Name;
            if(!AudioAssets.ContainsKey(sRadioName))
                AudioAssets.Add(sRadioName, new Dictionary<string, AudioAsset>());

            foreach(string oggFile in Directory.GetFiles(sRadioDirectory, "*.ogg"))
            {
                AudioAssets[sRadioName].Add(oggFile, CreateAudioAsset(oggFile, sRadioName));
            }
        }
    }

    private AudioAsset CreateAudioAsset(string sPath, string sRadioName)
    {
        AudioAsset audioAsset = new AudioAsset();
        Traverse audioAssetTravers = Traverse.Create(audioAsset);
        Dictionary<AudioAsset.Metatag, string> metatags = new Dictionary<AudioAsset.Metatag, string>();

        Track track = new(sPath, true);
        metatags[AudioAsset.Metatag.Title] = track.Title;
        metatags[AudioAsset.Metatag.Album] = track.Album;
        metatags[AudioAsset.Metatag.Artist] = track.Artist;
        metatags[AudioAsset.Metatag.Type] = "Music";
        metatags[AudioAsset.Metatag.Brand] = "Brand";
        metatags[AudioAsset.Metatag.RadioStation] = BASE_NETWORK;
        metatags[AudioAsset.Metatag.RadioChannel] = sRadioName;
        metatags[AudioAsset.Metatag.PSAType] = "";
        metatags[AudioAsset.Metatag.AlertType] = "";
        metatags[AudioAsset.Metatag.NewsType] = "";
        metatags[AudioAsset.Metatag.WeatherType] = "";

        audioAsset.AddTag(sPath);

        audioAssetTravers.Field("m_Metatags").SetValue(metatags);
        audioAssetTravers.Field("m_Instance").SetValue(null);

        return audioAsset;
    }

    public AudioAsset[] GetAllClips(string radioStation)
    {
        return AudioAssets[radioStation].Values.ToArray();
    }

    public AudioAsset GetNextClip(string radioStation)
    {
        if(CurrentIndex > AudioAssets[radioStation].Count - 1)
        {
            CurrentIndex = 0;
        }

        AudioAsset result = AudioAssets[radioStation].ElementAt(CurrentIndex).Value;
        CurrentIndex++;
        return result;
    }

    public AudioAsset GetRandomClip(string radioStation)
    {
        int randomIndex;

        do
        {
            randomIndex = Random.Range(0, AudioAssets[radioStation].Count);
        }
        while(PreviousIndex == randomIndex);
        PreviousIndex = randomIndex;

        return AudioAssets[radioStation].ElementAt(randomIndex).Value;
    }
}