using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Game.Audio.Radio;

using UnityEngine;
using UnityEngine.Networking;

namespace CustomRadio.MonoBehaviours;

public class MusicLoader : MonoBehaviour
{
    public const string DefaultChannel = "The Second Moon";

    private readonly string AssemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
    private Dictionary<string, AudioClip> AudioClips = new Dictionary<string, AudioClip>();
    private int PreviousIndex;
    public static Radio RadioInstance { set; get; }

    private void Start()
    {
        DontDestroyOnLoad(this);

        string musicDirectory = Path.Combine(Path.GetDirectoryName(AssemblyLocation), "Music");

        if(Directory.Exists(musicDirectory))
            LoadAllAudioClips(musicDirectory);
    }

    private void LoadAllAudioClips(string path)
    {
        string[] musicFiles = Directory.GetFiles(path, "*.ogg");

        foreach(string musicFile in musicFiles)
        {
            StartCoroutine(LoadAudioClip(musicFile));
        }
    }

    private IEnumerator LoadAudioClip(string filePath)
    {
        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.OGGVORBIS);
        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Failed to load AudioClip from: " + Path.GetFileName(filePath));
            Debug.Log(request.error);
        }
        else
        {
            if(!AudioClips.ContainsKey(Path.GetFileName(filePath)))
                AudioClips.Add(Path.GetFileNameWithoutExtension(filePath), DownloadHandlerAudioClip.GetContent(request));

            SortPlaylist();
            Debug.Log("Loaded audio clip: " + Path.GetFileName(filePath));
        }
    }

    private void SortPlaylist()
    {
        AudioClips = AudioClips
            .OrderBy(p => p.Key)
            .ToDictionary(p => p.Key, p => p.Value);
    }

    public KeyValuePair<string, AudioClip>? GetRandomClip()
    {
        int randomIndex;

        if(AudioClips.Count <= 0) return null;

        do
        {
            randomIndex = Random.Range(0, AudioClips.Count);
        }
        while(PreviousIndex == randomIndex);

        PreviousIndex = randomIndex;
        return AudioClips.ElementAt(randomIndex);
    }

    public int CountFiles()
    {
        return AudioClips.Count;
    }

    public KeyValuePair<string, string> GetCurrentClipInfo()
    {
        return new KeyValuePair<string, string>(AudioClips.ElementAt(PreviousIndex).Key, "");
    }
}