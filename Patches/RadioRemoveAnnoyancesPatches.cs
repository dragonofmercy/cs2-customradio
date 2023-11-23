using Game.Audio.Radio;
using HarmonyLib;

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
        return clip.m_SegmentType == Radio.SegmentType.Playlist;
    }
}