using CustomRadio.MonoBehaviours;
using Game.Audio;
using HarmonyLib;
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