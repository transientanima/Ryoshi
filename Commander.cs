using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.Udp;

/**************************************************************************
**                                Ryoshi                                 **
**************************************************************************/

namespace Ryoshi
{
    public class Commander : BaseCommandModule
    {
        public static bool refreshRequested = false;
        public static bool askedToHelp = false;

        static string nickname = null;
        
        static InteractivityExtension interactivity = null;

        static DSharpPlus.Entities.DiscordMessage timer = null;
        static DSharpPlus.Entities.DiscordMessage lastComment = null;

        static Process Gakushi = null;

        public static void StartGakushi(Process creation)
        {
            Gakushi = creation;

        }

        [Command("Ryoshi!")]
        public async Task Birth(CommandContext context)
        {
            if (interactivity == null)
            {
                nickname = Nickname(context.User.ToString());

                interactivity = context.Client.GetInteractivity();

                timer = await context.RespondAsync($"Aloha! Let me get the timer running!\n\n" +
                            $"{Schedule.GenerateSchedule()}\n" +
                            $"Please feel free to help us keep track of **field bosses** and **Golden Bells** by letting me know where you spot them!\n\n" +
                            $"You are also welcome to share this timer with any friendly spirits who cross your path.\n\n" +
                            $"https://discord.gg/ndVAvQ3");

                lastComment = await context.RespondAsync($"Health, wealth, and stealth, dachi! :wink: I hope you're having a lovely grind!");
            }

            if (context != null) { 
                await context.Message.DeleteAsync();
            }

            try
            {
                await Existence();
            } finally
            {
                await Task.Delay(120000);
                await Rebirth(context);
            }
        }

        public async Task Rebirth(CommandContext context)
        {
            await Birth(context);
        }

