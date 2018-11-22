﻿using System;
using System.Collections;
using System.Globalization;
using DSharpPlus.CommandsNext;

/**************************************************************************
**                                Ryoshi                                 **
**************************************************************************/

namespace Ryoshi
{
    public class Schedule
    {
        public static int BossCountdown = 0;

        public static bool GarmothUp = false;
        public static bool KzarkaUp = false;
        public static bool KarandaUp = false;
        public static bool NouverUp = false;
        public static bool IsabellaUp = false;
        public static bool OffinUp = false;
        public static bool KutumUp = false;
        public static bool MurakaUp = false;
        public static bool QuintUp = false;
        public static bool VellUp = false;
        public static bool FireworksUp = false;
        public static bool TargargoUp = false;
        public static bool BonusUp = false;

        public static ArrayList FieldBossNames = new ArrayList();
        public static ArrayList FieldBossServers = new ArrayList();
        public static ArrayList FieldBossCountdowns = new ArrayList();

        public static ArrayList BellServers = new ArrayList();
        public static ArrayList BellDurations = new ArrayList();

        public static int DayCode(string day)
        {
            day = day.ToLower();

            if (day == "sunday")
            {
                return 0;
            }
            else if (day == "monday")
            {
                return 1;

            }
            else if (day == "tuesday")
            {
                return 2;
            }
            else if (day == "wednesday")
            {
                return 3;
            }
            else if (day == "thursday")
            {
                return 4;
            }
            else if (day == "friday")
            {
                return 5;
            }
            else if (day == "saturday")
            {
                return 6;
            } else
            {
                return -1;
            }
        }

        public static string GenerateSchedule()
        {
            /*
             * These arrays MUST correspond in a one-to-one manner.
             */

            string[] unsortedTimes =
            {
                ETA(Garmoth),
                //ETA(Isabella),
                ETA(Karanda),
                ETA(Kutum),
                ETA(Kzarka),
                ETA(Muraka),
                ETA(Nouver),
                ETA(Offin),
                ETA(Quint),
                ETA(Vell),
                ETA(Fireworks),
                ETA(Targargo),
                ETA(BonusXP)
            };

            string[] unsortedNames =
            {
                "Garmoth",
                //"Isabella",
                "Karanda",
                "Kutum",
                "Kzarka",
                "Muraka",
                "Nouver",
                "Offin",
                "Quint",
                "Vell",
                "Fireworks",
                "Targargo",
                "BonusXP"
            };
            
            return SortedSchedule(unsortedTimes, unsortedNames);
        }

        public static string BossMessage(string bossName)
        {
            /**
             * New boss names must be added to this switch.  
             */
            switch (bossName)
            {
                case "Garmoth":
                    return "**Smite this monstrous creature!**";
                case "Isabella":
                    return "**Burn the witch!**";
                case "Karanda":
                    return "**The bird must die!**";
                case "Kutum":
                    return "**Return him to his tomb where he belongs!**";
                case "Kzarka":
                    return "**Do not allow him to escape the shrine!**";
                case "Muraka":
                    return "**The so-called King of Ogres is no king of us!**";
                case "Nouver":
                    return "**Kill that damn dragon!**";
                case "Offin":
                    return "**Be fearless and destroy this awful evil!**";
                case "Quint":
                    return "**Sever the heard and set fire to the corpse!**";
                case "Vell":
                    return "**The scourge of the deep sea must be slain!**";
                case "Fireworks":
                    return "**Fireworks will soon be on display off the coast of Velia!** Come see the show! :smiley:";
                case "Targargo":
                    return "**Let the feast begin!** :grin:";
                case "BonusXP":
                    return "**Time to grind, mi dachi! There is an active bonus of** ***1000%*** **on all combat experience gained!** :smiley:";
                default:
                    return "The abyss is calling.";
            }
        }

