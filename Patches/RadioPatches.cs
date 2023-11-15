using Game.Audio.Radio;
using HarmonyLib;
using UnityEngine;

namespace CustomRadio.Patches;

[HarmonyPatch(typeof(Radio), "QueueEmergencyClips")]
internal class RadioQueueEmergencyClipsPatch
{
    static bool Prefix()
    {
        return false;
    }
}

[HarmonyPatch(typeof(Radio), "QueueEmergencyIntroClip")]
internal class RadioQueueEmergencyIntroClipPatch
{
    static bool Prefix()
    {
        return false;
    }
}

[HarmonyPatch(typeof(Radio), "QueueClip")]
internal class RadioQueueClipPatch
{
    static bool Prefix(ref Game.Audio.Radio.Radio.ClipInfo clip, bool pushToFront = false)
    {
        if(clip.m_SegmentType == Radio.SegmentType.Playlist) return true;
        Debug.Log("Skipped radio clip: " + clip.m_SegmentType);
        return false;
    }
}