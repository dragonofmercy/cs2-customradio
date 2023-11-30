using System;
using CustomRadio.MonoBehaviours;

using System.IO;
using System.Reflection;
using System.Collections.Generic;

using Game;
using Game.Audio;
using Game.Audio.Radio;
using Game.SceneFlow;
using Game.UI;

using HarmonyLib;
using UnityEngine;

using Colossal.Json;
using Colossal.IO.AssetDatabase;

namespace CustomRadio.Patches;

/// <summary>
/// AudioManager patches
/// </summary>
[HarmonyPatch(typeof(AudioManager), "OnGamePreload")]
internal class AudioManagerOnGameLoadedPatch
{
    static void Postfix(GameMode mode)
    {
        if(mode != GameMode.Game) return;
        GameObject musicLoader = new GameObject("MusicLoader");
        musicLoader.AddComponent<MusicLoader>();
    }
}

/// <summary>
/// GameManager patches
/// </summary>
[HarmonyPatch(typeof(GameManager), "InitializeThumbnails")]
internal class GameManagerInitializeThumbnailsPatch
{
    private const string ICONS_RESOURCE_KEY = "dragon.cs2.customradioui";
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
/// Radio patches
/// </summary>
[HarmonyPatch(typeof(Radio), "LoadRadio")]
internal class RadioLoadRadioPatch
{
    private static MusicLoader _MusicLoader;
    public static readonly string RadioNetworkDirectory = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), MusicLoader.BASE_DIRECTORY);
    public static readonly List<string> RadioChannels = new List<string>();

    static void Postfix(Radio __instance)
    {
        if(!Directory.Exists(RadioNetworkDirectory)) return;

        if(_MusicLoader == null)
            _MusicLoader = GameObject.Find("MusicLoader").GetComponent<MusicLoader>();

        if(_MusicLoader == null) return;

        Traverse radioTravers = Traverse.Create(__instance);

        Dictionary<string, Radio.RadioNetwork> mNetworks = Traverse.Create(__instance).Field("m_Networks").GetValue<Dictionary<string, Radio.RadioNetwork>>();
        Dictionary<string, Radio.RuntimeRadioChannel> mRadioChannels = Traverse.Create(__instance).Field("m_RadioChannels").GetValue<Dictionary<string, Radio.RuntimeRadioChannel>>();

        string sRadioNetworkName = "User Radios";
        string sRadioNetworkDescription = "Custom User Radios";
        int intRadioNetworkIndex = mNetworks.Count - 1;

        if(mNetworks.ContainsKey(sRadioNetworkName) &&
           Directory.GetDirectories(RadioNetworkDirectory).Length == 0 &&
           File.Exists(Path.Combine(RadioNetworkDirectory, "icon.svg"))
        ) return;

        if(File.Exists(Path.Combine(RadioNetworkDirectory, "meta.json")))
        {
            Variant config = JSON.Load(File.ReadAllText(Path.Combine(RadioNetworkDirectory, "meta.json")));

            try
            {
                sRadioNetworkName = config["name"];
                sRadioNetworkDescription = config["description"];
            }
            catch(Exception)
            {
                // Do nothing
            }
        }

        Radio.RadioNetwork network = new(){
            name = sRadioNetworkName,
            nameId = sRadioNetworkName,
            description = sRadioNetworkDescription,
            descriptionId = sRadioNetworkDescription,
            icon = $"{GameManagerInitializeThumbnailsPatch.COUI_BASE_LOCATION}/Radios/icon.svg",
            uiPriority = intRadioNetworkIndex,
            allowAds = false
        };

        mNetworks.Add(network.name, network);

        foreach(string sRadioStationPath in Directory.GetDirectories(RadioNetworkDirectory))
        {
            Radio.RadioChannel radioChannel = CreateRadioStation(sRadioStationPath, network.name);
            string sChannelNameId = radioChannel.name;
            while(mRadioChannels.ContainsKey(sChannelNameId))
            {
                sChannelNameId = sChannelNameId + "_" + radioTravers.Method("MakeUniqueRandomName", sChannelNameId, 4).GetValue<string>();
            }

            mRadioChannels.Add(sChannelNameId, radioChannel.CreateRuntime(sRadioStationPath));
            RadioChannels.Add(radioChannel.name);
        }

        radioTravers.Field("m_Networks").SetValue(mNetworks);
        radioTravers.Field("m_RadioChannels").SetValue(mRadioChannels);
        radioTravers.Field("m_CachedRadioChannelDescriptors").SetValue(null);
    }

    private static Radio.RadioChannel CreateRadioStation(string path, string radioNetwork)
    {
        string sRadioName = new DirectoryInfo(path).Name;
        List<string> thumbnailPath = new List<string>{
            GameManagerInitializeThumbnailsPatch.COUI_BASE_LOCATION,
            MusicLoader.BASE_DIRECTORY
        };

        if(File.Exists(Path.Combine(path, "icon.svg")))
        {
            thumbnailPath.Add(sRadioName);
        }

        thumbnailPath.Add("icon.svg");

        string sRadioIcon = string.Join("/", thumbnailPath.ToArray());
        string sProgramName = "Music non stop";
        string sProgramDescription = "Dance all day, dance all night";

        AudioAsset[] audioAsset = _MusicLoader.GetClips(sRadioName);

        Radio.Segment segment = new(){
            type = Radio.SegmentType.Playlist,
            clipsCap = 2,
            clips = audioAsset,
            tags = new []{
                "type:Music",
                "radio channel:" + sRadioName
            }
        };

        if(File.Exists(Path.Combine(path, "meta.json")))
        {
            Variant config = JSON.Load(File.ReadAllText(Path.Combine(path, "meta.json")));

            try
            {
                sProgramName = config["program_name"];
                sProgramDescription = config["program_description"];
            }
            catch(Exception)
            {
                // Do nothing
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
            description = "Test",
            icon = sRadioIcon,
            uiPriority = 1,
            programs = new[]{ program }
        };

        return radioChannel;
    }
}

[HarmonyPatch(typeof(Radio), "GetPlaylistClips")]
internal class RadioGetPlaylistClipsPatch
{
    static bool Prefix(Radio __instance)
    {
        return !RadioLoadRadioPatch.RadioChannels.Contains(__instance.currentChannel.name);
    }
}