        public static string SortedSchedule(string[] bossWaitTimes, string[] bossNames)
        {
            for (int outer = 0; outer < bossWaitTimes.Length; outer++)
            {       
                string minimum = bossWaitTimes[outer];
                string minimumName = bossNames[outer];

                int minimumIndex = outer;

                for (int inner = outer; inner < bossWaitTimes.Length; inner++)
                {

                    if (NewTimeIsEarlier(bossWaitTimes[inner], bossWaitTimes[minimumIndex]))
                    {
                        minimumIndex = inner;
                    }
                }


                minimum = bossWaitTimes[minimumIndex];

                minimumName = bossNames[minimumIndex];

                bossWaitTimes[minimumIndex] = bossWaitTimes[outer];

                bossNames[minimumIndex] = bossNames[outer];

                bossWaitTimes[outer] = minimum;

                bossNames[outer] = minimumName;
            }
            
            string schedule = "";

            for (int index = 0; index < bossNames.Length; index++)
            {
                string next = "**" + bossNames[index] + "** will return in** " + bossWaitTimes[index] + ".**\n";

                if (next == "**" + bossNames[index] + "** will return in** 0 days, 0 hours, 0 minutes.**\n")
                {
                    next = "**" + bossNames[index] + "** has reappeared to terrorize the land! " + BossMessage(bossNames[index]) + "\n";

                    /**
                     * New boss names must be added to this switch.  
                     */
                    switch (bossNames[index])
                    {
                        case "Kzarka":
                            BossCountdown += 10;
                            KzarkaUp = true;
                            break;
                        case "Karanda":
                            BossCountdown += 10;
                            KarandaUp = true;
                            break;
                        case "Nouver":
                            BossCountdown += 10;
                            NouverUp = true;
                            break;
                        case "Isabella":
                            BossCountdown += 10;
                            IsabellaUp = true;
                            break;
                        case "Offin":
                            BossCountdown += 10;
                            OffinUp = true;
                            break;
                        case "Kutum":
                            BossCountdown += 10;
                            KutumUp = true;
                            break;
                        case "Muraka":
                            BossCountdown += 10;
                            MurakaUp = true;
                            break;
                        case "Quint":
                            BossCountdown += 10;
                            QuintUp = true;
                            break;
                        case "Vell":
                            BossCountdown += 10;
                            VellUp = true;
                            break;
                        case "Garmoth":
                            BossCountdown += 10;
                            GarmothUp = true;
                            break;
                        case "Fireworks":
                            BossCountdown += 19;
                            FireworksUp = true;
                            next = BossMessage(bossNames[index]) + "\n";
                            break;
                        case "Targargo":
                            BossCountdown += 10;
                            TargargoUp = true;
                            break;
                        case "BonusXP":
                            ActivateFieldBoss("BonusXP", "Infinity 0");
                            next = "";
                            break;
                    }
                }
                /**
                 * New boss names must be added to this conditional.  
                 */
                else if (
                            (GarmothUp && bossNames[index] == "Garmoth")
                            || (KzarkaUp && bossNames[index] == "Kzarka")
                            || (KarandaUp && bossNames[index] == "Karanda")
                            || (NouverUp && bossNames[index] == "Nouver")
                            || (IsabellaUp && bossNames[index] == "Isabella")
                            || (OffinUp && bossNames[index] == "Offin")
                            || (KutumUp && bossNames[index] == "Kutum")
                            || (MurakaUp && bossNames[index] == "Muraka")
                            || (QuintUp && bossNames[index] == "Quint")
                            || (VellUp && bossNames[index] == "Vell")
                            || (TargargoUp && bossNames[index] == "Targargo")
                            || (FireworksUp && bossNames[index] == "Fireworks")
                        )
                {
                    next = "**" + bossNames[index] + "** is terrorizing the land! " + BossMessage(bossNames[index]) + "\n";

                    if (bossNames[index] == "Fireworks")
                    {
                        next = BossMessage(bossNames[index]) + "\n";
                    }

                    if (!Commander.refreshRequested)
                    {
                        BossCountdown--;
                    }

                    if (BossCountdown == 0)
                    {
                        next = "**Well done!** Our people are safe, and **" + bossNames[index] + "** is no longer a threat. :wink:\n";

                        if (bossNames[index] == "Fireworks")
                        {
                            next = "Fireworks will appear in the sky north of Velia shortly. **Enjoy the show!** :smiley:\n";
                        }

                        // You must not forget to set boss back to false.

                        GarmothUp = false;
                        IsabellaUp = false;
                        KarandaUp = false;
                        KutumUp = false;
                        KzarkaUp = false;
                        MurakaUp = false;
                        NouverUp = false;
                        OffinUp = false;
                        QuintUp = false;
                        VellUp = false;
                        TargargoUp = false;
                        FireworksUp = false;
                    }
                }

                else if (bossNames[index] == "Fireworks")
                {
                    next = "**Fireworks will appear off the coast of Velia in " + bossWaitTimes[index] + ".**\n";
                }

                else if (bossNames[index] == "BonusXP" && !BonusUp)
                {
                    next = "**Get ready, homies! An epic bonus-experience event begins in " + bossWaitTimes[index] + ".** :grin:\n";
                }

                else if (bossNames[index] == "BonusXP" && BonusUp)
                {
                    next = "";
                }

                if (next.Contains("terroriz") || next.Contains("safe"))
                {
                    schedule = next + schedule;
                } else
                {
                    schedule += next;
                }
            }

            if (FieldBossCountdowns.ToArray().Length > 0)
            {
                schedule = AddFieldBosses(schedule);
            }

            if (BellServers.ToArray().Length > 0)
            {
                schedule = AddBells(schedule);
            }

            schedule = schedule.Replace(" 0 days,", "").Replace(" 0 hours,", "").Replace(" 0 minutes", "")
                .Replace(" 0 day,", "").Replace(" 0 hour,", "").Replace("days,.", "days.")
                .Replace("day,.", "day.").Replace("hours,.", "hours.").Replace("hour,.", "hour.")
                .Replace("1 minutes", "1 minute").Replace("1 hours", "1 hour").Replace("1 days", "1 day");

            return schedule;
        }

