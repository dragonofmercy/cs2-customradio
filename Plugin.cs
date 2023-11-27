using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace CustomRadio;

[BepInPlugin("dragon.cs2.customradio", "Custom Radio", "0.0.5")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Info.Metadata.GUID + "_Cities2Harmony");
        Logger.LogInfo($"Plugin {Info.Metadata.Name} is loaded!");
    }
}