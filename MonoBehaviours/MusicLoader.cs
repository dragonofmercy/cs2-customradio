using System.IO;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

using Colossal.IO.AssetDatabase;
using Colossal.Json;

namespace CustomRadio.MonoBehaviours;

public class MusicLoader : MonoBehaviour
{
    public const string BASE_DIRECTORY = "Radios";
    public const string BASE_NETWORK = "User Radios";

    private readonly Dictionary<string, List<AudioAsset>> AudioAssets = new Dictionary<string, List<AudioAsset>>();

    private void Start()
    {
        DontDestroyOnLoad(this);

        string sNetworkDirectory = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), BASE_DIRECTORY);

        if(Directory.Exists(sNetworkDirectory))
            LoadAllAudioClips(sNetworkDirectory);
    }

    private void LoadAllAudioClips(string sPath)
    {
        foreach(string sRadioDirectory in Directory.GetDirectories(sPath))
        {
            string sRadioName = new DirectoryInfo(sRadioDirectory).Name;

            foreach(string oggFile in Directory.GetFiles(sRadioDirectory, "*.ogg"))
            {
                PreloadClip(oggFile, sRadioName);
            }
        }
    }

    private async void PreloadClip(string sPath, string sRadioName)
    {
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + sPath, AudioType.OGGVORBIS);
        await www.SendWebRequest();
        if (www.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            Debug.Log(www.error);
        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
        www.Dispose();

        if(clip == null) return;

        if(!AudioAssets.ContainsKey(sRadioName))
            AudioAssets.Add(sRadioName, new List<AudioAsset>());

        AudioAsset audioAsset = new();
        Dictionary<AudioAsset.Metatag, string> metatags = new Dictionary<AudioAsset.Metatag, string>();

        Traverse audioAssetTravers = Traverse.Create(audioAsset);
        audioAssetTravers.Field("m_Instance").SetValue(clip);

        TagLib.File tagFile = TagLib.File.Create(sPath);

        metatags[AudioAsset.Metatag.Title] = tagFile.Tag.Title ?? Path.GetFileNameWithoutExtension(sPath);
        metatags[AudioAsset.Metatag.Album] = tagFile.Tag.Album ?? "";
        metatags[AudioAsset.Metatag.Artist] = tagFile.Tag.FirstPerformer ?? "";
        metatags[AudioAsset.Metatag.Type] = "Music";
        metatags[AudioAsset.Metatag.Brand] = "Brand";
        metatags[AudioAsset.Metatag.RadioStation] = BASE_NETWORK;
        metatags[AudioAsset.Metatag.RadioChannel] = sRadioName;
        metatags[AudioAsset.Metatag.PSAType] = sPath;
        metatags[AudioAsset.Metatag.AlertType] = "";
        metatags[AudioAsset.Metatag.NewsType] = "";
        metatags[AudioAsset.Metatag.WeatherType] = "";

        audioAssetTravers.Field("m_Metatags").SetValue(metatags);
        AudioAssets[sRadioName].Add(audioAsset);

        Debug.Log("File loaded: " + Path.GetFileName(sPath));
    }

    public AudioAsset[] GetClips(string radioStation)
    {
        return AudioAssets[radioStation].ToArray();
    }
}