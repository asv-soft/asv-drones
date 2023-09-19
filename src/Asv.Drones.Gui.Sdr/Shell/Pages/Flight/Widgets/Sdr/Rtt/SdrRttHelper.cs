namespace Asv.Drones.Gui.Sdr;

public static class SdrRttHelper
{
    #region Enums
    public static IEnumerable<string> GetLlzChannels()
    {
        yield return "18X";
        yield return "18Y";
        yield return "20X";
        yield return "20Y";
        yield return "22X";
        yield return "22Y";
        yield return "24X";
        yield return "24Y";
        yield return "26X";
        yield return "26Y";
        yield return "28X";
        yield return "28Y";
        yield return "30X";
        yield return "30Y";
        yield return "32X";
        yield return "32Y";
        yield return "34X";
        yield return "34Y";
        yield return "36X";
        yield return "36Y";
        yield return "38X";
        yield return "38Y";
        yield return "40X";
        yield return "40Y";
        yield return "42X";
        yield return "42Y";
        yield return "44X";
        yield return "44Y";
        yield return "46X";
        yield return "46Y";
        yield return "48X";
        yield return "48Y";
        yield return "50X";
        yield return "50Y";
        yield return "52X";
        yield return "52Y";
        yield return "54X";
        yield return "54Y";
        yield return "56X";
        yield return "56Y";
    }
    public static IEnumerable<string> GetVorChannels()
    {
        yield return "17X";
        yield return "17Y";
        yield return "19X";
        yield return "19Y";
        yield return "21X";
        yield return "21Y";
        yield return "23X";
        yield return "23Y";
        yield return "25X";
        yield return "25Y";
        yield return "27X";
        yield return "27Y";
        yield return "29X";
        yield return "29Y";
        yield return "31X";
        yield return "31Y";
        yield return "33X";
        yield return "33Y";
        yield return "35X";
        yield return "35Y";
        yield return "37X";
        yield return "37Y";
        yield return "39X";
        yield return "39Y";
        yield return "41X";
        yield return "41Y";
        yield return "43X";
        yield return "43Y";
        yield return "45X";
        yield return "45Y";
        yield return "47X";
        yield return "47Y";
        yield return "49X";
        yield return "49Y";
        yield return "51X";
        yield return "51Y";
        yield return "53X";
        yield return "53Y";
        yield return "55X";
        yield return "55Y";
        yield return "57X";
        yield return "57Y";
        yield return "58X";
        yield return "58Y";
        yield return "59X";
        yield return "59Y";
        yield return "70X";
        yield return "70Y";
        yield return "71X";
        yield return "71Y";
        yield return "72X";
        yield return "72Y";
        yield return "73X";
        yield return "73Y";
        yield return "74X";
        yield return "74Y";
        yield return "75X";
        yield return "75Y";
        yield return "76X";
        yield return "76Y";
        yield return "77X";
        yield return "77Y";
        yield return "78X";
        yield return "78Y";
        yield return "79X";
        yield return "79Y";
        yield return "80X";
        yield return "80Y";
        yield return "81X";
        yield return "81Y";
        yield return "82X";
        yield return "82Y";
        yield return "83X";
        yield return "83Y";
        yield return "84X";
        yield return "84Y";
        yield return "85X";
        yield return "85Y";
        yield return "86X";
        yield return "86Y";
        yield return "87X";
        yield return "87Y";
        yield return "88X";
        yield return "88Y";
        yield return "89X";
        yield return "89Y";
        yield return "90X";
        yield return "90Y";
        yield return "91X";
        yield return "91Y";
        yield return "92X";
        yield return "92Y";
        yield return "93X";
        yield return "93Y";
        yield return "94X";
        yield return "94Y";
        yield return "95X";
        yield return "95Y";
        yield return "96X";
        yield return "96Y";
        yield return "97X";
        yield return "97Y";
        yield return "98X";
        yield return "98Y";
        yield return "99X";
        yield return "99Y";
        yield return "100X";
        yield return "100Y";
        yield return "101X";
        yield return "101Y";
        yield return "102X";
        yield return "102Y";
        yield return "103X";
        yield return "103Y";
        yield return "104X";
        yield return "104Y";
        yield return "105X";
        yield return "105Y";
        yield return "106X";
        yield return "106Y";
        yield return "107X";
        yield return "107Y";
        yield return "108X";
        yield return "108Y";
        yield return "109X";
        yield return "109Y";
        yield return "110X";
        yield return "110Y";
        yield return "111X";
        yield return "111Y";
        yield return "112X";
        yield return "112Y";
        yield return "113X";
        yield return "113Y";
        yield return "114X";
        yield return "114Y";
        yield return "115X";
        yield return "115Y";
        yield return "116X";
        yield return "116Y";
        yield return "117X";
        yield return "117Y";
        yield return "118X";
        yield return "118Y";
        yield return "119X";
        yield return "119Y";
        yield return "120X";
        yield return "120Y";
        yield return "121X";
        yield return "121Y";
        yield return "122X";
        yield return "122Y";
        yield return "123X";
        yield return "123Y";
        yield return "124X";
        yield return "124Y";
        yield return "125X";
        yield return "125Y";
        yield return "126X";
        yield return "126Y";
    }
    #endregion
    
