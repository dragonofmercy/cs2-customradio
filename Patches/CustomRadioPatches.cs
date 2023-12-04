using CustomRadio.Models;
using CustomRadio.MonoBehaviours;

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using Game.Audio.Radio;
using Game.SceneFlow;
using Game.UI;

using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

using Colossal.Json;
using Colossal.IO.AssetDatabase;

namespace CustomRadio.Patches;

/// <summary>
/// GameManager patches
/// </summary>
[HarmonyPatch(typeof(GameManager), "InitializeThumbnails")]
internal class GameManagerInitializeThumbnailsPatch
{
    private const string ICONS_RESOURCE_KEY = "dragon.cs2.customradio.ui";
    public const string COUI_BASE_LOCATION = $"coui://{ICONS_RESOURCE_KEY}";

    static void Prefix()
    {
        GameUIResourceHandler gameUiResourceHandler = (GameUIResourceHandler) GameManager.instance.userInterface.view.uiSystem.resourceHandler;
        gameUiResourceHandler?.HostLocationsMap.Add(ICONS_RESOURCE_KEY, new List<string> {
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            }
        );
    }
}

/// <summary>
/// AudioAsset patches
/// </summary>
[HarmonyPatch(typeof(AudioAsset), "LoadAsync")]
internal class AudioAssetLoadAsyncPatch
{
    static bool Prefix(AudioAsset __instance, ref Task<AudioClip> __result)
    {
        List<string> mTags = Traverse.Create(__instance).Field("m_Tags").GetValue<List<string>>();
        if(mTags?.Find(s => s.Contains("custom:true")) == null) return true;
        __result = LoadAudioFile(__instance);
        return false;
    }

    private static async Task<AudioClip> LoadAudioFile(AudioAsset audioAsset)
    {
        Traverse audioAssetTravers = Traverse.Create(audioAsset);
        AudioClip mInstance = audioAssetTravers.Field("m_Instance").GetValue<AudioClip>();
        List<string> mTags = audioAssetTravers.Field("m_Tags").GetValue<List<string>>();

        if(mInstance != null || mTags == null) return mInstance;

        string sPath = mTags.Find(s => s.StartsWith("path:")).Replace("path:", "");
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + sPath, AudioType.OGGVORBIS);
        ((DownloadHandlerAudioClip) www.downloadHandler).streamAudio = true;
        await www.SendWebRequest();
        mInstance = DownloadHandlerAudioClip.GetContent(www);
        www.Dispose();

        mInstance.name = sPath;
        mInstance.hideFlags = HideFlags.DontSave;

        audioAssetTravers.Field("m_Instance").SetValue(mInstance);
        Debug.Log("File loaded: " + Path.GetFileName(sPath));

        return mInstance;
    }
}