        public async Task Existence()
        {
            while (true)
            {
                var remaining = 60 - Int32.Parse(DateTime.Now.ToString("ss"));
                var content = await interactivity.WaitForMessageAsync(xm => true, TimeSpan.FromSeconds(remaining));

                refreshRequested = false;

                string message;

                if (content != null)
                {
                    message = Message(content.Message.ToString()).ToLower();
                    nickname = Nickname(content.User.ToString());
                }
                else
                {
                    message = "";
                    nickname = "";
                }

                if (
                    message.Contains("how are you")
                        && (message.Contains("and") || message.Contains("ryo") || message.Contains("shi") || message.Contains("ryoshi"))
                   )
                {
                    lastComment = await lastComment.ModifyAsync($"I couldn't be better! And how are you?");
                }

                else if (message.Contains("how?") || message.Contains("help"))
                {
                    lastComment = await lastComment.ModifyAsync($"Keeping me in the loop is super easy! " +
                        $"Let's say `the Dim Tree Spirit` spawns on `Balenos 3`. You can tell me by saying:\n\n" +
                        $"\t`Tree is up on Balenos 3.`\n\n" +
                        $"The period is optional, and you can say (most) standard variations of field boss names, such as `Dim Tree`. " +
                        $"*However*, you ***must*** say `is up on` between the boss name and the server name.\n\n" +
                        $"Now, let's say a someone activates a **Golden Bell** after the Dim Tree Spirit dies:\n\n" +
                        $"\t`Bell 60 Balenos 3.`\n\n" +
                        $"Again, the period is optional but the rest of the format of the command is not. `60` represents the remaining time on the bell's XP buff. " +
                        $"If you know about a bell that only has `20` minutes left, you would just change the `60` to a `20`.\n\n" +
                        $"All commands are also **case-insensitive** (as they should be). " +
                        $"If you're confused or a command isn't working, just ask **Zendachi Kazenai** for more assistance. " +
                        $"May the force be with you, mi dachi! :wink:");
                    askedToHelp = true;
                }

                else if (message.Contains("?")
                            && (message.Contains("discord link") || message.Contains("this discord") || message.Contains("link here") || message.Contains("this server")
                                || message.Contains("permanent link") || message.Contains("this channel") || message.Contains("museifu discord"))
                   )
                {
                    lastComment = await lastComment.ModifyAsync($"If I'm not mistaken, you're looking for this:" +
                        $"\n\nhttps://discord.gg/ndVAvQ3");
                }

                else if (message.Contains("thank"))
                {
                    var lastMessage = Message(lastComment.ToString());

                    lastComment = await lastComment.ModifyAsync($"{lastMessage}\nDon't mention it, homie! :smiley:");
                }

                else if (message.Contains("farewell") || message.Contains("peace") || message.Contains("see ya") || message.Contains("bye") || message.Contains("later")
                            || message.Contains("seeya") || message.Contains("deuces") || message.Contains("fare well") || message.Contains("gotta go") || message.Contains("time to go")
                            || message.Contains("ciao") || message.Contains("ttyl"))
                {
                    lastComment = await lastComment.ModifyAsync($"Health, wealth, and stealth, my dachi! Have a lovely grind. :smiley:");
                }

                else if (message == "")
                {
                    timer = await timer.ModifyAsync($"One moment, please. I'm just refreshing the timer...");

                    timer = await timer.ModifyAsync($"I refreshed the timer at **{DateTime.Now.ToString("h:mm tt")}** (American Eastern Standard Time)!\n\n" +
                            $"{Schedule.GenerateSchedule()}\n" +
                            $"Please feel free to help us keep track of **field bosses** and **Golden Bells** by letting me know where you spot them!\n\n" +
                            $"You are also welcome to share this timer with any friendly spirits who cross your path.\n\n" +
                            $"https://discord.gg/ndVAvQ3");

                    if (!askedToHelp)
                    {
                        lastComment = await lastComment.ModifyAsync($"Health, wealth, and stealth, my dachi! Have a lovely grind. :smiley:");
                    } else
                    {
                        askedToHelp = false;
                    }
                }

                else if (message.Contains(" is up on "))
                {
                    lastComment = await lastComment.ModifyAsync($"Thanks for letting me know! I'll relay the message...");

                    try { 
                        AddBoss(message);
                    }
                    catch
                    {
                        lastComment = await lastComment.ModifyAsync($"Oh dear, I'm so confused... Can you please double-check your input?" +
                            $"\nIf it looks good to you, please ask Kazenai what's wrong. He knows me better than I know myself...");
                    }
                    finally
                    {
                        await Refresh();
                    }
                }

                else if (message.EndsWith("dead") || message.EndsWith("dead.") || message.EndsWith("dead!")
                        || message.EndsWith("died") || message.EndsWith("died.") || message.EndsWith("died!"))
                {
                    if (message.StartsWith("field boss"))
                    {

                        try {
                            int bossIndex = Int32.Parse(message.Split(' ')[2]) - 1;
                            if (Schedule.IsBoss(bossIndex))
                            {
                                Schedule.DeactivateFieldBoss(bossIndex);
                                lastComment = await lastComment.ModifyAsync($"If I recall correctly, death is good in this context... :smiley:");
                            }
                        }
                        catch
                        {
                            lastComment = await lastComment.ModifyAsync($"Oh dear, I'm so confused... Can you please double-check your input?" +
                                $"\nIf it looks good to you, please ask Kazenai what's wrong. He knows me better than I know myself...");
                        }
                        finally
                        {
                            await Refresh();
                        }
                    }
                    else if (message.StartsWith("boss"))
                    {
                        try { 
                            int bossIndex = Int32.Parse(message.Split(' ')[1]) - 1;
                            if (Schedule.IsBoss(bossIndex))
                            {
                                Schedule.DeactivateFieldBoss(bossIndex);
                                lastComment = await lastComment.ModifyAsync($"If I recall correctly, death is good in this context... :smiley:");
                            }
                        }
                        catch
                        {
                            lastComment = await lastComment.ModifyAsync($"Oh dear, I'm so confused... Can you please double-check your input?" +
                                $"\nIf it looks good to you, please ask Kazenai what's wrong. He knows me better than I know myself...");
                        }
                        finally
                        {
                            await Refresh();
                        }
                    }
                }

                else if (message.Contains("what is a bell") || message.Contains("what's a bell")
                    || message.Contains("what is a golden bell") || message.Contains("what's a golden bell")
                    || message.EndsWith("bell?") || message.EndsWith("bells?"))
                {
                    lastComment = await lastComment.ModifyAsync($"A **Golden Bell** grants **100% Bonus Combat Experience** to everyone on one server channel for one hour. If any bells are active, I'll post them below the boss times. :smiley:");
                }

                else if (message.StartsWith("bell"))
                {
                    lastComment = await lastComment.ModifyAsync($"Thanks! I've made sure our friends can see that.");

                    string duration = "";
                    
                    string server = "";
                    
                    if (message.Split(' ').Length > 2)
                    {
                        duration = message.Split(' ')[1];
                        server = message.Split(' ')[2];
                    }

                    try {
                        server += " " + message.Split(' ')[3];
                    } catch
                    {
                        // No worries, just Arsha.
                    }

                    try {
                        Schedule.ActivateBell(server, duration);
                    }
                    catch
                    {
                        lastComment = await lastComment.ModifyAsync($"Oh dear, I'm so confused... Can you please double-check your input?" +
                            $"\nIf it looks good to you, please ask Kazenai what's wrong. He knows me better than I know myself...");
                    }
                    finally
                    {
                        await Refresh();
                    }
                }

                else if (message.StartsWith("kill bell"))
                {
                    try {
                        int bellIndex = Int32.Parse(message.Split(' ')[2]) - 1;
                        Schedule.DeactivateBell(bellIndex);
                        lastComment = await lastComment.ModifyAsync($"No problem, I've deactivated that alert.");
                    }
                    catch
                    {
                        lastComment = await lastComment.ModifyAsync($"Oh dear, I'm so confused... Can you please double-check your input?" +
                            $"\nIf it looks good to you, please ask Kazenai what's wrong. He knows me better than I know myself...");
                    }
                    finally
                    {
                        await Refresh();
                    }
                }

                else if (message.Contains("time to sleep"))
                {
                    await content.Message.DeleteAsync();
                    await timer.DeleteAsync();
                    await lastComment.DeleteAsync();
                    Environment.Exit(0);
                }

                else if (RequestingRefresh(message))
                {
                    await Refresh();
                }

                else
                {
                    lastComment = await lastComment.ModifyAsync($"Sorry, but I don't know what that means! :sweat_smile:");
                }

                if (content != null)
                {
                    await content.Message.DeleteAsync(null);
                }
            }
        }

