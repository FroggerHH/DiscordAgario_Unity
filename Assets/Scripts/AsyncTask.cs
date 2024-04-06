﻿using System.Threading.Tasks;

public static class AsyncTask
{
    public static async Task Delay(int milliseconds)
    {
#if !UNITY_WebGL
        await Task.Delay(milliseconds);
#else
        // Unity 2021 do NOT support Task.Delay() in WebGL
        float startTime = Time.realtimeSinceStartup;
        float delay = (float)milliseconds / 1000f;

        while (Time.realtimeSinceStartup - startTime < delay)
        {
            // Wait for the delay time to pass
            await Task.Yield();
        }
#endif
    }
}