/// <summary>
/// Radio patches
/// </summary>
[HarmonyPatch(typeof(Radio), "LoadRadio")]
internal class RadioLoadRadioPatch
{
    public static readonly GameObject GameObjectMusicLoader = new("MusicLoader");
    public static readonly MusicLoader MusicLoaderInstance = GameObjectMusicLoader.AddComponent<MusicLoader>();
    public static readonly string RadioNetworkDirectory = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), MusicLoader.BASE_DIRECTORY);

    static void Postfix(Radio __instance)
    {
        if(MusicLoaderInstance == null || !Directory.Exists(RadioNetworkDirectory)) return;

        Traverse radioTravers = Traverse.Create(__instance);

        Dictionary<string, Radio.RadioNetwork> mNetworks = Traverse.Create(__instance).Field("m_Networks").GetValue<Dictionary<string, Radio.RadioNetwork>>();
        Dictionary<string, Radio.RuntimeRadioChannel> mRadioChannels = Traverse.Create(__instance).Field("m_RadioChannels").GetValue<Dictionary<string, Radio.RuntimeRadioChannel>>();

        string sRadioNetworkName = "User Radios";
        string sRadioNetworkDescription = "Custom User Radios";

        if(mNetworks.ContainsKey(sRadioNetworkName) &&
           Directory.GetDirectories(RadioNetworkDirectory).Length == 0 &&
           File.Exists(Path.Combine(RadioNetworkDirectory, "icon.svg"))
        ) return;

        if(File.Exists(Path.Combine(RadioNetworkDirectory, "meta.json")))
        {
            Variant config = JSON.Load(File.ReadAllText(Path.Combine(RadioNetworkDirectory, "meta.json")));

            sRadioNetworkName = config.TryGet("name") ?? sRadioNetworkName;
            sRadioNetworkDescription = config.TryGet("description") ?? sRadioNetworkDescription;
        }

        Radio.RadioNetwork network = new(){
            name = sRadioNetworkName,
            nameId = sRadioNetworkName,
            description = sRadioNetworkDescription,
            descriptionId = sRadioNetworkDescription,
            icon = $"{GameManagerInitializeThumbnailsPatch.COUI_BASE_LOCATION}/Radios/icon.svg",
            uiPriority = mNetworks.Count,
            allowAds = false
        };

        mNetworks.Add(network.name, network);

        foreach(KeyValuePair<string, CustomRadioChannel> kvCustomRadioChannel in MusicLoader.CustomRadioChannels)
        {
            CreateRadioStation(kvCustomRadioChannel.Value, network.name);
            string sChannelNameId = kvCustomRadioChannel.Value.Channel.name;
            while(mRadioChannels.ContainsKey(sChannelNameId))
            {
                sChannelNameId = sChannelNameId + "_" + radioTravers.Method("MakeUniqueRandomName", sChannelNameId, 4).GetValue<string>();
            }

            mRadioChannels.Add(sChannelNameId, kvCustomRadioChannel.Value.Channel.CreateRuntime(kvCustomRadioChannel.Key));
        }

        radioTravers.Field("m_Networks").SetValue(mNetworks);
        radioTravers.Field("m_RadioChannels").SetValue(mRadioChannels);
        radioTravers.Field("m_CachedRadioChannelDescriptors").SetValue(null);
    }

    private static void CreateRadioStation(CustomRadioChannel customRadioChannel, string radioNetwork)
    {
        string sPath = customRadioChannel.DirectoryName;
        string sRadioName = new DirectoryInfo(sPath).Name;

        List<string> thumbnailPath = new List<string>{
            GameManagerInitializeThumbnailsPatch.COUI_BASE_LOCATION,
            MusicLoader.BASE_DIRECTORY
        };

        if(File.Exists(Path.Combine(sPath, "icon.svg")))
        {
            thumbnailPath.Add(sRadioName);
        }

        thumbnailPath.Add("icon.svg");

        string sRadioIcon = string.Join("/", thumbnailPath.ToArray());
        string sProgramName = "Music non stop";
        string sProgramDescription = "Dance all day, dance all night";
        int uiPriority = 1;

        Radio.Segment segment = new(){
            type = Radio.SegmentType.Playlist,
            clipsCap = 1,
            clips = MusicLoader.GetAllClips(sPath),
            tags = new []{
                "type:Music",
                "radio channel:" + sRadioName,
                "custom:true",
                "path:" + sPath
            }
        };

        MusicLoader.ClipOrder clipOrder = MusicLoader.ClipOrder.Sequence;

        if(File.Exists(Path.Combine(sPath, "meta.json")))
        {
            Variant config = JSON.Load(File.ReadAllText(Path.Combine(sPath, "meta.json")));

            sRadioName = config.TryGet("radio_name") ?? sRadioName;
            sProgramName = config.TryGet("program_name") ?? sProgramName;
            sProgramDescription = config.TryGet("program_description") ?? sProgramDescription;
            uiPriority = config.TryGet("order") ?? uiPriority;

            if(config.TryGet("clip_order") != null)
            {
                clipOrder = config.TryGet("clip_order") == "sequence" ? MusicLoader.ClipOrder.Sequence : MusicLoader.ClipOrder.Random;
            }
            else
            {
                clipOrder = MusicLoader.ClipOrder.Sequence;
            }
        }

        Radio.Program program = new(){
            name = sProgramName,
            description = sProgramDescription,
            icon = sRadioIcon,
            startTime = "00:00",
            endTime = "00:00",
            loopProgram = true,
            segments = new[]{ segment }
        };

        Radio.RadioChannel radioChannel = new(){
            network = radioNetwork,
            name = sRadioName,
            description = "",
            icon = sRadioIcon,
            uiPriority = uiPriority,
            programs = new[]{ program }
        };

        customRadioChannel.ClipOrder = clipOrder;
        customRadioChannel.Channel = radioChannel;
    }
}

[HarmonyPatch(typeof(Radio), "GetPlaylistClips")]
internal class RadioGetPlaylistClipsPatch
{
    static bool Prefix(Radio.RuntimeSegment segment)
    {
        if(Array.Find(segment.tags, s => s.Contains("custom:true")) == null) return true;
        string sPath = Array.Find(segment.tags, s => s.StartsWith("path:")).Replace("path:", "");
        AudioAsset audioAsset = MusicLoader.CustomRadioChannels[sPath].ClipOrder == MusicLoader.ClipOrder.Sequence ? RadioLoadRadioPatch.MusicLoaderInstance.GetNextClip(sPath) : RadioLoadRadioPatch.MusicLoaderInstance.GetRandomClip(sPath);
        segment.clips = new []{ audioAsset };
        return false;
    }
}