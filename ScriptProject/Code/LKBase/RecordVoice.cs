using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class RecordVoice : Singleton<RecordVoice>
{
	private List<float> data2 = new List<float>();
	private const int HEADER_SIZE = 44;
	private AudioClip voice;
	public string mfilename;
	private float[] data;
	private int audioLength;//录音的长度，单位为秒，ui上可能需要显示 
							//通常的无损音质的采样率是44100，即每秒音频用44100个float数据表示，但是语音只需8000（通常移动电话是8000）就够了  
							//不然音频数据太大，不利于传输和存储  
	public const int SamplingRate = 10000;

	public void StartRecord()
	{
		if (Microphone.devices.Length == 0)
		{
			UnityEngine.Debug.LogError("Microphone.devices is null");
			return;
		}

		Microphone.End(Microphone.devices[0]);
		audioLength = 20;
		this.voice = Microphone.Start(Microphone.devices[0], false, audioLength, SamplingRate);
	}

	public void EndRecord(int roomId, int roleId)
	{
		if (Microphone.devices.Length == 0)
		{
			UnityEngine.Debug.LogError("Microphone.devices is null");
			return;
		}

		int lastPos = Microphone.GetPosition(Microphone.devices[0]);
		if (Microphone.IsRecording(Microphone.devices[0])) //录音小于20秒 
		{
			audioLength = lastPos / SamplingRate;//录音时长  
		}

		Microphone.End(Microphone.devices[0]);
		if (audioLength < 1.0f) //录音小于1秒就不处理了
		{
			return;
		}

		DateTime now = DateTime.Now;
		this.mfilename = roomId + "_" + roleId + "_" + now.Year.ToString() + string.Empty + now.Month + string.Empty + now.Day + string.Empty +
			now.Hour + string.Empty + now.Minute + string.Empty + now.Second + string.Empty + now.Millisecond;
		this.voice.name = this.mfilename;
		this.data = new float[200000];
		this.voice.GetData(this.data, 0);
		this.data2.Clear();
		for (int index = 0; index < this.data.Length; ++index)
		{
			if ((double)this.data[index] != 0.0)
				this.data2.Add(this.data[index]);
		}
		UnityEngine.Debug.Log("count: " + data2.Count + ", length: " + audioLength + ", cliplength: " + voice.length);
		AudioClip clip = AudioClip.Create(this.mfilename, this.data2.Count, 1, 10000, false);
		clip.SetData(this.data2.ToArray(), 0);
		if (RecordVoice.instance.Save(this.mfilename + ".wav", clip))
		{
			//StartCoroutine(this.UpLoad(File.ReadAllBytes(Application.persistentDataPath + "/" + this.mfilename + ".wav"), this.mfilename + ".wav"));
		}
	}

	public bool Save(string filename, AudioClip clip)
	{
		string str = Path.Combine(Application.persistentDataPath, filename);
		UnityEngine.Debug.Log("voice filename: " + str);
		Directory.CreateDirectory(Path.GetDirectoryName(str));
		using (FileStream empty = RecordVoice.instance.CreateEmpty(str))
		{
			RecordVoice.instance.ConvertAndWrite(empty, clip);
			RecordVoice.instance.WriteHeader(empty, clip);
		}
		return true;
	}

	private void WriteHeader(FileStream fileStream, AudioClip clip)
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

	private FileStream CreateEmpty(string filepath)
	{
		if (File.Exists(filepath))
			File.Delete(filepath);
		FileStream fileStream = new FileStream(filepath, FileMode.Create);
		byte num = 0;
		for (int index = 0; index < 44; ++index)
			fileStream.WriteByte(num);
		return fileStream;
	}

	private void ConvertAndWrite(FileStream fileStream, AudioClip clip)
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

	public void DownLoad(string url, int seat)
	{
		//RecordVoice.instance.StartCoroutine(RecordVoice.instance.DownloadVoice(url, seat));
	}


	public IEnumerator DownloadVoice(string url, int seat)
	{
		yield return null;
	}
}