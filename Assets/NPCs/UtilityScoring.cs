
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
    public static List<Tuple<string, double>> ScoreGetItem( Dictionary<string, int> personality, 
                                                            Dictionary<string, int> skills,
                                                            double utilityFind,
                                                            bool craftable)
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
                                                            skills["wealth"],
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
        if(craftable)
            uScores.Add(new Tuple<string, double>("craft", 
                                                    UtilityCraft(
                                                                skills["crafts"],
                                                                personality["confidence"]
                                                                )));

        uScores.Sort((x, y) => y.Item2.CompareTo(x.Item2));

        return uScores;

    }

    // Score all actions for convince and sort them by value
    public static List<Tuple<string, double>> ScoreConvince( Dictionary<string, int> personality,
                                                            Dictionary<string, int> skills)
    {
        List<Tuple<string, double>> uScores = new List<Tuple<string, double>>();
        uScores.Add(new Tuple<string, double>("bribe",
                                                UtilityHardBuy(
                                                            skills["wealth"],
                                                            personality["carelessness"]
                                                            )));
        uScores.Add(new Tuple<string, double>("persuade",
                                                UtilityHardPersuade(
                                                            skills["charisma"],
                                                            personality["friendliness"],
                                                            personality["confidence"]
                                                            )));
        uScores.Add(new Tuple<string, double>("intimidate",
                                                UtilityHardIntimidate(
                                                            skills["combat"],
                                                            personality["friendliness"],
                                                            personality["confidence"]
                                                            )));

        uScores.Sort((x, y) => y.Item2.CompareTo(x.Item2));

        return uScores;

    }

    // Score all actions for neutralise and sort them by value
    public static List<Tuple<string, double>> ScoreNeutralize(  Dictionary<string, int> personality,
                                                                Dictionary<string, int> skills,
                                                                double allegianceToTarget)
    {
        List<Tuple<string, double>> uScores = ScoreConvince(personality, skills);
        uScores.Add(new Tuple<string, double>("fight",
                                                UtilityFight(
                                                            skills["combat"],
                                                            allegianceToTarget,
                                                            personality["friendliness"],
                                                            personality["confidence"]
                                                            )));
        uScores.Add(new Tuple<string, double>("assassinate",
                                                UtilityAssassinate(
                                                            skills["combat"],
                                                            skills["stealth"],
                                                            allegianceToTarget,
                                                            personality["friendliness"],
                                                            personality["confidence"]
                                                            )));

        uScores.Sort((x, y) => y.Item2.CompareTo(x.Item2));

        return uScores;

    }

    // Score all actions for develop and sort them by value
    public static List<Tuple<string, double>> ScoreDevelop(   Dictionary<string, int> personality,
                                                                Dictionary<string, int> skills)
    {
        List<Tuple<string, double>> uScores = new List<Tuple<string, double>>();
        uScores.Add(new Tuple<string, double>("build",
                                                UtilityHardCraft(
                                                            skills["crafts"],
                                                            personality["confidence"]
                                                            )));
        uScores.Add(new Tuple<string, double>("outsource",
                                                UtilityHardBuy(
                                                            skills["wealth"],
                                                            personality["carelessness"]
                                                            )));
        uScores.Add(new Tuple<string, double>("persuade",
                                                UtilityHardPersuade(
                                                            skills["charisma"],
                                                            personality["friendliness"],
                                                            personality["confidence"]
                                                            )));
        uScores.Add(new Tuple<string, double>("intimidate",
                                                UtilityHardIntimidate(
                                                            skills["combat"],
                                                            personality["friendliness"],
                                                            personality["confidence"]
                                                            )));

        uScores.Sort((x, y) => y.Item2.CompareTo(x.Item2));

        return uScores;

    }



    // Given confidence, return the m in y=mx to calculate perceived skill (y) from real skill (x)
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

    // Calculate perceived skill from real skill and confidence
    private static double PerceivedSkill(double realSkill, double confidence)
    {
        return SlopeGivenConfidence(confidence) * realSkill;
    }

    // Utility for steal action
    // Sigmoid shape
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

    // Utility for buy action
    // Sine shape
    private static double UtilityBuy(double wealth, double carelessness)
    {
        double multiplier = 0.4;
        double padding = 1.5;
        double xMod = 0.015708;

        return multiplier * wealth * (padding + Math.Sin(xMod * carelessness));
    }

    // Utility for steal action
    // Sigmoid shape
    private static double UtilityPersuade(double charisma, double friendliness, double confidence)
    {
        double c = PerceivedSkill(charisma, confidence);

        double vertOffset = -100.0;
        double sigmoidSize = 200.0;
        double xMod = 0.03;
        double frDivisor = 200;

        // friendliness adds to the utility score
        return vertOffset + sigmoidSize / (1 + Math.Exp(-xMod * c - friendliness / frDivisor));
    }

    // Utility for steal action
    // Sigmoid shape
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

    // Utility for craft action
    // Exponential shape
    private static double UtilityCraft(double crafts, double confidence)
    {
        double c = PerceivedSkill(crafts, confidence);

        double xMod = 0.01;

        return xMod * Math.Pow(c, 2);

    }


    // Utility for fight action
    // Sigmoid shape
    private static double UtilityFight(double combat, double allegiance, double confidence, double friendliness)
    {
        double c = PerceivedSkill(combat, confidence);
        double sigmoidHardness = 0.016;

        return -friendliness / 5 + 1.5 * allegiance + -3 * allegiance / (1 + Math.Exp(-sigmoidHardness * c));
    }

    // Utility for assassinate action
    // Sigmoid shape
    private static double UtilityAssassinate(double combat, double stealth, double allegiance, double confidence, double friendliness)
    {
        double c = PerceivedSkill(combat, confidence);
        double s = PerceivedSkill(stealth, confidence);
        double sigmoidHardness = 0.016;

        return -friendliness / 2 + 1.5 * allegiance + -3 * allegiance / (1 + Math.Exp(-sigmoidHardness * Math.Pow(s, 0.5) * Math.Pow(c, 0.333)));

    }

    // Utility for buy action, modified to be less preferred
    // Sine shape
    private static double UtilityHardBuy(double wealth, double carelessness)
    {
        double multiplier = 0.4;
        double padding = 1.1;
        double xMod = 0.015708;

        return multiplier * wealth * (padding + Math.Sin(xMod * carelessness));
    }

    // Utility for persuade action, modified to be less preferred
    // sigmoid shape
    private static double UtilityHardPersuade(double charisma, double friendliness, double confidence)
    {
        double c = PerceivedSkill(charisma, confidence);

        double vertOffset = -100.0;
        double sigmoidSize = 200.0;
        double xMod = 0.02;
        double frDivisor = 200;

        // friendliness adds to the utility score
        return vertOffset + sigmoidSize / (1 + Math.Exp(-xMod * c - friendliness / frDivisor));
    }

    // Utility for intimidate action, modified to be less preferred
    // sigmoid shape
    private static double UtilityHardIntimidate(double combat, double friendliness, double confidence)
    {
        double c = PerceivedSkill(combat, confidence);

        double vertOffset = -101.0;
        double sigmoidSize = 200.0;
        double xMod = 0.02;
        double frDivisor = 200;

        // friendliness subtracts from the utility score
        return vertOffset + sigmoidSize / (1 + Math.Exp(-xMod * c + friendliness / frDivisor));
    }

    // Utility for craft action, modified to be less preferred
    // exmponential shape
    private static double UtilityHardCraft(double crafts, double confidence)
    {
        double c = PerceivedSkill(crafts, confidence);

        double xMod = 0.009;
        double diff = 5;

        return xMod * Math.Pow(c, 2) - diff;

    }


}