    #region Direct
    public static string GetVorChannelFromVorFrequency(ulong frequency)
    {
        switch (frequency)
        {
            case 108_000_000:
                return "17X";
            case 108_050_000:
                return "17Y";
            case 108_200_000:
                return "19X";
            case 108_250_000:
                return "19Y";
            case 108_400_000:
                return "21X";
            case 108_450_000:
                return "21Y";
            case 108_600_000:
                return "23X";
            case 108_650_000:
                return "23Y";
            case 108_800_000:
                return "25X";
            case 108_850_000:
                return "25Y";
            case 109_000_000:
                return "27X";
            case 109_050_000:
                return "27Y";
            case 109_200_000:
                return "29X";
            case 109_250_000:
                return "29Y";
            case 109_400_000:
                return "31X";
            case 109_450_000:
                return "31Y";
            case 109_600_000:
                return "33X";
            case 109_650_000:
                return "33Y";
            case 109_800_000:
                return "35X";
            case 109_850_000:
                return "35Y";
            case 110_000_000:
                return "37X";
            case 110_050_000:
                return "37Y";
            case 110_200_000:
                return "39X";
            case 110_250_000:
                return "39Y";
            case 110_400_000:
                return "41X";
            case 110_450_000:
                return "41Y";
            case 110_600_000:
                return "43X";
            case 110_650_000:
                return "43Y";
            case 110_800_000:
                return "45X";
            case 110_850_000:
                return "45Y";
            case 111_000_000:
                return "47X";
            case 111_050_000:
                return "47Y";
            case 111_200_000:
                return "49X";
            case 111_250_000:
                return "49Y";
            case 111_400_000:
                return "51X";
            case 111_450_000:
                return "51Y";
            case 111_600_000:
                return "53X";
            case 111_650_000:
                return "53Y";
            case 111_800_000:
                return "55X";
            case 111_850_000:
                return "55Y";
            case 112_000_000:
                return "57X";
            case 112_050_000:
                return "57Y";
            case 112_100_000:
                return "58X";
            case 112_150_000:
                return "58Y";
            case 112_200_000:
                return "59X";
            case 112_250_000:
                return "59Y";
            case 112_300_000:
                return "70X";
            case 112_350_000:
                return "70Y";
            case 112_400_000:
                return "71X";
            case 112_450_000:
                return "71Y";
            case 112_500_000:
                return "72X";
            case 112_550_000:
                return "72Y";
            case 112_600_000:
                return "73X";
            case 112_650_000:
                return "73Y";
            case 112_700_000:
                return "74X";
            case 112_750_000:
                return "74Y";
            case 112_800_000:
                return "75X";
            case 112_850_000:
                return "75Y";
            case 112_900_000:
                return "76X";
            case 112_950_000:
                return "76Y";
            case 113_000_000:
                return "77X";
            case 113_050_000:
                return "77Y";
            case 113_100_000:
                return "78X";
            case 113_150_000:
                return "78Y";
            case 113_200_000:
                return "79X";
            case 113_250_000:
                return "79Y";
            case 113_300_000:
                return "80X";
            case 113_350_000:
                return "80Y";
            case 113_400_000:
                return "81X";
            case 113_450_000:
                return "81Y";
            case 113_500_000:
                return "82X";
            case 113_550_000:
                return "82Y";
            case 113_600_000:
                return "83X";
            case 113_650_000:
                return "83Y";
            case 113_700_000:
                return "84X";
            case 113_750_000:
                return "84Y";
            case 113_800_000:
                return "85X";
            case 113_850_000:
                return "85Y";
            case 113_900_000:
                return "86X";
            case 113_950_000:
                return "86Y";
            case 114_000_000:
                return "87X";
            case 114_050_000:
                return "87Y";
            case 114_100_000:
                return "88X";
            case 114_150_000:
                return "88Y";
            case 114_200_000:
                return "89X";
            case 114_250_000:
                return "89Y";
            case 114_300_000:
                return "90X";
            case 114_350_000:
                return "90Y";
            case 114_400_000:
                return "91X";
            case 114_450_000:
                return "91Y";
            case 114_500_000:
                return "92X";
            case 114_550_000:
                return "92Y";
            case 114_600_000:
                return "93X";
            case 114_650_000:
                return "93Y";
            case 114_700_000:
                return "94X";
            case 114_750_000:
                return "94Y";
            case 114_800_000:
                return "95X";
            case 114_850_000:
                return "95Y";
            case 114_900_000:
                return "96X";
            case 114_950_000:
                return "96Y";
            case 115_000_000:
                return "97X";
            case 115_050_000:
                return "97Y";
            case 115_100_000:
                return "98X";
            case 115_150_000:
                return "98Y";
            case 115_200_000:
                return "99X";
            case 115_250_000:
                return "99Y";
            case 115_300_000:
                return "100X";
            case 115_350_000:
                return "100Y";
            case 115_400_000:
                return "101X";
            case 115_450_000:
                return "101Y";
            case 115_500_000:
                return "102X";
            case 115_550_000:
                return "102Y";
            case 115_600_000:
                return "103X";
            case 115_650_000:
                return "103Y";
            case 115_700_000:
                return "104X";
            case 115_750_000:
                return "104Y";
            case 115_800_000:
                return "105X";
            case 115_850_000:
                return "105Y";
            case 115_900_000:
                return "106X";
            case 115_950_000:
                return "106Y";
            case 116_000_000:
                return "107X";
            case 116_050_000:
                return "107Y";
            case 116_100_000:
                return "108X";
            case 116_150_000:
                return "108Y";
            case 116_200_000:
                return "109X";
            case 116_250_000:
                return "109Y";
            case 116_300_000:
                return "110X";
            case 116_350_000:
                return "110Y";
            case 116_400_000:
                return "111X";
            case 116_450_000:
                return "111Y";
            case 116_500_000:
                return "112X";
            case 116_550_000:
                return "112Y";
            case 116_600_000:
                return "113X";
            case 116_650_000:
                return "113Y";
            case 116_700_000:
                return "114X";
            case 116_750_000:
                return "114Y";
            case 116_800_000:
                return "115X";
            case 116_850_000:
                return "115Y";
            case 116_900_000:
                return "116X";
            case 116_950_000:
                return "116Y";
            case 117_000_000:
                return "117X";
            case 117_050_000:
                return "117Y";
            case 117_100_000:
                return "118X";
            case 117_150_000:
                return "118Y";
            case 117_200_000:
                return "119X";
            case 117_250_000:
                return "119Y";
            case 117_300_000:
                return "120X";
            case 117_350_000:
                return "120Y";
            case 117_400_000:
                return "121X";
            case 117_450_000:
                return "121Y";
            case 117_500_000:
                return "122X";
            case 117_550_000:
                return "122Y";
            case 117_600_000:
                return "123X";
            case 117_650_000:
                return "123Y";
            case 117_700_000:
                return "124X";
            case 117_750_000:
                return "124Y";
            case 117_800_000:
                return "125X";
            case 117_850_000:
                return "125Y";
            case 117_900_000:
                return "126X";
            case 117_950_000:
                return "126Y";
        }
        return RS.LLzSdrRttViewModel_ValueNotAvailable;
    }
    public static string GetIlsChannelFromLocalizerModeFrequency(ulong frequency)
    {
        switch (frequency)
        {
            case 108_100_000:
                return "18X";
            case 108_150_000:
                return "18Y";
            case 108_300_000:
                return "20X";
            case 108_350_000:
                return "20Y";
            case 108_500_000:
                return "22X";
            case 108_550_000:
                return "22Y";
            case 108_700_000:
                return "24X";
            case 108_750_000:
                return "24Y";
            case 108_900_000:
                return "26X";
            case 108_950_000:
                return "26Y";
            case 109_100_000:
                return "28X";
            case 109_150_000:
                return "28Y";
            case 109_300_000:
                return "30X";
            case 109_350_000:
                return "30Y";
            case 109_500_000:
                return "32X";
            case 109_550_000:
                return "32Y";
            case 109_700_000:
                return "34X";
            case 109_750_000:
                return "34Y";
            case 109_900_000:
                return "36X";
            case 109_950_000:
                return "36Y";
            case 110_100_000:
                return "38X";
            case 110_150_000:
                return "38Y";
            case 110_300_000:
                return "40X";
            case 110_350_000:
                return "40Y";
            case 110_500_000:
                return "42X";
            case 110_550_000:
                return "42Y";
            case 110_700_000:
                return "44X";
            case 110_750_000:
                return "44Y";
            case 110_900_000:
                return "46X";
            case 110_950_000:
                return "46Y";
            case 111_100_000:
                return "48X";
            case 111_150_000:
                return "48Y";
            case 111_300_000:
                return "50X";
            case 111_350_000:
                return "50Y";
            case 111_500_000:
                return "52X";
            case 111_550_000:
                return "52Y";
            case 111_700_000:
                return "54X";
            case 111_750_000:
                return "54Y";
            case 111_900_000:
                return "56X";
            case 111_950_000:
                return "56Y";
        }
        return RS.LLzSdrRttViewModel_ValueNotAvailable;
    }
    public static string GetIlsChannelFromGlidepathModeFrequency(ulong frequency)
    {
        switch (frequency)
        {
            case 334_700_000:
                return "18X";
            case 334_550_000:
                return "18Y";
            case 334_100_000:
                return "20X";
            case 333_950_000:
                return "20Y";
            case 329_900_000:
                return "22X";
            case 329_750_000:
                return "22Y";
            case 330_500_000:
                return "24X";
            case 330_350_000:
                return "24Y";
            case 329_300_000:
                return "26X";
            case 329_150_000:
                return "26Y";
            case 331_400_000:
                return "28X";
            case 331_250_000:
                return "28Y";
            case 332_000_000:
                return "30X";
            case 331_850_000:
                return "30Y";
            case 332_600_000:
                return "32X";
            case 332_450_000:
                return "32Y";
            case 333_200_000:
                return "34X";
            case 333_050_000:
                return "34Y";
            case 333_800_000:
                return "36X";
            case 333_650_000:
                return "36Y";
            case 334_400_000:
                return "38X";
            case 334_250_000:
                return "38Y";
            case 335_000_000:
                return "40X";
            case 334_850_000:
                return "40Y";
            case 329_600_000:
                return "42X";
            case 329_450_000:
                return "42Y";
            case 330_200_000:
                return "44X";
            case 330_050_000:
                return "44Y";
            case 330_800_000:
                return "46X";
            case 330_650_000:
                return "46Y";
            case 331_700_000:
                return "48X";
            case 331_550_000:
                return "48Y";
            case 332_300_000:
                return "50X";
            case 332_150_000:
                return "50Y";
            case 332_900_000:
                return "52X";
            case 332_750_000:
                return "52Y";
            case 333_500_000:
                return "54X";
            case 333_350_000:
                return "54Y";
            case 331_100_000:
                return "56X";
            case 330_950_000:
                return "56Y";
        }
        return RS.LLzSdrRttViewModel_ValueNotAvailable;
    }
    #endregion
    
