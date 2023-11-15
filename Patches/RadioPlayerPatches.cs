using CustomRadio.MonoBehaviours;
using Game.Audio;
using Game.Audio.Radio;
using HarmonyLib;
using System.Diagnostics;
using UnityEngine;

namespace CustomRadio.Patches;

[HarmonyPatch(typeof(AudioManager), "OnGameLoaded")]
internal class AudioManagerOnGameLoadedPatch
{
    static void Postfix()
    {
        GameObject musicLoader = new GameObject("MusicLoader");
        musicLoader.AddComponent<MusicLoader>();
    }
}

[HarmonyPatch(typeof(Radio.RadioPlayer), "Play")]
internal class RadioPlayerPlayPatch
{
    private static MusicLoader _MusicLoader;

    static bool Prefix(Radio __instance, AudioClip clip, int timeSamples = 0)
    {
        AudioSource mAudioSource = Traverse.Create(__instance).Field("m_AudioSource").GetValue<AudioSource>();

        if(mAudioSource == null)
        {
            return false;
        }

        if(_MusicLoader == null)
        {
            _MusicLoader = GameObject.Find("MusicLoader").GetComponent<MusicLoader>();
        }

        if(_MusicLoader == null)
        {
            mAudioSource.clip = clip;
        }
        else
        {
            mAudioSource.clip = _MusicLoader.GetRandomClip() ?? clip;
        }

        mAudioSource.timeSamples = timeSamples;
        mAudioSource.Play();

        Traverse.Create(__instance).Field("m_Elapsed").SetValue(GetAudioSourceTimeElapsed(__instance, mAudioSource));

        Stopwatch mTimer = Traverse.Create(__instance).Field("m_Timer").GetValue<Stopwatch>();
        mTimer.Restart();

        return false;
    }

    public static double GetAudioSourceTimeElapsed(Radio __instance, AudioSource audioSource)
    {
        bool isCreated = Traverse.Create(__instance).Field("isCreated").GetValue<bool>();
        return isCreated && audioSource.clip != null ? audioSource.timeSamples / (double) audioSource.clip.frequency : 0.0;
    }
}