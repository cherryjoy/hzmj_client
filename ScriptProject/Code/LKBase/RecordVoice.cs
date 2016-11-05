using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using UnityEngine;

public class RecordVoice : MonoBehaviour
{
	private List<float> data2 = new List<float>();
	private const int HEADER_SIZE = 44;
	private AudioClip voice;
	public string mfilename;
	public static RecordVoice instance;
	private float[] data;

	private void Start()
	{
		RecordVoice.instance = this;
	}

	public void StartRecord()
	{
		this.voice = Microphone.Start(Microphone.devices[0], false, 20, 10000);
	}

	public void Stop()
	{
		Microphone.End(Microphone.devices[0]);
	}

	public void EndRecord(int roomId)
	{
		DateTime now = DateTime.Now;
		this.mfilename = now.Year.ToString() + string.Empty + (object)now.Month + string.Empty + (object)now.Day + string.Empty + (object)now.Hour + string.Empty + (object)now.Minute + string.Empty + (object)now.Second + string.Empty + (object)now.Millisecond + roomId;
		Microphone.End(Microphone.devices[0]);
		this.voice.name = this.mfilename;
		this.data = new float[200000];
		this.voice.GetData(this.data, 0);
		this.data2.Clear();
		for (int index = 0; index < this.data.Length; ++index)
		{
			if ((double)this.data[index] != 0.0)
				this.data2.Add(this.data[index]);
		}
		AudioClip clip = AudioClip.Create(this.mfilename, this.data2.Count, 1, 10000, false);
		clip.SetData(this.data2.ToArray(), 0);
		if (RecordVoice.Save(this.mfilename + ".wav", clip))
			StartCoroutine(this.UpLoad(File.ReadAllBytes(Application.persistentDataPath + "/" + this.mfilename + ".wav"), this.mfilename + ".wav"));
		//Voice.Add(clip, MJPlayers.desk.yourSeat);
	}

	private void Update()
	{
	}

	public static bool Save(string filename, AudioClip clip)
	{
		string str = Path.Combine(Application.persistentDataPath, filename);
		Debug.Log("voice filename: " + str);
		Directory.CreateDirectory(Path.GetDirectoryName(str));
		using (FileStream empty = RecordVoice.CreateEmpty(str))
		{
			RecordVoice.ConvertAndWrite(empty, clip);
			RecordVoice.WriteHeader(empty, clip);
		}
		return true;
	}

	private static void WriteHeader(FileStream fileStream, AudioClip clip)
	{
		int frequency = clip.frequency;
		int channels = clip.channels;
		int samples = clip.samples;
		fileStream.Seek(0L, SeekOrigin.Begin);
		byte[] bytes1 = Encoding.UTF8.GetBytes("RIFF");
		fileStream.Write(bytes1, 0, 4);
		byte[] bytes2 = BitConverter.GetBytes(fileStream.Length - 8L);
		fileStream.Write(bytes2, 0, 4);
		byte[] bytes3 = Encoding.UTF8.GetBytes("WAVE");
		fileStream.Write(bytes3, 0, 4);
		byte[] bytes4 = Encoding.UTF8.GetBytes("fmt ");
		fileStream.Write(bytes4, 0, 4);
		byte[] bytes5 = BitConverter.GetBytes(16);
		fileStream.Write(bytes5, 0, 4);
		byte[] bytes6 = BitConverter.GetBytes((ushort)1);
		fileStream.Write(bytes6, 0, 2);
		byte[] bytes7 = BitConverter.GetBytes(channels);
		fileStream.Write(bytes7, 0, 2);
		byte[] bytes8 = BitConverter.GetBytes(frequency);
		fileStream.Write(bytes8, 0, 4);
		byte[] bytes9 = BitConverter.GetBytes(frequency * channels * 2);
		fileStream.Write(bytes9, 0, 4);
		ushort num = (ushort)(channels * 2);
		fileStream.Write(BitConverter.GetBytes(num), 0, 2);
		byte[] bytes10 = BitConverter.GetBytes((ushort)16);
		fileStream.Write(bytes10, 0, 2);
		byte[] bytes11 = Encoding.UTF8.GetBytes("data");
		fileStream.Write(bytes11, 0, 4);
		byte[] bytes12 = BitConverter.GetBytes(samples * channels * 2);
		fileStream.Write(bytes12, 0, 4);
	}

	private static FileStream CreateEmpty(string filepath)
	{
		if (File.Exists(filepath))
			File.Delete(filepath);
		FileStream fileStream = new FileStream(filepath, FileMode.Create);
		byte num = 0;
		for (int index = 0; index < 44; ++index)
			fileStream.WriteByte(num);
		return fileStream;
	}

	private static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
	{
		float[] data = new float[clip.samples];
		clip.GetData(data, 0);
		short[] numArray1 = new short[data.Length];
		byte[] array = new byte[data.Length * 2];
		int maxValue = (int)short.MaxValue;
		for (int index = 0; index < data.Length; ++index)
		{
			numArray1[index] = (short)((double)data[index] * (double)maxValue);
			BitConverter.GetBytes(numArray1[index]).CopyTo((Array)array, index * 2);
		}
		fileStream.Write(array, 0, array.Length);
	}


	public IEnumerator UpLoad(byte[] data, string filename)
	{
		//WWW = new WWW("");
		//yield return WWW;
		yield return null;
	}

	public static void DownLoad(string url, int seat)
	{
		RecordVoice.instance.StartCoroutine(RecordVoice.instance.DownloadVoice(url, seat));
	}


	public IEnumerator DownloadVoice(string url, int seat)
	{
		yield return null;	
	}
}
