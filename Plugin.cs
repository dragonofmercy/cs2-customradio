using BepInEx.Unity.Mono;
using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace CustomRadio;

[BepInPlugin("dragon.cs2.customradio", "Custom Radio", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Info.Metadata.GUID + "_Cities2Harmony");
        Logger.LogInfo($"Plugin {Info.Metadata.GUID} is loaded!");
    }
}