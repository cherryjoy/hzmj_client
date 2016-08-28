using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : Singleton<MusicManager>
{
	private float music_volume_ = 1;//音乐音量
    private float audio_volume_ = 1;//音效音量
	
	public float MusicVolume
	{
		set 
		{
			music_volume_ = value;
			AudioComp.volume = music_volume_; 
		}
		get { return music_volume_; }
	}
	public float AudioVolume
	{
		set { audio_volume_ = value; }
		get { return audio_volume_; }
	}

	private GameObject music_go_;
	private AudioSource audio_comp_;
    private const string win_music_path_ = "Music/Common/Win";
	public AudioSource AudioComp 
	{ 
		get 
		{ 
			if (audio_comp_ == null)
			{
				audio_comp_ = music_go_.GetComponent<AudioSource>();
				if (audio_comp_ == null)
					audio_comp_ = music_go_.AddComponent<AudioSource>();
			}

			return audio_comp_;
		}
	}

	public void PlayBackMusic(string music_path)
	{
		if (music_go_ == null)
            music_go_ = GameObject.Find("NeverDestroyObj");
        
        AudioClip clip = GetClip(music_path);
        AudioComp.clip = clip;
		AudioComp.loop = true;
		AudioComp.volume = music_volume_;
		AudioComp.Play();
	}

    public void SetBackMusicVolume(float volume) {
        AudioComp.volume = volume;
    }
    public void PlayMusicOnce(string music_path)
    {
        if (music_go_ == null)
            music_go_ = GameObject.Find("NeverDestroyObj");

        AudioClip clip = GetClip(music_path);
        AudioComp.clip = clip;
        AudioComp.loop = false;
        AudioComp.volume = music_volume_;
        AudioComp.Play();
    }
    public void PlayerWinMusic()
    {
        PlayMusicOnce(win_music_path_);
    }

	private AudioClip GetClip(string path)
	{
		AudioClip clip;
		clip = LoadClip(path);

		return clip;
	}

	public static AudioClip LoadClip(string path)
	{
		AudioClip clip = ResLoader.Load(path) as AudioClip;
		if (clip == null)
			LKDebug.LogError("can't load audio at path: " + path);
		return clip;
	}
}