    #region Reversed
    public static ulong GetVorFrequencyFromVorChannel(string channel)
    {
        switch (channel)
        {
            case "17X":
                return 108_000_000;
            case "17Y":
                return 108_050_000;
            case "19X":
                return 108_200_000;
            case "19Y":
                return 108_250_000;
            case "21X":
                return 108_400_000;
            case "21Y":
                return 108_450_000;
            case "23X":
                return 108_600_000;
            case "23Y":
                return 108_650_000;
            case "25X":
                return 108_800_000;
            case "25Y":
                return 108_850_000;
            case "27X":
                return 109_000_000;
            case "27Y":
                return 109_050_000;
            case "29X":
                return 109_200_000;
            case "29Y":
                return 109_250_000;
            case "31X":
                return 109_400_000;
            case "31Y":
                return 109_450_000;
            case "33X":
                return 109_600_000;
            case "33Y":
                return 109_650_000;
            case "35X":
                return 109_800_000;
            case "35Y":
                return 109_850_000;
            case "37X":
                return 110_000_000;
            case "37Y":
                return 110_050_000;
            case "39X":
                return 110_200_000;
            case "39Y":
                return 110_250_000;
            case "41X":
                return 110_400_000;
            case "41Y":
                return 110_450_000;
            case "43X":
                return 110_600_000;
            case "43Y":
                return 110_650_000;
            case "45X":
                return 110_800_000;
            case "45Y":
                return 110_850_000;
            case "47X":
                return 111_000_000;
            case "47Y":
                return 111_050_000;
            case "49X":
                return 111_200_000;
            case "49Y":
                return 111_250_000;
            case "51X":
                return 111_400_000;
            case "51Y":
                return 111_450_000;
            case "53X":
                return 111_600_000;
            case "53Y":
                return 111_650_000;
            case "55X":
                return 111_800_000;
            case "55Y":
                return 111_850_000;
            case "57X":
                return 112_000_000;
            case "57Y":
                return 112_050_000;
            case "58X":
                return 112_100_000;
            case "58Y":
                return 112_150_000;
            case "59X":
                return 112_200_000;
            case "59Y":
                return 112_250_000;
            case "70X":
                return 112_300_000;
            case "70Y":
                return 112_350_000;
            case "71X":
                return 112_400_000;
            case "71Y":
                return 112_450_000;
            case "72X":
                return 112_500_000;
            case "72Y":
                return 112_550_000;
            case "73X":
                return 112_600_000;
            case "73Y":
                return 112_650_000;
            case "74X":
                return 112_700_000;
            case "74Y":
                return 112_750_000;
            case "75X":
                return 112_800_000;
            case "75Y":
                return 112_850_000;
            case "76X":
                return 112_900_000;
            case "76Y":
                return 112_950_000;
            case "77X":
                return 113_000_000;
            case "77Y":
                return 113_050_000;
            case "78X":
                return 113_100_000;
            case "78Y":
                return 113_150_000;
            case "79X":
                return 113_200_000;
            case "79Y":
                return 113_250_000;
            case "80X":
                return 113_300_000;
            case "80Y":
                return 113_350_000;
            case "81X":
                return 113_400_000;
            case "81Y":
                return 113_450_000;
            case "82X":
                return 113_500_000;
            case "82Y":
                return 113_550_000;
            case "83X":
                return 113_600_000;
            case "83Y":
                return 113_650_000;
            case "84X":
                return 113_700_000;
            case "84Y":
                return 113_750_000;
            case "85X":
                return 113_800_000;
            case "85Y":
                return 113_850_000;
            case "86X":
                return 113_900_000;
            case "86Y":
                return 113_950_000;
            case "87X":
                return 114_000_000;
            case "87Y":
                return 114_050_000;
            case "88X":
                return 114_100_000;
            case "88Y":
                return 114_150_000;
            case "89X":
                return 114_200_000;
            case "89Y":
                return 114_250_000;
            case "90X":
                return 114_300_000;
            case "90Y":
                return 114_350_000;
            case "91X":
                return 114_400_000;
            case "91Y":
                return 114_450_000;
            case "92X":
                return 114_500_000;
            case "92Y":
                return 114_550_000;
            case "93X":
                return 114_600_000;
            case "93Y":
                return 114_650_000;
            case "94X":
                return 114_700_000;
            case "94Y":
                return 114_750_000;
            case "95X":
                return 114_800_000;
            case "95Y":
                return 114_850_000;
            case "96X":
                return 114_900_000;
            case "96Y":
                return 114_950_000;
            case "97X":
                return 115_000_000;
            case "97Y":
                return 115_050_000;
            case "98X":
                return 115_100_000;
            case "98Y":
                return 115_150_000;
            case "99X":
                return 115_200_000;
            case "99Y":
                return 115_250_000;
            case "100X":
                return 115_300_000;
            case "100Y":
                return 115_350_000;
            case "101X":
                return 115_400_000;
            case "101Y":
                return 115_450_000;
            case "102X":
                return 115_500_000;
            case "102Y":
                return 115_550_000;
            case "103X":
                return 115_600_000;
            case "103Y":
                return 115_650_000;
            case "104X":
                return 115_700_000;
            case "104Y":
                return 115_750_000;
            case "105X":
                return 115_800_000;
            case "105Y":
                return 115_850_000;
            case "106X":
                return 115_900_000;
            case "106Y":
                return 115_950_000;
            case "107X":
                return 116_000_000;
            case "107Y":
                return 116_050_000;
            case "108X":
                return 116_100_000;
            case "108Y":
                return 116_150_000;
            case "109X":
                return 116_200_000;
            case "109Y":
                return 116_250_000;
            case "110X":
                return 116_300_000;
            case "110Y":
                return 116_350_000;
            case "111X":
                return 116_400_000;
            case "111Y":
                return 116_450_000;
            case "112X":
                return 116_500_000;
            case "112Y":
                return 116_550_000;
            case "113X":
                return 116_600_000;
            case "113Y":
                return 116_650_000;
            case "114X":
                return 116_700_000;
            case "114Y":
                return 116_750_000;
            case "115X":
                return 116_800_000;
            case "115Y":
                return 116_850_000;
            case "116X":
                return 116_900_000;
            case "116Y":
                return 116_950_000;
            case "117X":
                return 117_000_000;
            case "117Y":
                return 117_050_000;
            case "118X":
                return 117_100_000;
            case "118Y":
                return 117_150_000;
            case "119X":
                return 117_200_000;
            case "119Y":
                return 117_250_000;
            case "120X":
                return 117_300_000;
            case "120Y":
                return 117_350_000;
            case "121X":
                return 117_400_000;
            case "121Y":
                return 117_450_000;
            case "122X":
                return 117_500_000;
            case "122Y":
                return 117_550_000;
            case "123X":
                return 117_600_000;
            case "123Y":
                return 117_650_000;
            case "124X":
                return 117_700_000;
            case "124Y":
                return 117_750_000;
            case "125X":
                return 117_800_000;
            case "125Y":
                return 117_850_000;
            case "126X":
                return 117_900_000;
            case "126Y":
                return 117_950_000;
        }
        return 0;
    }
    public static ulong GetLocalizerModeFrequencyFromIlsChannel(string channel)
    {
        switch (channel)
        {
            case "18X":
                return 108_100_000;
            case "18Y":
                return 108_150_000;
            case "20X":
                return 108_300_000;
            case "20Y":
                return 108_350_000;
            case "22X":
                return 108_500_000;
            case "22Y":
                return 108_550_000;
            case "24X":
                return 108_700_000;
            case "24Y":
                return 108_750_000;
            case "26X":
                return 108_900_000;
            case "26Y":
                return 108_950_000;
            case "28X":
                return 109_100_000;
            case "28Y":
                return 109_150_000;
            case "30X":
                return 109_300_000;
            case "30Y":
                return 109_350_000;
            case "32X":
                return 109_500_000;
            case "32Y":
                return 109_550_000;
            case "34X":
                return 109_700_000;
            case "34Y":
                return 109_750_000;
            case "36X":
                return 109_900_000;
            case "36Y":
                return 109_950_000;
            case "38X":
                return 110_100_000;
            case "38Y":
                return 110_150_000;
            case "40X":
                return 110_300_000;
            case "40Y":
                return 110_350_000;
            case "42X":
                return 110_500_000;
            case "42Y":
                return 110_550_000;
            case "44X":
                return 110_700_000;
            case "44Y":
                return 110_750_000;
            case "46X":
                return 110_900_000;
            case "46Y":
                return 110_950_000;
            case "48X":
                return 111_100_000;
            case "48Y":
                return 111_150_000;
            case "50X":
                return 111_300_000;
            case "50Y":
                return 111_350_000;
            case "52X":
                return 111_500_000;
            case "52Y":
                return 111_550_000;
            case "54X":
                return 111_700_000;
            case "54Y":
                return 111_750_000;
            case "56X":
                return 111_900_000;
            case "56Y":
                return 111_950_000;
        }
        return 0;
    }
    public static ulong GetGlidepathModeFrequencyFromIlsChannel(string channel)
    {
        switch (channel) 
        {
            case "18X":
                return 334_700_000;
            case "18Y":
                return 334_550_000;
            case "20X":
                return 334_100_000;
            case "20Y":
                return 333_950_000;
            case "22X":
                return 329_900_000;
            case "22Y":
                return 329_750_000;
            case "24X":
                return 330_500_000;
            case "24Y":
                return 330_350_000;
            case "26X":
                return 329_300_000;
            case "26Y":
                return 329_150_000;
            case "28X":
                return 331_400_000;
            case "28Y":
                return 331_250_000;
            case "30X":
                return 332_000_000;
            case "30Y":
                return 331_850_000;
            case "32X":
                return 332_600_000;
            case "32Y":
                return 332_450_000;
            case "34X":
                return 333_200_000;
            case "34Y":
                return 333_050_000;
            case "36X":
                return 333_800_000;
            case "36Y":
                return 333_650_000;
            case "38X":
                return 334_400_000;
            case "38Y":
                return 334_250_000;
            case "40X":
                return 335_000_000;
            case "40Y":
                return 334_850_000;
            case "42X":
                return 329_600_000;
            case "42Y":
                return 329_450_000;
            case "44X":
                return 330_200_000;
            case "44Y":
                return 330_050_000;
            case "46X":
                return 330_800_000;
            case "46Y":
                return 330_650_000;
            case "48X":
                return 331_700_000;
            case "48Y":
                return 331_550_000;
            case "50X":
                return 332_300_000;
            case "50Y":
                return 332_150_000;
            case "52X":
                return 332_900_000;
            case "52Y":
                return 332_750_000;
            case "54X":
                return 333_500_000;
            case "54Y":
                return 333_350_000;
            case "56X":
                return 331_100_000;
            case "56Y":
                return 330_950_000;
        }
        return 0;
    }
    #endregion
}