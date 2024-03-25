﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SubtitleGenerator
{
    public static class Utils
    {
        public static Dictionary<string, string> languageCodes = new()
            {
                {"English", "en"},
                {"Serbian", "sr"},
                {"Hindi", "hi"},
                {"Spanish", "es"},
                {"Russian", "ru"},
                {"Korean", "ko"},
                {"French", "fr"},
                {"Japanese", "ja"},
                {"Portuguese", "pt"},
                {"Turkish", "tr"},
                {"Polish", "pl"},
                {"Catalan", "ca"},
                {"Dutch", "nl"},
                {"Arabic", "ar"},
                {"Swedish", "sv"},
                {"Italian", "it"},
                {"Indonesian", "id"},
                {"Macedonian", "mk" },
                {"Mandarin", "zh" }
        };
        public static int GetLangId(string languageString)
        {
            int langId = 50259;
            Dictionary<string, int> langToId = new Dictionary<string, int>
        {
            {"af", 50327},
            {"am", 50334},
            {"ar", 50272},
            {"as", 50350},
            {"az", 50304},
            {"ba", 50355},
            {"be", 50330},
            {"bg", 50292},
            {"bn", 50302},
            {"bo", 50347},
            {"br", 50309},
            {"bs", 50315},
            {"ca", 50270},
            {"cs", 50283},
            {"cy", 50297},
            {"da", 50285},
            {"de", 50261},
            {"el", 50281},
            {"en", 50259},
            {"es", 50262},
            {"et", 50307},
            {"eu", 50310},
            {"fa", 50300},
            {"fi", 50277},
            {"fo", 50338},
            {"fr", 50265},
            {"gl", 50319},
            {"gu", 50333},
            {"haw", 50352},
            {"ha", 50354},
            {"he", 50279},
            {"hi", 50276},
            {"hr", 50291},
            {"ht", 50339},
            {"hu", 50286},
            {"hy", 50312},
            {"id", 50275},
            {"is", 50311},
            {"it", 50274},
            {"ja", 50266},
            {"jw", 50356},
            {"ka", 50329},
            {"kk", 50316},
            {"km", 50323},
            {"kn", 50306},
            {"ko", 50264},
            {"la", 50294},
            {"lb", 50345},
            {"ln", 50353},
            {"lo", 50336},
            {"lt", 50293},
            {"lv", 50301},
            {"mg", 50349},
            {"mi", 50295},
            {"mk", 50308},
            {"ml", 50296},
            {"mn", 50314},
            {"mr", 50320},
            {"ms", 50282},
            {"mt", 50343},
            {"my", 50346},
            {"ne", 50313},
            {"nl", 50271},
            {"nn", 50342},
            {"no", 50288},
            {"oc", 50328},
            {"pa", 50321},
            {"pl", 50269},
            {"ps", 50340},
            {"pt", 50267},
            {"ro", 50284},
            {"ru", 50263},
            {"sa", 50344},
            {"sd", 50332},
            {"si", 50322},
            {"sk", 50298},
            {"sl", 50305},
            {"sn", 50324},
            {"so", 50326},
            {"sq", 50317},
            {"sr", 50303},
            {"su", 50357},
            {"sv", 50273},
            {"sw", 50318},
            {"ta", 50287},
            {"te", 50299},
            {"tg", 50331},
            {"th", 50289},
            {"tk", 50341},
            {"tl", 50325},
            {"tr", 50268},
            {"tt", 50335},
            {"ug", 50348},
            {"uk", 50260},
            {"ur", 50337},
            {"uz", 50351},
            {"vi", 50278},
            {"xh", 50322},
            {"yi", 50305},
            {"yo", 50324},
            {"zh", 50258},
            {"zu", 50321}
        };

            if (languageCodes.TryGetValue(languageString, out string langCode))
            {
                langId = langToId[langCode];
            }

            return langId;
        }

        public static string ConvertToSrt(string subtitleString, int batchIndex, int batchSizeInSeconds)
        {
            Regex pattern = new Regex(@"<\|([\d.]+)\|>([^<]+)<\|([\d.]+)\|>");
            MatchCollection matches = pattern.Matches(subtitleString);
            // Placeholder for srt content
            string srtContent = "";

            // Calculate the time offset based on the batch number. Each batch represents an additional 30 seconds.
            double batchOffset = batchIndex * batchSizeInSeconds; // 30 seconds per batch

            for (int i = 0; i < matches.Count; i++)
            {
                // Parse the original start and end times
                double start = double.Parse(matches[i].Groups[1].Value);
                double end = double.Parse(matches[i].Groups[3].Value);

                // Apply the batch offset to the start and end times
                start += batchOffset;
                end += batchOffset;

                // Convert the adjusted start and end times into the SRT format
                string startSrt = $"{(int)(start / 3600):D2}:{(int)((start % 3600) / 60):D2}:{(int)(start % 60):D2},{(int)((start * 1000) % 1000):D3}";
                string endSrt = $"{(int)(end / 3600):D2}:{(int)((end % 3600) / 60):D2}:{(int)(end % 60):D2},{(int)((end * 1000) % 1000):D3}";

                // Build the SRT content string, incrementing the subtitle index by 1 for readability
                srtContent += $"{i + 1}\n{startSrt} --> {endSrt}\n{matches[i].Groups[2].Value.Trim()}\n\n";
            }

            // The SaveSrtContentToTempFile method needs to exist and handle the saving of the SRT content to a file
            // Make sure to implement it or adjust this part as per your application's requirements
            return srtContent;
            //return SaveSrtContentToTempFile(srtContent, fileName);
        }

        public static string SaveSrtContentToTempFile(List<string> srtContent, string fileName)
        {
            string srtFilePath = "";
            try
            {
                // Use MyDocuments as the directory to save the SRT file
                string documentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                // Create a unique filename for the SRT file
                string uniqueFileName = $"{fileName}.srt";

                // Combine the documents folder path with the unique file name to get the full path
                srtFilePath = Path.Combine(documentsFolderPath, uniqueFileName);

                // Join the list of strings into a single string with newline characters
                string combinedContent = string.Join("\n", srtContent);

                // Write the combined string content to the file
                File.WriteAllText(srtFilePath, combinedContent);

                Console.WriteLine($"SRT file saved to: {srtFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving SRT file: {ex.Message}");
            }
            return srtFilePath;
        }
    }
}
