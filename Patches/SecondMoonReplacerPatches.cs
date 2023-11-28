using System;
using CustomRadio.MonoBehaviours;
using CustomRadio.Models;

using Game.Audio;
using Game.Audio.Radio;
using Game.UI.InGame;

using Colossal.IO.AssetDatabase;

using HarmonyLib;
using UnityEngine;

namespace CustomRadio.Patches;

/// <summary>
/// AudioManager patches
/// </summary>
[HarmonyPatch(typeof(AudioManager), "OnGameLoaded")]
internal class AudioManagerOnGameLoadedPatch
{
    static void Postfix()
    {
        GameObject musicLoader = new GameObject("MusicLoader");
        musicLoader.AddComponent<MusicLoader>();
    }
}

/// <summary>
/// Radio patches
/// </summary>
[HarmonyPatch(typeof(Radio), "Update")]
internal class RadioUpdatePatch
{
    static void Postfix(Radio __instance)
    {
        MusicLoader.RadioInstance = __instance;
    }
}

/// <summary>
/// RadioPlayer patches
/// </summary>
[HarmonyPatch(typeof(Radio.RadioPlayer), "Play")]
internal class RadioPlayerPlayPatch
{
    private static MusicLoader _MusicLoader;

    static bool Prefix(Radio.RadioPlayer __instance, AudioClip clip, int timeSamples = 0)
    {
        AudioSource mAudioSource = Traverse.Create(__instance).Field("m_AudioSource").GetValue<AudioSource>();

        if(clip == null)
            return true;

        if(mAudioSource == null)
            return false;

        if(_MusicLoader == null)
            _MusicLoader = GameObject.Find("MusicLoader").GetComponent<MusicLoader>();

        if(_MusicLoader == null || MusicLoader.RadioInstance.currentChannel.name != MusicLoader.DefaultChannel || _MusicLoader.CountFiles() == 0)
        {
            mAudioSource.clip = clip;
        }
        else
        {
            AudioSong song = _MusicLoader.GetRandomSong();

            if(song == null)
            {
                mAudioSource.clip = null;
            }
            else
            {
                mAudioSource.clip = song.Clip;
                MusicLoader.RadioInstance.currentChannel.currentProgram.name = "Custom Music Playlist";
            }
        }

        mAudioSource.timeSamples = timeSamples;
        mAudioSource.Play();

        Traverse.Create(__instance).Field("m_Elapsed").SetValue(GetAudioSourceTimeElapsed(__instance, mAudioSource));

        System.Diagnostics.Stopwatch mTimer = Traverse.Create(__instance).Field("m_Timer").GetValue<System.Diagnostics.Stopwatch>();
        mTimer.Restart();

        return false;
    }

    public static double GetAudioSourceTimeElapsed(Radio.RadioPlayer __instance, AudioSource audioSource)
    {
        bool isCreated = Traverse.Create(__instance).Field("isCreated").GetValue<bool>();
        return isCreated && audioSource.clip != null ? audioSource.timeSamples / (double) audioSource.clip.frequency : 0.0;
    }
}

/// <summary>
/// RadioUISystem patches
/// </summary>
///
[HarmonyPatch(typeof(RadioUISystem), "GetClipInfo")]
internal class RadioUiSystemGetClipInfoPatch
{
    private static MusicLoader _MusicLoader;

    static void Postfix(Game.Audio.Radio.Radio radio, AudioAsset asset, ref RadioUISystem.ClipInfo __result)
    {
        if(asset == null) return;
        if(radio.currentChannel.name != MusicLoader.DefaultChannel) return;

        if(_MusicLoader == null)
            _MusicLoader = GameObject.Find("MusicLoader").GetComponent<MusicLoader>();

        AudioSong song = _MusicLoader.GetCurrentSong();

        if(song == null)
            return;

        __result = new RadioUISystem.ClipInfo {
            title = song.Title,
            info = song.Artist
        };
    }
}