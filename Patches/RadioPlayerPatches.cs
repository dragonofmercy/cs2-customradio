using CustomRadio.MonoBehaviours;

using Game.Audio.Radio;
using HarmonyLib;
using UnityEngine;

namespace CustomRadio.Patches;

[HarmonyPatch(typeof(Radio.RadioPlayer), "Play")]
internal class RadioPlayerPlayPatch
{
    private static MusicLoader _MusicLoader;
    private static int? _CurrentlyPlaying;

    static bool Prefix(Radio __instance, AudioClip clip, int timeSamples = 0)
    {
        AudioSource mAudioSource = Traverse.Create(__instance).Field("m_AudioSource").GetValue<AudioSource>();

        if(mAudioSource == null)
            return false;

        if(_MusicLoader == null)
            _MusicLoader = GameObject.Find("MusicLoader").GetComponent<MusicLoader>();

        if(_CurrentlyPlaying != null && _CurrentlyPlaying == MusicLoader.CurrentIndex)
            MusicLoader.CurrentIndex++;

        if(_MusicLoader == null || MusicLoader.CurrentChannel != MusicLoader.DefaultChannel)
            mAudioSource.clip = clip;
        else
            mAudioSource.clip = _MusicLoader.GetCurrentClip() ?? clip;

        mAudioSource.timeSamples = timeSamples;
        mAudioSource.Play();

        _CurrentlyPlaying = MusicLoader.CurrentIndex;

        Traverse.Create(__instance).Field("m_Elapsed").SetValue(GetAudioSourceTimeElapsed(__instance, mAudioSource));

        System.Diagnostics.Stopwatch mTimer = Traverse.Create(__instance).Field("m_Timer").GetValue<System.Diagnostics.Stopwatch>();
        mTimer.Restart();

        return false;
    }

    public static double GetAudioSourceTimeElapsed(Radio __instance, AudioSource audioSource)
    {
        bool isCreated = Traverse.Create(__instance).Field("isCreated").GetValue<bool>();
        return isCreated && audioSource.clip != null ? audioSource.timeSamples / (double) audioSource.clip.frequency : 0.0;
    }
}