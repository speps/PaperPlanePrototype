using UnityEngine;

public class HelperConstants
{
#if UNITY_EDITOR
    public static string DataPath = "Data/Resources";
    public static string PathPopUps = "PopUps";
    public static string PathSketch = "Sketch";
    public static string PathSounds = "Sounds";
    public static string PathSoundEngine = "SoundEngine";
    public static string DataPathPopUps = DataPath + "/" + PathPopUps;
    public static string DataPathSketch = DataPath + "/" + PathSketch;
    public static string DataPathSounds = DataPath + "/" + PathSounds;
    public static string DataPathTweak = DataPath;
    public static string DataPathBook = DataPath;
    public static string DataPathSoundEngine =".";

    public static string FileTweak = "Tweak";
    public static string FileBook = "Book";

    public static string EditorPathPopUps
    {
        get
        {
            return Application.dataPath + "/" + DataPathPopUps;
        }
    }

    public static string EditorPathSketchs
    {
        get
        {
            return Application.dataPath + "/" + DataPathSketch;
        }
    }

    public static string EditorPathSounds
    {
        get
        {
            return Application.dataPath + "/" + DataPathSounds;
        }
    }

    public static string EditorPathTweak
    {
        get
        {
            return Application.dataPath + "/" + DataPathTweak;
        }
    }

    public static string EditorPathBook
    {
        get
        {
            return Application.dataPath + "/" + DataPathBook;
        }
    }

#endif

    public static string PrefixCam = "CAM_";
    public static string PrefixPopUp = "POP_";
    public static string PrefixPart = "PART_";
    public static string SuffixNoColliders = "_NC";

    public static bool IsCam(string cam)
    {
        return cam.StartsWith(PrefixCam);
    }

    public static bool ExtractNoCollidersName(string nc, out string name)
    {
        name = "";

        //Debug.Log(nc);

        if (nc.Length < SuffixNoColliders.Length)
            return false;
        if (nc.Substring(nc.Length - SuffixNoColliders.Length) != SuffixNoColliders)
            return false;

        name = nc.Substring(0, nc.Length - SuffixNoColliders.Length).Trim();
        return name != "";
    }

    public static bool IsPopUp(string pop)
    {
        string name;
        return ExtractPopUpName(pop, out name);
    }

    public static bool ExtractPopUpName(string pop, out string name)
    {
        name = "";

        if (!pop.StartsWith(PrefixPopUp))
            return false;

        name = pop.Substring(PrefixPopUp.Length).Trim();
        return name != "";
    }

    public static bool IsPart(string part)
    {
        int delay;
        bool noStrips;
        string name;
        return ExtractPart(part, out delay, out noStrips, out name);
    }

    public static bool ExtractPartName(string part, out string name)
    {
        int delay;
        bool noStrips;
        return ExtractPart(part, out delay, out noStrips, out name);
    }

    public static bool ExtractPart(string part, out int delay, out bool noStrips, out string name)
    {
        delay = 0;
        noStrips = false;
        name = "";

        if (!part.StartsWith(PrefixPart))
            return false;

        int index = PrefixPart.Length;

        if (part[index] == 'D')
        {
            int result = 0;
            index++;
            while (part[index] >= '0' && part[index] <= '9')
            {
                int number = part[index] - '0';
                result = result * 10 + number;
                index++;
            }
            index++; // _
            delay = result;
        }

        if (part.Substring(index).StartsWith("NS"))
        {
            noStrips = true;
            index += "NS_".Length;
        }

        if (part.Length > index)
        {
            name = part.Substring(index).Trim();
            return name != "";
        }

        return false;
    }
}
