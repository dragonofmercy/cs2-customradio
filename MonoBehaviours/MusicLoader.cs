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
    public static int CurrentIndex { set; get; }
    public static int HistoryIndex { set; get; }
    public static Radio RadioInstance { set; get; }

    private void Start()
    {
        DontDestroyOnLoad(this);

        string musicDirectory = Path.Combine(Path.GetDirectoryName(AssemblyLocation), "Music");

        if(Directory.Exists(musicDirectory))
        {
            Debug.Log(musicDirectory + " Found");
            LoadAllAudioClips(musicDirectory);
        }
        else
        {
            Debug.Log("Error: " + musicDirectory + " was not found");
        }
    }

    private void LoadAllAudioClips(string path)
    {
        string[] musicFiles = Directory.GetFiles(path, "*.ogg");

        foreach(string musicFile in musicFiles)
        {
            StartCoroutine(LoadAudioClip(musicFile));
        }

        CurrentIndex = 0;
        HistoryIndex = -1;
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

    private KeyValuePair<string, AudioClip> GetCurrentClip()
    {
        if(CurrentIndex < 0)
        {
            CurrentIndex = AudioClips.Count - 1;
        }

        if(CurrentIndex > AudioClips.Count - 1)
        {
            CurrentIndex = 0;
        }

        return AudioClips.ElementAt(CurrentIndex);
    }

    public AudioClip GetCurrentClipAudio()
    {
        return GetCurrentClip().Value;
    }

    public string GetCurrentClipName()
    {
        return GetCurrentClip().Key;
    }

    public static MusicLoader Instance()
    {
        return GameObject.Find("MusicLoader").GetComponent<MusicLoader>();
    }
}