using UnityEngine;
using System.Collections;

public class Animate
{
    public enum Event
    {
        Start,
        Running,
        End
    }

    public delegate void LerpCallback(float value, Event ev);
    public delegate void LerpCallback<T>(T value, Event ev);
    public delegate float LerpTransform(float value);

    public static float Linear(float value)
    {
        return value;
    }

    public static float QuadraticIn(float value)
    {
        return value * value;
    }

    public static float QuadraticOut(float value)
    {
        return -value * (value - 2.0f);
    }

    public static float QuadraticInOut(float value)
    {
        if (value < 0.5f)
            return 0.5f * QuadraticIn(value * 2.0f);
        else
            return 0.5f + 0.5f * QuadraticOut((value - 0.5f) * 2.0f);
    }

    public static float CubicIn(float value)
    {
        return value * value * value;
    }

    public static float CubicOut(float value)
    {
        value = 1.0f - value;
        value *= value;
        value *= value;
        return value;
    }

    public static float CubicInOut(float value)
    {
        if (value < 0.5f)
            return 0.5f * QuadraticIn(value * 2.0f);
        else
            return 0.5f + 0.5f * QuadraticOut((value - 0.5f) * 2.0f);
    }

    public static IEnumerator Lerp(float delay, float duration, LerpCallback callback, LerpTransform transform)
    {
        float time = 0.0f;
        while (time < delay)
        {
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        time = 0.0f;
        callback(0.0f, Event.Start);
        while (time < duration)
        {
            time += Time.fixedDeltaTime;
            callback(transform(Mathf.Clamp01(time / duration)), Event.Running);
            yield return new WaitForFixedUpdate();
        }
        callback(1.0f, Event.End);
    }

    public static IEnumerator LerpTo(float delay, float duration, float from, float to, LerpCallback<float> callback, LerpTransform transform)
    {
        return Lerp(delay, duration, (f, ev) => callback(from + f * (to - from), ev), transform);
    }

    public static IEnumerator LerpTo(float delay, float duration, Vector3 from, Vector3 to, LerpCallback<Vector3> callback, LerpTransform transform)
    {
        return Lerp(delay, duration, (f, ev) => callback(from + f * (to - from), ev), transform);
    }

    public static IEnumerator LerpTo(float delay, float duration, Color from, Color to, LerpCallback<Color> callback, LerpTransform transform)
    {
        return Lerp(delay, duration, (f, ev) => callback(from + f * (to - from), ev), transform);
    }

    public static IEnumerator LerpTo(float delay, float duration, Quaternion from, Quaternion to, LerpCallback<Quaternion> callback, LerpTransform transform)
    {
        return Lerp(delay, duration, (f, ev) => callback(Quaternion.Slerp(from, to, f), ev), transform);
    }
}