        public static bool NewTimeIsEarlier(string newTime, string oldTime)
        {
            string[] timeOneArray = newTime.Replace(",", "").Split(' ');
            string[] timeTwoArray = oldTime.Replace(",", "").Split(' ');

            bool newIsEarlier = false;
            
            if (Int32.Parse(timeTwoArray[0]) > Int32.Parse(timeOneArray[0]))
            {
                newIsEarlier = true;
            } else if (Int32.Parse(timeTwoArray[0]) == Int32.Parse(timeOneArray[0]) 
                    && Int32.Parse(timeTwoArray[2]) > Int32.Parse(timeOneArray[2]))
            {
                newIsEarlier = true;
            } else if (Int32.Parse(timeTwoArray[0]) == Int32.Parse(timeOneArray[0])
                    && Int32.Parse(timeTwoArray[2]) == Int32.Parse(timeOneArray[2])
                    && Int32.Parse(timeTwoArray[4]) > Int32.Parse(timeOneArray[4]))
            {
                newIsEarlier = true;
            }
            
            return newIsEarlier;
        }
        
        public static string ETA(string[] Boss)
        {
            string ETA = "";

            var nextSpawn = Boss[GetNextTime(Boss)];

            var time = nextSpawn.Substring(2, 5);

            int day = Int32.Parse(nextSpawn.Substring(0, 1));
            int hour = Int32.Parse(time.Substring(0, 2));
            int minute = Int32.Parse(time.Substring(3, 2));

            int currentDay = DayCode(DateTime.Now.DayOfWeek.ToString());
            int currentHour = Int32.Parse(DateTime.Now.ToString("HH"));
            int currentMinute = Int32.Parse(DateTime.Now.ToString("mm"));
            
            int daysRemaining = day - currentDay;
            int hoursRemaining = hour - currentHour;
            int minutesRemaining = minute - currentMinute;
            
            if (minutesRemaining < 0)
            {
                hoursRemaining -= 1;
                minutesRemaining = 60 + minutesRemaining;
            }
            if (minutesRemaining > 1 || minutesRemaining == 0)
            {
                ETA = minutesRemaining + " minutes" + ETA;
            }
            else
            {
                ETA = minutesRemaining + " minute" + ETA;
            }

            if (hoursRemaining < 0)
            {
                daysRemaining -= 1;
                hoursRemaining = 24 + hoursRemaining;
            }
            if (hoursRemaining > 1 || hoursRemaining == 0)
            {
                ETA = hoursRemaining + " hours, " + ETA;
            }
            else
            {
                ETA = hoursRemaining + " hour, " + ETA;
            }

            if (daysRemaining < 0)
            {
                daysRemaining = 7 + daysRemaining;
            }
            if (daysRemaining > 1 || daysRemaining == 0)
            {
                ETA = daysRemaining + " days, " + ETA;
            }
            else
            {
                ETA = daysRemaining + " day, " + ETA;
            }

            return ETA;
        }
        
