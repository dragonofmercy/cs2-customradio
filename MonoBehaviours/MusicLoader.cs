using System.IO;
using System.Linq;
using System.Collections.Generic;

using ATL;
using HarmonyLib;
using UnityEngine;

using Colossal.IO.AssetDatabase;
using CustomRadio.Models;

namespace CustomRadio.MonoBehaviours;

public class MusicLoader : MonoBehaviour
{
    public const string BASE_DIRECTORY = "Radios";
    public const string BASE_NETWORK = "User Radios";

    public static readonly Dictionary<string, CustomRadioChannel> CustomRadioChannels = new Dictionary<string, CustomRadioChannel>();

    private int PreviousIndex = -1;
    private int CurrentIndex;

    public enum ClipOrder
    {
        Random,
        Sequence
    }

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
            CustomRadioChannels.TryAdd(sRadioDirectory, new CustomRadioChannel{ DirectoryName = sRadioDirectory });

            foreach(string oggFile in Directory.GetFiles(sRadioDirectory, "*.ogg"))
            {
                CustomRadioChannels[sRadioDirectory].AudioAssets.Add(oggFile, CreateAudioAsset(oggFile, sRadioName));
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

        audioAsset.AddTag("path:" + sPath);
        audioAsset.AddTag("custom:true");

        audioAssetTravers.Field("m_Metatags").SetValue(metatags);
        audioAssetTravers.Field("m_Instance").SetValue(null);

        return audioAsset;
    }

    public static AudioAsset[] GetAllClips(string radioStation)
    {
        return CustomRadioChannels[radioStation].AudioAssets.Values.ToArray();
    }

    public AudioAsset GetNextClip(string radioStation)
    {
        if(CurrentIndex > CustomRadioChannels[radioStation].AudioAssets.Count - 1)
        {
            CurrentIndex = 0;
        }

        AudioAsset result = CustomRadioChannels[radioStation].AudioAssets.ElementAt(CurrentIndex).Value;
        CurrentIndex++;
        return result;
    }

    public AudioAsset GetRandomClip(string radioStation)
    {
        int randomIndex;

        do
        {
            randomIndex = Random.Range(0, CustomRadioChannels[radioStation].AudioAssets.Count);
        }
        while(PreviousIndex == randomIndex);
        PreviousIndex = randomIndex;

        return CustomRadioChannels[radioStation].AudioAssets.ElementAt(randomIndex).Value;
    }
}