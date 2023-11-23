using System.Collections.Generic;

using Colossal.IO.AssetDatabase;
using CustomRadio.MonoBehaviours;

using Game.Audio;
using Game.Audio.Radio;

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

[HarmonyPatch(typeof(Radio), "NextSong")]
internal class RadioNextSongPatch
{
    static bool Prefix(Radio __instance)
    {
        if(MusicLoader.RadioInstance.currentChannel.name != MusicLoader.DefaultChannel) return true;

        Radio.RadioPlayer mRadioPlayer = Traverse.Create(__instance).Field("m_RadioPlayer").GetValue<Radio.RadioPlayer>();
        MusicLoader.HistoryIndex = MusicLoader.CurrentIndex;
        mRadioPlayer.Play(null);

        return true;
    }
}

[HarmonyPatch(typeof(Radio), "PreviousSong")]
internal class RadioPreviousSongPatch
{
    static bool Prefix(Radio __instance)
    {
        if(MusicLoader.RadioInstance.currentChannel.name != MusicLoader.DefaultChannel) return true;

        Radio.RadioPlayer mRadioPlayer = Traverse.Create(__instance).Field("m_RadioPlayer").GetValue<Radio.RadioPlayer>();
        MusicLoader.CurrentIndex -= 2;
        MusicLoader.HistoryIndex = MusicLoader.CurrentIndex;
        mRadioPlayer.Play(null);

        return true;
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
        bool custom = false;
        AudioSource mAudioSource = Traverse.Create(__instance).Field("m_AudioSource").GetValue<AudioSource>();

        if(clip == null)
            return true;

        if(mAudioSource == null)
            return false;

        if(_MusicLoader == null)
            _MusicLoader = GameObject.Find("MusicLoader").GetComponent<MusicLoader>();

        if(_MusicLoader == null || MusicLoader.RadioInstance.currentChannel.name != MusicLoader.DefaultChannel)
        {
            mAudioSource.clip = clip;
        }
        else
        {
            if(MusicLoader.CurrentIndex == MusicLoader.HistoryIndex)
            {
                MusicLoader.CurrentIndex++;
            }

            AudioClip acSong = _MusicLoader.GetCurrentClipAudio();

            if(acSong == null)
            {
                acSong = clip;
            }
            else
            {
                custom = true;

                MusicLoader.RadioInstance.currentChannel.currentProgram.name = "Custom Music Playlist";
                MusicLoader.RadioInstance.currentChannel.currentProgram.description = _MusicLoader.GetCurrentClipName();

                Dictionary<AudioAsset.Metatag, string> mMetatags = Traverse.Create(MusicLoader.RadioInstance.currentChannel.currentProgram.currentSegment.currentClip).Field("m_Metatags").GetValue<Dictionary<AudioAsset.Metatag, string>>();
                mMetatags[AudioAsset.Metatag.Title] = _MusicLoader.GetCurrentClipName();
                mMetatags[AudioAsset.Metatag.Artist] = "";
            }

            mAudioSource.clip = acSong;
        }

        mAudioSource.timeSamples = timeSamples;
        mAudioSource.Play();

        if(custom)
        {
            MusicLoader.HistoryIndex = MusicLoader.CurrentIndex;
        }

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