        public static int GetNextTime(string[] BossTimes)
        {
            int index = 0;

            int currentDay = DayCode(DateTime.Now.DayOfWeek.ToString());

            foreach (string time in BossTimes)
            {
                int dayCode = Int32.Parse(time.Substring(0, 1));

                int hour = Int32.Parse(time.Substring(2, 2));
                int minute = Int32.Parse(time.Substring(5, 2));

                int currentHour = Int32.Parse(DateTime.Now.ToString("HH"));
                int currentMinute = Int32.Parse(DateTime.Now.ToString("mm"));

                if (dayCode > currentDay)
                {
                    return index;
                } else if (dayCode == currentDay && hour > currentHour)
                {
                    return index;
                } else if (dayCode == currentDay && hour == currentHour && minute >= currentMinute)
                {
                    return index;
                }
                

                index += 1;
            }
            
            return 0;
        }

        public static void ActivateBell(string server, string duration)
        {
            int sort = SortedInsertionPoint(duration);
            
            BellServers.Insert(sort, server);
            BellDurations.Insert(sort, duration);
        }

        public static int SortedInsertionPoint(string duration)
        {
            int sort = 0; 

            for (int index = 0; index < BellDurations.Count; index++)
            {
                string next = "" + BellDurations[index];

                if (Int32.Parse(next) > Int32.Parse(duration))
                {
                    sort++;
                }
                else
                {
                    return sort;
                }
            }

            return sort;
        }

        public static void DeactivateBell(int bellIndex)
        {
            BellServers.RemoveAt(bellIndex);
            BellDurations.RemoveAt(bellIndex);
        }

        public static string AddBells(string schedule)
        {
            int bells = BellServers.ToArray().Length;

            if (bells == 1)
            {
                schedule += "\n:crossed_swords: Some wonderful adventurer has activated a **Golden Bell**! :crossed_swords:\n\n";
            } else if (bells == 2) {
                schedule += "\n:crossed_swords: Some wonderful adventurers have activated a couple **Golden Bells**! :crossed_swords:\n\n";
            } else if (bells == 3) {
                schedule += "\n:crossed_swords: Some wonderful adventurers have activated a few **Golden Bells**! :crossed_swords:\n\n";
            } else {
                schedule += "\n:crossed_swords: Some wonderful adventurers have activated several **Golden Bells**! :crossed_swords:\n\n";
            }

            for (int index = 0; index < BellServers.ToArray().Length; index++)
            {

                if (!Commander.refreshRequested) { 
                    BellDurations[index] = Int32.Parse(BellDurations[index].ToString()) - 1;
                }

                if (Int32.Parse(BellDurations[index].ToString()) == 0)
                {
                    BellServers.RemoveAt(index);
                    BellDurations.RemoveAt(index);
                    index--;
                } else
                {
                    schedule += "**" + Capitalize(BellServers[index].ToString()) + "** has an active buff with about **" + Int32.Parse(BellDurations[index].ToString())
                        + " minutes remaining.**\n";
                }
            }

            return schedule;
        }