        public bool RequestingRefresh(string message)
        {
            return      (
                            (message.Contains("ryo") || message.Contains("shi") || message.Contains("ryoshi"))
                            && (message.Contains("what") || message.Contains("which") || message.Contains("who") || message.Contains("whom"))
                            && (message.Contains("up") || message.Contains("next"))
                        )
                        ||
                        (
                            message.Contains("who next") || message.Contains("who's next") || message.Contains("what's next")
                            || message.Contains("who is next") || message.Contains("spawns next?") || message.Contains("any bosses up")
                            || message.Contains("boss next?") || message.Contains("boss is up next?") || message.Contains("boss spawns next?")
                            || message.Contains("bosses spawn next") || message.Contains("boss spawns soon") || message.Contains("bosses spawn")
                            || message.Contains("bosses spawning") || message.Contains("bosses spawn soon") || message.Contains("bosses spawning soon")
                            || message.Contains("up next?") || message.Contains("spawning next?") || message.Contains("who's up?") || message.EndsWith("next?")
                            || message.Contains("next boss?") || message.Contains("boss spawning?") || message.Contains("boss about to spawn?")
                            || message.Contains("any bosses?") || message.Contains("boss up?") || message.Contains("bosses up?") || message.Contains("refresh")
                        )
                        ||
                        (
                            message == "next?"
                        );
        }

        public async Task Refresh()
        {
            refreshRequested = true;

            timer = await timer.ModifyAsync($"Need a refresh? Sure, I can do that for ya. One moment, please...");

            timer = await timer.ModifyAsync($"Refreshed for **{nickname}** at **{DateTime.Now.ToString("h:mm tt")}** (American Eastern Standard Time)! :sunglasses:\n\n" +
                $"{Schedule.GenerateSchedule()}\n" +
                $"Please feel free to help us keep track of **field bosses** and **Golden Bells** by letting me know where you spot them!\n\n" +
                $"You are also welcome to share this timer with any friendly spirits who cross your path.\n\n" +
                $"https://discord.gg/ndVAvQ3");
        }

        public string Nickname(string fullname)
        {
            var start = fullname.IndexOf("(") + 1;
            var end = fullname.IndexOf(")");
            var size = end - start;

            var nickname = fullname.Substring(start, size);

            return nickname;
        }

        public string Message(string data)
        {
            var start = data.IndexOf("Contents: ") + 10;
            var end = data.Length;
            var size = end - start;

            var message = data.Substring(start, size);

            return message;
        }

        public static void AddBoss(string message)
        {
            string[] messageArray = message.Split(' ');

            int bossNameEndpoint = -1;
            int serverNameStartpoint = -1;

            int index = 0;

            while (bossNameEndpoint == -1)
            {
                if (messageArray[index] == "is")
                {
                    bossNameEndpoint = index;
                    serverNameStartpoint = index + 3;
                }

                index++;
            }

            string boss = messageArray[0];

            for (index = 1; index < bossNameEndpoint; index++)
            {
                boss += " " + messageArray[index];
            }

            string server = message.Split(' ')[serverNameStartpoint] + " " + message.Split(' ')[serverNameStartpoint + 1];

            Schedule.ActivateFieldBoss(boss, server);

        }
    }
}
