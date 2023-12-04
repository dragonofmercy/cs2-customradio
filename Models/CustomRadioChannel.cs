using System.Collections.Generic;
using Colossal.IO.AssetDatabase;
using CustomRadio.MonoBehaviours;
using Game.Audio.Radio;

namespace CustomRadio.Models;

public class CustomRadioChannel
{
    public readonly Dictionary<string, AudioAsset> AudioAssets = new Dictionary<string, AudioAsset>();
    public string DirectoryName { get; set; }
    public MusicLoader.ClipOrder ClipOrder { get; set; }
    public Radio.RadioChannel Channel { get; set; }
}