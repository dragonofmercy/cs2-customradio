using System.IO;
using System.Collections.Generic;
using System.Linq;

using ATL;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

using Colossal.IO.AssetDatabase;

namespace CustomRadio.MonoBehaviours;

public class MusicLoader : MonoBehaviour
{
    public const string BASE_DIRECTORY = "Radios";
    public const string BASE_NETWORK = "User Radios";

    private int PreviousIndex = -1;
    private readonly Dictionary<string, Dictionary<string, AudioAsset>> AudioAssets = new Dictionary<string, Dictionary<string, AudioAsset>>();

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

        audioAssetTravers.Field("m_Metatags").SetValue(metatags);
        audioAssetTravers.Field("m_Instance").SetValue(null);

        LoadAudioFile(sPath, sRadioName);

        return audioAsset;
    }

    private async void LoadAudioFile(string sPath, string sRadioName)
    {
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + sPath, AudioType.OGGVORBIS);
        await www.SendWebRequest();
        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
        www.Dispose();

        Traverse.Create(AudioAssets[sRadioName][sPath]).Field("m_Instance").SetValue(clip);
        Debug.Log("File loaded: " + Path.GetFileName(sPath));
    }

    public AudioAsset[] GetAllClips(string radioStation)
    {
        return AudioAssets[radioStation].Values.ToArray();
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

    /*
    //// The function below need some work

    public AudioAsset[] GetClips(string radioStation, int clipsCap)
    {
        System.Random rnd = new();
        AudioAsset[] audioAssetsArray = new AudioAsset[clipsCap];
        Dictionary<string, AudioAsset> audioAssetsDictionary = AudioAssets[radioStation];
        List<int> list = Enumerable.Range(0, audioAssetsDictionary.Count).OrderBy(_ => rnd.Next()).Take(clipsCap).ToList();

        for(int index = 0; index < audioAssetsArray.Length; ++index)
        {
            KeyValuePair<string, AudioAsset> element = audioAssetsDictionary.ElementAt(list[index]);

            if(Traverse.Create(element.Value).Field("m_Instance").GetValue() == null)
            {
                LoadAudioFile(element.Key, radioStation);
            }

            audioAssetsArray[index] = element.Value;
        }


        return audioAssetsArray;
    }*/
}