
using System;
using System.Collections.Generic;

public class UtilityScoring
{
    static UtilityScoring()
    {

    }

    // Give NPC stats and the utility of find 
    // -1 if it cant be found
    // >0 and proportional to inverse of distance if it can
    public static List<Tuple<string, double>> ScoreGetItem(Dictionary<string, int> personality, 
                                    Dictionary<string, int> skills,
                                    Dictionary<string, int> pseudoSkills,
                                    double utilityFind)
    {
        List<Tuple<string, double>> uScores = new List<Tuple<string, double>>();
        uScores.Add(new Tuple<string, double>("find", utilityFind));
        uScores.Add(new Tuple<string, double>("steal", 
                                                UtilitySteal(
                                                            personality["conscience"],
                                                            personality["confidence"],
                                                            skills["pickpocket"],
                                                            skills["stealth"]
                                                            )));
        uScores.Add(new Tuple<string, double>("buy", 
                                                UtilityBuy(
                                                            pseudoSkills["wealth"],
                                                            personality["carelessness"]
                                                            )));
        uScores.Add(new Tuple<string, double>("persuade", 
                                                UtilityPersuade(
                                                            skills["charisma"],
                                                            personality["friendliness"],
                                                            personality["confidence"]
                                                            )));
        uScores.Add(new Tuple<string, double>("intimidate", 
                                                UtilityIntimidate(
                                                            skills["combat"],
                                                            personality["friendliness"],
                                                            personality["confidence"]
                                                            )));
        uScores.Add(new Tuple<string, double>("craft", 
                                                UtilityCraft(
                                                            skills["crafts"],
                                                            personality["confidence"]
                                                            )));

        uScores.Sort((x, y) => y.Item2.CompareTo(x.Item2));

        return uScores;

    }



    private static double SlopeGivenConfidence(double confidence)
    {
        double c = confidence / 100.0;
        double slope = 0.0f;

        // arbitrary constants to shape the function
        double vertDisplacement = 0.25;
        double exponentMultiplier = 2.34;
        double xIncrement = 0.23;

        if (confidence > -50.0)
            slope = 1 + 2 * Math.Pow(c, 3);
        else
            slope = vertDisplacement + Math.Pow(3, exponentMultiplier * (c + xIncrement));

        return slope;
    }

    private static double PerceivedSkill(double realSkill, double confidence)
    {
        return SlopeGivenConfidence(confidence) * realSkill;
    }


    private static double UtilitySteal(double conscience, double confidence, double pickpocket, double stealth)
    {
        double p = PerceivedSkill(pickpocket, confidence);
        double s = PerceivedSkill(stealth, confidence);

        double skillProductMod = 0.014;
        double sigmoidSize = 160.0;
        double halfSize = 80.0; // sigmoidSize * 0.5;
        double sigmoidHardness = 0.016;

        return skillProductMod * p * s + halfSize + -sigmoidSize / (1 + Math.Exp(-sigmoidHardness * conscience));
    }

    private static double UtilityBuy(double wealth, double carelessness)
    {
        double multiplier = 0.4;
        double padding = 1.5;
        double xMod = 0.015708;

        return multiplier * wealth * (padding + Math.Sin(xMod * carelessness));
    }

    private static double UtilityPersuade(double charisma, double friendliness, double confidence)
    {
        double c = PerceivedSkill(charisma, confidence);

        double vertOffset = -100.0;
        double sigmoidSize = 200.0;
        double xMod = 0.02;
        double frDivisor = 200;

        // friendliness adds to the utility score
        return vertOffset + sigmoidSize / (1 + Math.Exp(-xMod * c - friendliness / frDivisor));
    }

    private static double UtilityIntimidate(double combat, double friendliness, double confidence)
    {
        double c = PerceivedSkill(combat, confidence);

        double vertOffset = -101.0;
        double sigmoidSize = 200.0;
        double xMod = 0.02;
        double frDivisor = 200;

        // friendliness subtracts from the utility score
        return vertOffset + sigmoidSize / (1 + Math.Exp(-xMod * c + friendliness / frDivisor));
    }

    private static double UtilityCraft(double crafts, double confidence)
    {
        double c = PerceivedSkill(crafts, confidence);

        double xMod = 0.01;

        return xMod * Math.Pow(c, 2);

    }
}
