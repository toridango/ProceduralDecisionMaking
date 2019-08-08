
using System.Collections.Generic;
using System;
using System.IO;

public class BehaviourAnalytics
{
    static BehaviourAnalytics()
    {
    }

    public static void PerformAnalytics()
    {
        // simulated NPCs       
        //Dictionary<string, int> m_personality;
        //Dictionary<string, int> m_skills;
        //Dictionary<string, int> m_allegiances;

        Dictionary<string, Dictionary<string, Dictionary<string, int>>> testNPCs = MakeTestNPCs();



        // Goals

        List<string> goals = new List<string>()
        {
            "a_gi_pinkoin_close", // close
            "a_gi_pinkoin_far", // far
            "a_gi_sword", // cheap-ish
            "a_gi_wizardstaff", // expensive
            "a_co_paladincork",
            "a_co_sneekibreeki",
            "a_nu_paladincork",
            "a_nu_sneekibreeki",
            "a_dv_enchant", // expensive
            "a_dv_house", // very expensive
            "a_dv_nails" // cheap
        };


        // Rank actions for goals for all NPCs

        Dictionary<string, Dictionary<string, List<string>>> generatedActionRanks = new Dictionary<string, Dictionary<string, List<string>>>();

        foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, int>>> npc in testNPCs)
        {
            generatedActionRanks.Add(npc.Key, new Dictionary<string, List<string>>());
            for (int i = 0; i < goals.Count; ++i)
            {
                switch (goals[i])
                {
                    case "a_gi_pinkoin_close":
                        {
                            List<string> actions = new List<string>();
                            // personality, skills, utility for find, craftable
                            List<Tuple<string, double>> us = UtilityScoring.ScoreGetItem(
                                                            npc.Value["personality"],
                                                            npc.Value["skills"],
                                                            98.0,
                                                            false
                                                            );

                            foreach (Tuple<string, double> t in us)
                            {
                                actions.Add(t.Item1);
                            }
                            generatedActionRanks[npc.Key].Add(goals[i], actions);
                            break;
                        }
                    case "a_gi_pinkoin_far":
                        {
                            List<string> actions = new List<string>();
                            // personality, skills, utility for find, craftable
                            List<Tuple<string, double>> us = UtilityScoring.ScoreGetItem(
                                                            npc.Value["personality"],
                                                            npc.Value["skills"],
                                                            10.0,
                                                            false
                                                            );
                            foreach (Tuple<string, double> t in us)
                            {
                                actions.Add(t.Item1);
                            }
                            generatedActionRanks[npc.Key].Add(goals[i], actions);
                            break;
                        }
                    case "a_gi_sword":
                        {
                            List<string> actions = new List<string>();
                            // personality, skills, utility for find, craftable
                            List<Tuple<string, double>> us = UtilityScoring.ScoreGetItem(
                                                            npc.Value["personality"],
                                                            npc.Value["skills"],
                                                            -1.0,
                                                            true
                                                            );

                            foreach (Tuple<string, double> t in us)
                            {
                                actions.Add(t.Item1);
                            }
                            generatedActionRanks[npc.Key].Add(goals[i], actions);
                            break;
                        }
                    case "a_gi_wizardstaff":
                        {
                            List<string> actions = new List<string>();
                            // personality, skills, utility for find, craftable
                            List<Tuple<string, double>> us = UtilityScoring.ScoreGetItem(
                                                            npc.Value["personality"],
                                                            npc.Value["skills"],
                                                            -1.0,
                                                            true
                                                            );

                            foreach (Tuple<string, double> t in us)
                            {
                                actions.Add(t.Item1);
                            }
                            generatedActionRanks[npc.Key].Add(goals[i], actions);
                            break;
                        }
                    case "a_co_paladincork":
                        {
                            List<string> actions = new List<string>();
                            // personality, skills, utility for find, craftable
                            List<Tuple<string, double>> us = UtilityScoring.ScoreConvince(
                                                            npc.Value["personality"],
                                                            npc.Value["skills"]
                                                            );

                            foreach (Tuple<string, double> t in us)
                            {
                                actions.Add(t.Item1);
                            }
                            generatedActionRanks[npc.Key].Add(goals[i], actions);
                            break;
                        }
                    case "a_co_sneekibreeki":
                        {
                            List<string> actions = new List<string>();
                            // personality, skills, utility for find, craftable
                            List<Tuple<string, double>> us = UtilityScoring.ScoreConvince(
                                                            npc.Value["personality"],
                                                            npc.Value["skills"]
                                                            );

                            foreach (Tuple<string, double> t in us)
                            {
                                actions.Add(t.Item1);
                            }
                            generatedActionRanks[npc.Key].Add(goals[i], actions);
                            break;
                        }
                    case "a_nu_paladincork":
                        {
                            List<string> actions = new List<string>();
                            // personality, skills, utility for find, craftable
                            List<Tuple<string, double>> us = UtilityScoring.ScoreNeutralize(
                                                            npc.Value["personality"],
                                                            npc.Value["skills"],
                                                            npc.Value["allegiances"]["npc_paladincork"]
                                                            );

                            foreach (Tuple<string, double> t in us)
                            {
                                actions.Add(t.Item1);
                            }
                            generatedActionRanks[npc.Key].Add(goals[i], actions);
                            break;
                        }
                    case "a_nu_sneekibreeki":
                        {
                            List<string> actions = new List<string>();
                            // personality, skills, utility for find, craftable
                            List<Tuple<string, double>> us = UtilityScoring.ScoreNeutralize(
                                                            npc.Value["personality"],
                                                            npc.Value["skills"],
                                                            npc.Value["allegiances"]["npc_sneekibreeki"]
                                                            );

                            foreach (Tuple<string, double> t in us)
                            {
                                actions.Add(t.Item1);
                            }
                            generatedActionRanks[npc.Key].Add(goals[i], actions);
                            break;
                        }
                    case "a_dv_enchant":
                        {
                            List<string> actions = new List<string>();
                            // personality, skills, utility for find, craftable
                            List<Tuple<string, double>> us = UtilityScoring.ScoreDevelop(
                                                            npc.Value["personality"],
                                                            npc.Value["skills"]
                                                            );

                            foreach (Tuple<string, double> t in us)
                            {
                                actions.Add(t.Item1);
                            }
                            generatedActionRanks[npc.Key].Add(goals[i], actions);
                            break;
                        }
                    case "a_dv_house":
                        {
                            List<string> actions = new List<string>();
                            // personality, skills, utility for find, craftable
                            List<Tuple<string, double>> us = UtilityScoring.ScoreDevelop(
                                                            npc.Value["personality"],
                                                            npc.Value["skills"]
                                                            );

                            foreach (Tuple<string, double> t in us)
                            {
                                actions.Add(t.Item1);
                            }
                            generatedActionRanks[npc.Key].Add(goals[i], actions);
                            break;
                        }
                    case "a_dv_nails":
                        {
                            List<string> actions = new List<string>();
                            // personality, skills, utility for find, craftable
                            List<Tuple<string, double>> us = UtilityScoring.ScoreDevelop(
                                                            npc.Value["personality"],
                                                            npc.Value["skills"]
                                                            );

                            foreach (Tuple<string, double> t in us)
                            {
                                actions.Add(t.Item1);
                            }
                            generatedActionRanks[npc.Key].Add(goals[i], actions);
                            break;
                        }
                    default:
                        break;
                }
            }
        }