        public static void ActivateFieldBoss(string bossName, string server)
        {
            string boss = bossName.ToLower().Replace("the ", "");
            server = server.Replace(".", "").Replace(",", "").Replace("?", "").Replace("!", "");

            if (boss == "bheg" || boss == "dastard bheg")
            {
                FieldBossNames.Add("The Dastard Bheg");
            }
            else if (boss == "tree" || boss == "spirit" || boss == "dim tree"
                || boss == "tree spirit" || boss == "dim tree spirit")
            {
                FieldBossNames.Add("The Dim Tree Spirit");
            }
            else if (boss == "mudster" || boss == "giant mudster")
            {
                FieldBossNames.Add("The Giant Mudster");
            }
            else if (boss == "red nose" || boss == "red" || boss == "imp captain" || boss == "captain red nose"
                || boss == "imp captain red nose" || boss == "nose")
            {
                FieldBossNames.Add("The Imp Captain Red Nose");
            }
            else if (boss == "katzvariak" || boss == "katz")
            {
                FieldBossNames.Add("The Mysterious Katzvariak");
            }
            else if (boss == "targargo" || boss == "king targargo" || boss == "turkey king targargo" || boss == "turkey")
            {
                FieldBossNames.Add("The Turkey King Targargo");
            }
            else if (boss == "bonusxp")
            {
                BonusUp = true;
                FieldBossNames.Add("**Time to grind, mi dachi! There is an active bonus of** ***1000%*** **on all combat experience gained!** :smiley:");
                FieldBossServers.Add(server);
                FieldBossCountdowns.Add(180);
                return;
            }
            else
            {
                return;
            }
            FieldBossServers.Add(server);
            FieldBossCountdowns.Add(15);
        }

        public static void DeactivateFieldBoss(int index)
        {
            if (FieldBossNames[index].ToString().ToLower().Contains("time to grind"))
            {
                BonusUp = false;
            }

            FieldBossNames.RemoveAt(index);
            FieldBossServers.RemoveAt(index);
            FieldBossCountdowns.RemoveAt(index);
        }

        public static string AddFieldBosses(string schedule)
        {
            string fields = "";

            for (int index = 0; index < FieldBossNames.ToArray().Length; index++)
            {
                if (FieldBossNames[index].ToString().ToLower().Contains("time to grind"))
                {
                    fields += FieldBossNames[index].ToString() + "\n";
                }
                else { 
                    fields += "**" + Capitalize(FieldBossNames[index].ToString()) + "** was spotted approximately **" + (16 - Int32.Parse(FieldBossCountdowns[index].ToString())) 
                        + " minutes ago**. If you're nearby, check **" + Capitalize(FieldBossServers[index].ToString()) + "**!\n";
                }

                FieldBossCountdowns[index] = Int32.Parse(FieldBossCountdowns[index].ToString()) - 1;

                if (Int32.Parse(FieldBossCountdowns[index].ToString()) == 0)
                {
                    DeactivateFieldBoss(index);
                    index--;
                }
            }

            return fields + schedule;
        }

