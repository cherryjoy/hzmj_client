using UnityEngine;

public class GameQualityChecker : Singleton<GameQualityChecker>
{
    public enum GameQualityLevel
    {
        lowLevel,
        highLevel,
    }

    private GameQualityLevel mGameQuality;

    void Awake()
    {

    }

    void Start()
    {

    }

    public GameQualityLevel EstimateGameQualityLevel()
    {
        mGameQuality = GameQualityLevel.lowLevel;

#if UNITY_ANDROID
        mGameQuality = (GameQualityLevel)CheckAndroidDeviceQuality();
#elif UNITY_IPHONE
        mGameQuality = (GameQualityLevel)CheckIOSDeviceQuality();
#elif UNITY_EDITOR
        mGameQuality = GameQualityLevel.highLevel;
#elif UNITY_STANDALONE
        mGameQuality = GameQualityLevel.highLevel;
#endif

        return mGameQuality;
    }

    public void SetGameQuality(GameQualityLevel gameQualityLevel)
    {
        mGameQuality = gameQualityLevel;

        if (mGameQuality == GameQualityLevel.highLevel)
        {
            Shader.globalMaximumLOD = 600;
        }
        else if (mGameQuality == GameQualityLevel.lowLevel)
        {
            Shader.globalMaximumLOD = 100;
        }
    }

    public GameQualityLevel GetQualityLevel()
    {
        return mGameQuality;
    }

    private GameQualityLevel EstimateIOSDevice()
    {
        GameQualityLevel gameQualityLevel = GameQualityLevel.highLevel;

        if (SystemInfo.processorCount <= 1)
        {
            gameQualityLevel = GameQualityLevel.lowLevel;
        }

        if (SystemInfo.systemMemorySize <= 256)
        {
            gameQualityLevel = GameQualityLevel.lowLevel;
        }

        if (SystemInfo.systemMemorySize <= 256)
        {
            gameQualityLevel = GameQualityLevel.lowLevel;
        }

        return gameQualityLevel;
    }

    private int CheckAndroidDeviceQuality()
    {
        int qualityLevel = 0;

        if (SystemInfo.processorCount > 2 && SystemInfo.systemMemorySize > 1024 && SystemInfo.graphicsMemorySize >= 512)
        {
            qualityLevel = 1;
        }

        return qualityLevel;
    }

    private int CheckIOSDeviceQuality()
    {
        int qualityLevel = 0;

        UnityEngine.iOS.DeviceGeneration iOSGeneration = UnityEngine.iOS.Device.generation;
        if (iOSGeneration >= UnityEngine.iOS.DeviceGeneration.iPhone5S)
        {
            if (iOSGeneration == UnityEngine.iOS.DeviceGeneration.iPadMini2Gen)
            {
                qualityLevel = 0;
            }
            else
            {
                qualityLevel = 1;
            }
        }
        else
        {
            qualityLevel = 0;
        }

        return qualityLevel;
    }

}