        // scripted rankings
        // NPC, Goal, Actions
        Dictionary<string, Dictionary<string, List<string>>> scriptedActionRanks = GetScriptedActionRankings();

        
        string data = "NPC\t\t\t" + String.Format("{0,-20}\t\t{1,-20}\t\t{2,-20}\t\t{3,-20}", "Goal", "HarmonicDWD", "GeometricCWD", "LevenshteinDistance") + "\n";
        string actionData = "NPC\t\t\t\t" + String.Format("{0,-30}\t\t{1,-40}\t\t{2,-40}", "Goal", "Scripted", "Generated") + "\n";
        foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, int>>> npc in testNPCs)
        {
            data += npc.Key + "\n";
            actionData += npc.Key + "\n";
            for (int i = 0; i < goals.Count; ++i)
            {


                // Evaluate and add to log data
                //data += "\t\t\t" + goals[i] + " \t\t " + Evaluate(generatedActionRanks[npc.Key][goals[i]], 
                //                                                    scriptedActionRanks[npc.Key][goals[i]]) + "\n";
                data += "\t\t\t" + String.Format("{0,-20}\t\t{1,-20}",
                                                    goals[i],
                                                    Evaluate(scriptedActionRanks[npc.Key][goals[i]],
                                                            generatedActionRanks[npc.Key][goals[i]])) + "\n";

                // TODO python-like Join of elements in lists
                actionData += "\t\t\t\t" + String.Format("{0,-30}\t\t{1,-40}\t\t{2,-40}",
                            goals[i],
                            string.Join<string>(", " , scriptedActionRanks[npc.Key][goals[i]]),
                            string.Join<string>(", ", generatedActionRanks[npc.Key][goals[i]])) + "\n";
            }
        }
        WriteDataToTextFile("./AnalyticsLogs", "Evaluation_", data);
        WriteDataToTextFile("./AnalyticsLogs", "ActionLists_", actionData);
    }


    // Difference between A and B
    // weight: 1/n for the nth element (1, 1/2, 1/3...)
    // e.g. "abcde" vs "abfgh" is not even 1
    // e.g. 2 completely disparate 13-character strings sum up to 3.18 aprox (a to m vs n to z)
    private static double HarmonicDivergentWeightedDifference(List<string> scriptedRanking, List<string> generatedRanking)
    {
        double score = 0.0;
        int x = 1;

        int l1 = generatedRanking.Count;
        int l2 = scriptedRanking.Count;

        for (int i = 0; i < l1; ++i)
        {
            double div = (i + x);
            if (i < l2)
            {
                score += generatedRanking[i] == scriptedRanking[i] ? 0 : 1.0 / div;
            }
            else
            {
                score += 1.0 / div;
            }
        }
        if (l2 > l1)
        {
            for (int i = l1; i < l2; ++i)
            {
                double div = (i + x);
                score += 1.0 / div;
            }
        }


        return score;
    }

    // Difference between A and B
    // weight: 1/n^2 for the nth element (1/2, 1/4, 1/8...) 
    // SUm ranges from 0 to 1 (1 when n tends to infinity)
    // e.g. "abcde" vs "abfgh" is 0.13 aprox
    // e.g. "abcde" vs "fbcde" is 0.25
    // e.g. 2 completely disparate 13-character strings sum up to 0.576 aprox (a to m vs n to z)
    private static double GeometricConvergentDifference(List<string> scriptedRanking, List<string> generatedRanking)
    {
        double score = 0.0;
        int x = 2;

        int l1 = generatedRanking.Count;
        int l2 = scriptedRanking.Count;

        for (int i = 0; i < l1; ++i)
        {
            double div = (i + x) * (i + x);
            if (i < l2)
            {
                score += generatedRanking[i] == scriptedRanking[i] ? 0 : 1.0 / div;
            }
            else
            {
                score += 1.0 / div;
            }
        }
        if (l2 > l1)
        {
            for (int i = l1; i < l2; ++i)
            {
                double div = (i + x) * (i + x);
                score += 1.0 / div;
            }
        }


        return score;
    }

    // Levenshtein Distance (recursive inefficient method):
    // String metric for measuring the difference between two sequences
    // Does not account for order unlike my custom metric
    // (Adapted for lists of strings)
    private static int LevenshteinDistance(List<string> s, int len_s, List<string> t, int len_t)
    {
        int cost;

        /* base case: empty strings */
        if (len_s == 0) return len_t;
        if (len_t == 0) return len_s;

        /* test if last characters of the strings match */
        if (s[len_s - 1] == t[len_t - 1])
            cost = 0;
        else
            cost = 1;

        /* return minimum of delete char from s, delete char from t, and delete char from both */
        return Math.Min(LevenshteinDistance(s, len_s - 1, t, len_t) + 1,
               Math.Min(LevenshteinDistance(s, len_s, t, len_t - 1) + 1,
                       LevenshteinDistance(s, len_s - 1, t, len_t - 1) + cost));
    }

    // Probably return string
    // Evaluates the generated course of action against the expected one
    // Independently of eventual success or failure
    public static string Evaluate(List<string> scriptedRanking, List<string> generatedRanking)
    {
        string data = "";
               
        data += String.Format("{0,-20}\t\t", HarmonicDivergentWeightedDifference(scriptedRanking, generatedRanking).ToString());
        data += String.Format("{0,-20}\t\t", GeometricConvergentDifference(scriptedRanking, generatedRanking).ToString());
        data += String.Format("{0,-20}\t\t", LevenshteinDistance(scriptedRanking, scriptedRanking.Count,
                                                                    generatedRanking, generatedRanking.Count).ToString());

        return data;
    }


    private static void WriteDataToTextFile(string path, string fileprefix, string data)
    {
        string filename = Path.Combine(path, fileprefix + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt");
        File.WriteAllText(filename, data);

    }



    private static Dictionary<string, Dictionary<string, Dictionary<string, int>>> MakeTestNPCs()
    {
        Dictionary<string, Dictionary<string, Dictionary<string, int>>> testNPCs = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>
        {
            {
                "HostileThief",
                new Dictionary<string, Dictionary<string, int>>()
                {
                    { "personality", new Dictionary<string, int>()
                        {
                            {"caution",         10},
                            {"carelessness",    -10},
                            {"conscience",      -50},
                            {"friendliness",    -30},
                            {"confidence",      20}
                        }
                    },
                    { "skills", new Dictionary<string, int>()
                        {
                            {"pickpocket",  60},
                            {"stealth",     60},
                            {"charisma",    10},
                            {"crafts",      0},
                            {"combat",      20},
                            {"wealth",      20}
                        }
                    },
                    { "allegiances", new Dictionary<string, int>()
                        {
                            {"npc_paladincork",     -80},
                            {"npc_sneekibreeki",    -20}
                        }
                    }
                }
            },
            {
                "CharmingThief",
                new Dictionary<string, Dictionary<string, int>>()
                {
                    { "personality", new Dictionary<string, int>()
                        {
                            {"caution",         20},
                            {"carelessness",    -20},
                            {"conscience",      -40},
                            {"friendliness",    30},
                            {"confidence",      40}
                        }
                    },
                    { "skills", new Dictionary<string, int>()
                        {
                            {"pickpocket",  60},
                            {"stealth",     60},
                            {"charisma",    40},
                            {"crafts",      0},
                            {"combat",      0},
                            {"wealth",      40}
                        }
                    },
                    { "allegiances", new Dictionary<string, int>()
                        {
                            {"npc_paladincork",     -30},
                            {"npc_sneekibreeki",    20}
                        }
                    }
                }
            },
            {
                "Barbarian",
                new Dictionary<string, Dictionary<string, int>>()
                {
                    { "personality", new Dictionary<string, int>()
                        {
                            {"caution",         -20},
                            {"carelessness",    50},
                            {"conscience",      -30},
                            {"friendliness",    -20},
                            {"confidence",      50}
                        }
                    },
                    { "skills", new Dictionary<string, int>()
                        {
                            {"pickpocket",  0},
                            {"stealth",     0},
                            {"charisma",    10},
                            {"crafts",      0},
                            {"combat",      80},
                            {"wealth",      10}
                        }
                    },
                    { "allegiances", new Dictionary<string, int>()
                        {
                            {"npc_paladincork",     -20},
                            {"npc_sneekibreeki",    -10}
                        }
                    }
                }
            },
            {
                "Paladin",
                new Dictionary<string, Dictionary<string, int>>()
                {
                    { "personality", new Dictionary<string, int>()
                        {
                            {"caution",         20},
                            {"carelessness",    -30},
                            {"conscience",      50},
                            {"friendliness",    20},
                            {"confidence",      40}
                        }
                    },
                    { "skills", new Dictionary<string, int>()
                        {
                            {"pickpocket",  0},
                            {"stealth",     0},
                            {"charisma",    30},
                            {"crafts",      0},
                            {"combat",      80},
                            {"wealth",      50}
                        }
                    },
                    { "allegiances", new Dictionary<string, int>()
                        {
                            {"npc_paladincork",     50},
                            {"npc_sneekibreeki",    -40}
                        }
                    }
                }
            },
            {
                "VirtuousRogue",
                new Dictionary<string, Dictionary<string, int>>()
                {
                    { "personality", new Dictionary<string, int>()
                        {
                            {"caution",         30},
                            {"carelessness",    -40},
                            {"conscience",      -10},
                            {"friendliness",    20},
                            {"confidence",      50}
                        }
                    },
                    { "skills", new Dictionary<string, int>()
                        {
                            {"pickpocket",  20},
                            {"stealth",     50},
                            {"charisma",    30},
                            {"crafts",      20},
                            {"combat",      60},
                            {"wealth",      30}
                        }
                    },
                    { "allegiances", new Dictionary<string, int>()
                        {
                            {"npc_paladincork",     -20},
                            {"npc_sneekibreeki",    10}
                        }
                    }
                }
            },
            {
                "Assassin",
                new Dictionary<string, Dictionary<string, int>>()
                {
                    { "personality", new Dictionary<string, int>()
                        {
                            {"caution",         20},
                            {"carelessness",    20},
                            {"conscience",      -60},
                            {"friendliness",    -30},
                            {"confidence",      60}
                        }
                    },
                    { "skills", new Dictionary<string, int>()
                        {
                            {"pickpocket",  10},
                            {"stealth",     60},
                            {"charisma",    20},
                            {"crafts",      0},
                            {"combat",      60},
                            {"wealth",      50}
                        }
                    },
                    { "allegiances", new Dictionary<string, int>()
                        {
                            {"npc_paladincork",     -50},
                            {"npc_sneekibreeki",    0}
                        }
                    }
                }
            },
            {
                "RudeMerchant",
                new Dictionary<string, Dictionary<string, int>>()
                {
                    { "personality", new Dictionary<string, int>()
                        {
                            {"caution",         30},
                            {"carelessness",    -60},
                            {"conscience",      -20},
                            {"friendliness",    -40},
                            {"confidence",      10}
                        }
                    },
                    { "skills", new Dictionary<string, int>()
                        {
                            {"pickpocket",  0},
                            {"stealth",     0},
                            {"charisma",    30},
                            {"crafts",      50},
                            {"combat",      0},
                            {"wealth",      80}
                        }
                    },
                    { "allegiances", new Dictionary<string, int>()
                        {
                            {"npc_paladincork",     20},
                            {"npc_sneekibreeki",    -40}
                        }
                    }
                }
            },
            {
                "NiceMerchant",
                new Dictionary<string, Dictionary<string, int>>()
                {
                    { "personality", new Dictionary<string, int>()
                        {
                            {"caution",         -10},
                            {"carelessness",    -40},
                            {"conscience",      20},
                            {"friendliness",    0},
                            {"confidence",      0}
                        }
                    },
                    { "skills", new Dictionary<string, int>()
                        {
                            {"pickpocket",  0},
                            {"stealth",     0},
                            {"charisma",    50},
                            {"crafts",      50},
                            {"combat",      0},
                            {"wealth",      80}
                        }
                    },
                    { "allegiances", new Dictionary<string, int>()
                        {
                            {"npc_paladincork",     30},
                            {"npc_sneekibreeki",    -20}
                        }
                    }
                }
            }
        };




        return testNPCs;
    }


    private static Dictionary<string, Dictionary<string, List<string>>> GetScriptedActionRankings()
    {
        //List<string> actionRanking = new List<string>();

        Dictionary<string, Dictionary<string, List<string>>> scriptedActionRanks = new Dictionary<string, Dictionary<string, List<string>>>()
        {
            {
                "HostileThief",
                new Dictionary<string, List<string>>()
                {
                    {
                        "a_gi_pinkoin_close",
                        new List<string>(){ "find", "steal", "intimidate", "persuade" }
                    },
                    {
                        "a_gi_pinkoin_far",
                        new List<string>(){ "steal", "intimidate", "persuade", "find" }
                    },
                    {
                        "a_gi_sword",
                        new List<string>(){ "steal", "intimidate", "persuade" }
                    },
                    {
                        "a_gi_wizardstaff",
                        new List<string>(){ "steal", "intimidate", "persuade" }
                    },
                    {
                        "a_co_paladincork",
                        new List<string>(){ "intimidate", "persuade" }
                    },
                    {
                        "a_co_sneekibreeki",
                        new List<string>(){ "intimidate", "persuade" }
                    },
                    {
                        "a_nu_paladincork",
                        new List<string>(){ "intimidate", "persuade", "assassinate", "fight" }
                    },
                    {
                        "a_nu_sneekibreeki",
                        new List<string>(){ "intimidate", "persuade", "assassinate", "fight" }
                    },
                    {
                        "a_dv_enchant",
                        new List<string>(){ "intimidate", "persuade", "outsource" }
                    },
                    {
                        "a_dv_house",
                        new List<string>(){ "intimidate", "persuade", "outsource" }
                    },
                    {
                        "a_dv_nails",
                        new List<string>(){ "intimidate", "persuade", "outsource" }
                    }
                }
            },
            {
                "CharmingThief",new Dictionary<string, List<string>>()
                {
                    {
                        "a_gi_pinkoin_close",
                        new List<string>(){ "find", "steal", "persuade", "intimidate" }
                    },
                    {
                        "a_gi_pinkoin_far",
                        new List<string>(){ "steal", "persuade", "intimidate", "find" }
                    },
                    {
                        "a_gi_sword",
                        new List<string>(){ "steal", "persuade", "intimidate" }
                    },
                    {
                        "a_gi_wizardstaff",
                        new List<string>(){ "steal", "persuade", "intimidate" }
                    },
                    {
                        "a_co_paladincork",
                        new List<string>(){ "persuade", "intimidate" }
                    },
                    {
                        "a_co_sneekibreeki",
                        new List<string>(){ "persuade", "intimidate" }
                    },
                    {
                        "a_nu_paladincork",
                        new List<string>(){ "persuade", "intimidate" }
                    },
                    {
                        "a_nu_sneekibreeki",
                        new List<string>(){ "persuade", "intimidate" }
                    },
                    {
                        "a_dv_enchant",
                        new List<string>(){ "persuade", "intimidate", "outsource" }
                    },
                    {
                        "a_dv_house",
                        new List<string>(){ "persuade", "intimidate", "outsource" }
                    },
                    {
                        "a_dv_nails",
                        new List<string>(){ "persuade", "intimidate", "outsource" }
                    }
                }
            },
            {
                "Barbarian",new Dictionary<string, List<string>>()
                {
                    {
                        "a_gi_pinkoin_close",
                        new List<string>(){ "find", "intimidate", "steal" }
                    },
                    {
                        "a_gi_pinkoin_far",
                        new List<string>(){ "intimidate", "steal", "find" }
                    },
                    {
                        "a_gi_sword",
                        new List<string>(){ "intimidate", "steal" }
                    },
                    {
                        "a_gi_wizardstaff",
                        new List<string>(){ "intimidate", "steal" }
                    },
                    {
                        "a_co_paladincork",
                        new List<string>(){ "intimidate" }
                    },
                    {
                        "a_co_sneekibreeki",
                        new List<string>(){ "intimidate"  }
                    },
                    {
                        "a_nu_paladincork",
                        new List<string>(){ "fight", "assassinate", "intimidate" }
                    },
                    {
                        "a_nu_sneekibreeki",
                        new List<string>(){ "fight", "assassinate", "intimidate" }
                    },
                    {
                        "a_dv_enchant",
                        new List<string>(){ "intimidate" }
                    },
                    {
                        "a_dv_house",
                        new List<string>(){ "intimidate" }
                    },
                    {
                        "a_dv_nails",
                        new List<string>(){ "intimidate" }
                    }
                }
            },
            {
                "Paladin",new Dictionary<string, List<string>>()
                {
                    {
                        "a_gi_pinkoin_close",
                        new List<string>(){ "find", "buy", "persuade", "intimidate" }
                    },
                    {
                        "a_gi_pinkoin_far",
                        new List<string>(){ "buy", "intimidate", "persuade", "find" }
                    },
                    {
                        "a_gi_sword",
                        new List<string>(){ "buy", "intimidate", "persuade" }
                    },
                    {
                        "a_gi_wizardstaff",
                        new List<string>(){ "buy", "intimidate", "persuade" }
                    },
                    {
                        "a_co_paladincork",
                        new List<string>(){ "persuade", "intimidate", "bribe" }
                    },
                    {
                        "a_co_sneekibreeki",
                        new List<string>(){ "intimidate", "persuade", "bribe" }
                    },
                    {
                        "a_nu_paladincork",
                        new List<string>(){ "intimidate", "persuade", "fight" }
                    },
                    {
                        "a_nu_sneekibreeki",
                        new List<string>(){ "intimidate", "persuade", "fight" }
                    },
                    {
                        "a_dv_enchant",
                        new List<string>(){ "outsource", "intimidate", "persuade" }
                    },
                    {
                        "a_dv_house",
                        new List<string>(){ "outsource", "intimidate", "persuade" }
                    },
                    {
                        "a_dv_nails",
                        new List<string>(){ "outsource", "intimidate", "persuade" }
                    }
                }
            },
            {
                "VirtuousRogue",new Dictionary<string, List<string>>()
                {
                    {
                        "a_gi_pinkoin_close",
                        new List<string>(){ "find", "persuade", "intimidate", "buy" }
                    },
                    {
                        "a_gi_pinkoin_far",
                        new List<string>(){ "persuade", "intimidate", "buy", "find" }
                    },
                    {
                        "a_gi_sword",
                        new List<string>(){ "persuade", "intimidate", "buy" }
                    },
                    {
                        "a_gi_wizardstaff",
                        new List<string>(){ "persuade", "intimidate", "buy" }
                    },
                    {
                        "a_co_paladincork",
                        new List<string>(){ "persuade", "intimidate", "bribe" }
                    },
                    {
                        "a_co_sneekibreeki",
                        new List<string>(){ "persuade", "intimidate", "bribe" }
                    },
                    {
                        "a_nu_paladincork",
                        new List<string>(){ "persuade", "assassinate", "intimidate", "fight" }
                    },
                    {
                        "a_nu_sneekibreeki",
                        new List<string>(){ "persuade", "intimidate", "assassinate", "fight" }
                    },
                    {
                        "a_dv_enchant",
                        new List<string>(){ "persuade", "intimidate", "outsource" }
                    },
                    {
                        "a_dv_house",
                        new List<string>(){ "persuade", "intimidate", "outsource" }
                    },
                    {
                        "a_dv_nails",
                        new List<string>(){ "persuade", "intimidate", "outsource" }
                    }
                }
            },
            {
                "Assassin",new Dictionary<string, List<string>>()
                {
                    {
                        "a_gi_pinkoin_close",
                        new List<string>(){ "find", "intimidate", "buy", "persuade" }
                    },
                    {
                        "a_gi_pinkoin_far",
                        new List<string>(){ "intimidate", "buy", "persuade", "find" }
                    },
                    {
                        "a_gi_sword",
                        new List<string>(){ "intimidate", "buy", "persuade" }
                    },
                    {
                        "a_gi_wizardstaff",
                        new List<string>(){ "intimidate", "buy", "persuade" }
                    },
                    {
                        "a_co_paladincork",
                        new List<string>(){ "intimidate", "bribe", "persuade" }
                    },
                    {
                        "a_co_sneekibreeki",
                        new List<string>(){ "intimidate", "bribe", "persuade" }
                    },
                    {
                        "a_nu_paladincork",
                        new List<string>(){ "assassinate", "intimidate", "fight" }
                    },
                    {
                        "a_nu_sneekibreeki",
                        new List<string>(){ "assassinate", "intimidate", "fight" }
                    },
                    {
                        "a_dv_enchant",
                        new List<string>(){ "intimidate", "outsource", "persuade" }
                    },
                    {
                        "a_dv_house",
                        new List<string>(){ "intimidate", "outsource", "persuade" }
                    },
                    {
                        "a_dv_nails",
                        new List<string>(){ "intimidate", "outsource", "persuade" }
                    }
                }
            },
            {
                "RudeMerchant",new Dictionary<string, List<string>>()
                {
                    {
                        "a_gi_pinkoin_close",
                        new List<string>(){ "find", "buy", "persuade", "intimidate" }
                    },
                    {
                        "a_gi_pinkoin_far",
                        new List<string>(){ "persuade", "buy", "intimidate", "find" }
                    },
                    {
                        "a_gi_sword",
                        new List<string>(){ "persuade", "buy", "intimidate" }
                    },
                    {
                        "a_gi_wizardstaff",
                        new List<string>(){ "persuade", "buy", "intimidate" }
                    },
                    {
                        "a_co_paladincork",
                        new List<string>(){ "persuade", "bribe", "intimidate" }
                    },
                    {
                        "a_co_sneekibreeki",
                        new List<string>(){ "persuade", "bribe", "intimidate" }
                    },
                    {
                        "a_nu_paladincork",
                        new List<string>(){ "persuade", "bribe", "intimidate" }
                    },
                    {
                        "a_nu_sneekibreeki",
                        new List<string>(){ "persuade", "bribe", "intimidate" }
                    },
                    {
                        "a_dv_enchant",
                        new List<string>(){ "persuade", "outsource", "intimidate" }
                    },
                    {
                        "a_dv_house",
                        new List<string>(){ "persuade", "outsource", "intimidate" }
                    },
                    {
                        "a_dv_nails",
                        new List<string>(){ "persuade", "outsource", "intimidate" }
                    }
                }
            },
            {
                "NiceMerchant",new Dictionary<string, List<string>>()
                {
                    {
                        "a_gi_pinkoin_close",
                        new List<string>(){ "find", "persuade", "buy", "intimidate" }
                    },
                    {
                        "a_gi_pinkoin_far",
                        new List<string>(){ "persuade", "buy", "intimidate", "find" }
                    },
                    {
                        "a_gi_sword",
                        new List<string>(){ "persuade", "buy", "intimidate" }
                    },
                    {
                        "a_gi_wizardstaff",
                        new List<string>(){ "persuade", "buy", "intimidate" }
                    },
                    {
                        "a_co_paladincork",
                        new List<string>(){ "persuade", "bribe", "intimidate" }
                    },
                    {
                        "a_co_sneekibreeki",
                        new List<string>(){ "persuade", "bribe", "intimidate" }
                    },
                    {
                        "a_nu_paladincork",
                        new List<string>(){ "persuade", "bribe", "intimidate" }
                    },
                    {
                        "a_nu_sneekibreeki",
                        new List<string>(){ "persuade", "bribe", "intimidate" }
                    },
                    {
                        "a_dv_enchant",
                        new List<string>(){ "persuade", "outsource", "intimidate" }
                    },
                    {
                        "a_dv_house",
                        new List<string>(){ "persuade", "outsource", "intimidate" }
                    },
                    {
                        "a_dv_nails",
                        new List<string>(){ "persuade", "outsource", "intimidate" }
                    }
                }
            }
        };

        return scriptedActionRanks;
    }


}