        public static string Capitalize(string input)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input);
        }
        
        public static bool IsBoss(int index)
        {
            if (FieldBossNames[index].ToString().ToLower().Contains("time to grind"))
            {
                return false;
            }

            return true;
        }

        public static string[] Garmoth = {
                                    "0 20:00",
                                    "3 01:15",
                                    "5 01:15"
                                        };

        public static string[] Isabella = {
                                    "0 02:00", "0 14:00", "0 18:00",
                                    "1 02:00", "1 14:00", "1 18:00",
                                    "2 02:00", "2 14:00", "2 18:00",
                                    "3 02:00", "3 14:00", "3 18:00",
                                    "4 02:00", "4 14:00", "4 18:00",
                                    "5 02:00", "5 14:00", "5 18:00",
                                    "6 02:00", "6 14:00", "6 18:00"
                                        };
        //"0 01:00", "0 13:00", "0 17:00",
        //"1 01:00", "1 13:00", "1 17:00",
        //"2 01:00", "2 13:00", "2 17:00",
        //"3 01:00", "3 13:00", "3 17:00", DAYLIGHT SAVINGS

        public static string[] Karanda = {
                                    "0 20:00",
                                    "1 03:00",
                                    "2 01:15", "2 20:00",
                                    "3 03:00", "3 10:00", "3 23:15",
                                    "4 23:15",
                                    "5 06:00", "5 13:00",
                                    "6 01:15", "6 20:00"
                                        };
        //"0 19:00",
        //"1 02:00",
        //"2 00:15", "2 19:00",
        //"3 02:00", "3 09:00", "3 22:15", DAYLIGHT SAVINGS

        public static string[] Kutum = {
                                    "0 01:15", "0 06:00",
                                    "1 01:15", "1 17:00",
                                    "2 03:00", "2 10:00",
                                    "3 01:15", "3 20:00",
                                    "4 03:00", "4 10:00", "4 17:00",
                                    "5 10:00", "5 23:15",
                                    "6 10:00"
                                        };
        //"0 00:15", "0 05:00",
        //"1 00:15", "1 16:00",
        //"2 02:00", "2 09:00",
        //"3 00:15", "3 19:00", DAYLIGHT SAVINGS

        public static string[] Kzarka = {
                                    "0 03:00", "0 13:00", "0 23:15",
                                    "1 06:00", "1 10:00", "1 23:15",
                                    "2 06:00", "2 23:15",
                                    "3 17:00", "3 23:15",
                                    "4 06:00",
                                    "5 01:15", "5 20:00", "5 23:15",
                                    "6 20:00"
                                        };
        //"0 02:00", "0 12:00", "0 22:15",
        //"1 05:00", "1 09:00", "1 22:15",
        //"2 05:00", "2 22:15",
        //"3 16:00", "3 22:15", DAYLIGHT SAVINGS

        public static string[] Muraka = {
                                    "6 17:00"
                                        };

        public static string[] Nouver = {
                                    "0 01:15", "0 10:00", "0 23:15",
                                    "1 13:00", "1 20:00",
                                    "2 17:00", "2 23:15",
                                    "3 13:00",
                                    "4 01:15", "4 13:00",
                                    "5 03:00", "5 17:00",
                                    "6 06:00", "6 13:00"
                                        };
        //"0 00:15", "0 09:00", "0 22:15",
        //"1 12:00", "1 19:00",
        //"2 16:00", "2 22:15",
        //"3 12:00", DAYLIGHT SAVINGS

        public static string[] Offin = {
                                    "2 13:00",
                                    "4 20:00",
                                    "6 03:00"
                                        };
        //"2 12:00", DAYLIGHT SAVINGS

        public static string[] Quint = {
                                    "6 17:00"
                                        };

        public static string[] Vell = {
                                    "0 17:00"
                                       };
        //"0 16:00" DAYLIGHT SAVINGS

        public static string[] Fireworks = {
                                    "5 19:40"
                                       };
        //"0 16:00" DAYLIGHT SAVINGS

        public static string[] Targargo = {
                                    "0 02:00", "0 14:00", "0 18:00",
                                    "1 02:00", "1 14:00", "1 18:00",
                                    "2 02:00", "2 14:00", "2 18:00",
                                    "3 02:00", "3 14:00", "3 18:00",
                                    "4 02:00", "4 14:00", "4 18:00",
                                    "5 02:00", "5 14:00", "5 18:00",
                                    "6 02:00", "6 14:00", "6 18:00",
                                       };

        public static string[] BonusXP = {
                                    "0 18:00", "0 23:00",
                                    "5 18:00", "5 23:00",
                                    "6 18:00", "6 23:00",
                                       };
    }
}
