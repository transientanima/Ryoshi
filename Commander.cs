using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.Udp;

/**************************************************************************
**                                Ryoshi                                 **
**************************************************************************/

namespace Ryoshi
{
    public class Commander : BaseCommandModule
    {
        public static List<DiscordMember> TaggedMembers = new List<DiscordMember>();

        public static DiscordGuild museifu = null;

        public static bool refreshRequested = false;
        public static bool askedToHelp = false;

        static string nickname = null;
        
        static InteractivityExtension interactivity = null;

        static DSharpPlus.Entities.DiscordMessage timer = null;
        static DSharpPlus.Entities.DiscordMessage lastComment = null;

        [Command("Ryoshi!")]
        public async Task Birth(CommandContext context)
        {
            if (interactivity == null)
            {
                nickname = Nickname(context.User.ToString());

                interactivity = context.Client.GetInteractivity();

                museifu = context.Guild;

                LoadAlerts(context);
                
                string schedule = await Schedule.GenerateSchedule();

                timer = await context.RespondAsync($"Aloha! Let me get the timer running!\n\n" +
                            $"{schedule}\n" +
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
            }
            catch (Exception error)
            {
                await CommandRespond(context, $"An strange error occurred! {error}");
                await Rebirth();
            }
        }

        public async Task Rebirth()
        {
            await Birth(null);
        }

        public async Task Existence()
        {
            while (true)
            {
                var remaining = 60 - Int32.Parse(DateTime.Now.ToString("ss"));
                var context = await interactivity.WaitForMessageAsync(data => !data.Author.IsBot, TimeSpan.FromSeconds(remaining));
                
                string message;

                if (Received(context))
                {
                    message = Message(context.Message.ToString()).ToLower();
                    nickname = Nickname(context.User.ToString());
                }
                else
                {
                    message = "";
                    nickname = "";
                }

                if (Received(context) && context.Message.Author.IsBot)
                {
                    // Do nothing and wait for next message.
                }

                else if (Howdy(message))
                {
                    await Respond(context, $"I couldn't be better! And how are you?");
                }

                else if (OfferingHelp(message))
                {
                    await Respond(context, $"Keeping me in the loop is super easy! " +
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

                else if (WantsLink(message))
                {
                    await Respond(context, $"If I'm not mistaken, you're looking for this:" +
                        $"\n\nhttps://discord.gg/ndVAvQ3");
                }

                else if (Grateful(message))
                {
                    var lastMessage = Message(lastComment.ToString());

                    await Respond(context, $"{lastMessage}\nDon't mention it, homie! :smiley:");
                }

                else if (RequestingUntag(message))
                {
                    await Unregister(context);
                }

                else if (RequestingTag(message))
                {
                    await Register(context);
                }

                else if (Farewell(message))
                {
                    await Respond(context, $"Health, wealth, and stealth, my dachi! Have a lovely grind. :smiley:");
                }

                else if (Blank(message))
                {
                    refreshRequested = false;

                    timer = await timer.ModifyAsync($"One moment, please. I'm just refreshing the timer...");

                    string schedule = await Schedule.GenerateSchedule();

                    timer = await timer.ModifyAsync($"I refreshed the timer at **{DateTime.Now.ToString("h:mm tt")} (North American Eastern Standard Time)**!\n\n" +
                            $"{schedule}\n" +
                            $"Please feel free to help us keep track of **field bosses** and **Golden Bells** by letting me know where you spot them!\n\n" +
                            $"You are also welcome to share this timer with any friendly spirits who cross your path.\n\n" +
                            $"https://discord.gg/ndVAvQ3");

                    if (!askedToHelp)
                    {
                        await Respond(context, $"Health, wealth, and stealth, my dachi! Have a lovely grind. :smiley:");
                    } else
                    {
                        askedToHelp = false;
                    }
                }

                else if (ActivatingFieldBoss(message))
                {
                    await Respond(context, $"Thanks for letting me know! I'll relay the message...");

                    try {
                        AddBoss(message);
                    }
                    catch
                    {
                        await Respond(context, $"Oh dear, I'm so confused... Can you please double-check your input?" +
                            $"\nIf it looks good to you, please ask Kazenai what's wrong. He knows me better than I know myself...");
                    }
                    finally
                    {
                        refreshRequested = true;
                        await Refresh();
                    }
                }

                else if (DeactivatingFieldBoss(message))
                {
                    if (message.StartsWith("field boss"))
                    {

                        try {
                            int bossIndex = Int32.Parse(message.Split(' ')[2]) - 1;

                            if (Schedule.BonusUp)
                            {
                                bossIndex++;
                            }
                            
                            Schedule.DeactivateFieldBoss(bossIndex);
                            await Respond(context, $"If I recall correctly, death is good in this context... :smiley:");
                        }
                        catch
                        {
                            await Respond(context, $"Oh dear, I'm so confused... Can you please double-check your input?" +
                                $"\nIf it looks good to you, please ask Kazenai what's wrong. He knows me better than I know myself...");
                        }
                        finally
                        {
                            refreshRequested = true;
                            await Refresh();
                        }
                    }
                    else if (message.StartsWith("boss"))
                    {
                        try { 
                            int bossIndex = Int32.Parse(message.Split(' ')[1]) - 1;

                            Schedule.DeactivateFieldBoss(bossIndex);
                            await Respond(context, $"If I recall correctly, death is good in this context... :smiley:");
                        }
                        catch
                        {
                            await Respond(context, $"Oh dear, I'm so confused... Can you please double-check your input?" +
                                $"\nIf it looks good to you, please ask Kazenai what's wrong. He knows me better than I know myself...");
                        }
                        finally
                        {
                            refreshRequested = true;
                            await Refresh();
                        }
                    }
                }

                else if (WhatIsBell(message))
                {
                    await Respond(context, $"A **Golden Bell** grants **100% Bonus Combat Experience** to everyone on one server channel for one hour. If any bells are active, I'll post them below the boss times. :smiley:");
                }

                else if (ActivatingBell(message))
                {
                    await Respond(context, $"Thanks! I've made sure our friends can see that.");

                    int duration = 0;
                    
                    string server = "";
                    
                    if (message.Split(' ').Length > 2)
                    {
                        duration = Int32.Parse(message.Split(' ')[1]);
                        server = Server(message.Split(' ')[2]);
                    }

                    try {
                        server += " " + message.Split(' ')[3];
                    } catch
                    {
                        // No worries, just Arsha.
                    }

                    try {
                        await Schedule.ActivateBell(server, duration);
                    }
                    catch
                    {
                        await Respond(context, $"Oh dear, I'm so confused... Can you please double-check your input?" +
                            $"\nIf it looks good to you, please ask Kazenai what's wrong. He knows me better than I know myself...");
                    }
                    finally
                    {
                        refreshRequested = true;
                        await Refresh();
                    }
                }

                else if (DeactivatingBell(message))
                {
                    try {
                        int bellIndex = Int32.Parse(message.Split(' ')[2]) - 1;

                        string server = Schedule.BellServers[bellIndex];

                        Schedule.DeactivateBell(bellIndex);

                        foreach (DiscordMember member in Commander.TaggedMembers)
                        {
                            await member.SendMessageAsync($"The **Golden Bell on {server}** ***was a mistake!*** Please disregard that alert.");
                        }

                        await Respond(context, $"No problem, I've deactivated that alert.");
                    }
                    catch
                    {
                        await Respond(context, $"Oh dear, I'm so confused... Can you please double-check your input?" +
                            $"\nIf it looks good to you, please ask Kazenai what's wrong. He knows me better than I know myself...");
                    }
                    finally
                    {
                        refreshRequested = true;
                        await Refresh();
                    }
                }

                else if (SleepCommand(message))
                {
                    await context.Message.DeleteAsync();
                    await timer.DeleteAsync();
                    await lastComment.DeleteAsync();
                    Environment.Exit(0);
                }
                
                else if (RefreshTimeZoneCode(message))
                {
                    string code = message.Split(' ')[1].Replace(".", "").Replace("!", "").Replace("?", "").Trim();
                    await Refresh(code);
                }

                else if (TimeZoneCode(message))
                {
                    string code = message.Split(' ')[0].Replace(".", "").Replace("!", "").Replace("?", "").Trim();
                    await Refresh(code);
                }

                else if (RequestingRefresh(message))
                {
                    await Refresh();
                }

                else
                {
                    await Respond(context, $"Sorry, but I don't know what that means! :sweat_smile:");
                }

                if (context != null && !context.Channel.IsPrivate)
                {
                    await context.Message.DeleteAsync(null);
                }
            }
        }

        /**************************************************
        **             SCHEDULING FUNCTIONS              **
        **************************************************/
        
        public static void LoadAlerts(CommandContext context)
        {
            var role = context.Guild.Roles.FirstOrDefault(roles => roles.Name == "Alert");

            var everybody = context.Guild.Members.ToList<DiscordMember>();

            TaggedMembers = everybody.Where(user => user.Roles.ToList<DiscordRole>().Contains(role)).ToList<DiscordMember>();
        }

        public async Task CommandRespond(CommandContext context, string message)
        {
            if (context.Channel.IsPrivate)
            {
                await context.Message.RespondAsync($"{message}");
            }
            else
            {
                lastComment = await lastComment.ModifyAsync($"{message}");
            }
        }

        public async Task Respond(MessageContext context, string message)
        { 
            if (context != null && context.Channel.IsPrivate)
            {
                await context.Message.RespondAsync($"{message}");
            }
            else
            {
                lastComment = await lastComment.ModifyAsync($"{message}");
            }
        }

        public async Task Register(MessageContext context)
        {
            try { 
                var member = await museifu.GetMemberAsync(context.User.Id);
                var role = museifu.Roles.FirstOrDefault(roles => roles.Name == "Alert");
                
                await member.GrantRoleAsync(role);

                var everybody = museifu.Members.ToList<DiscordMember>();
                TaggedMembers = everybody.Where(user => user.Roles.Contains(role)).ToList<DiscordMember>();

                await Respond(context, $"Alright, I'll send you a message when bells occur! :smiley:");
            } catch
            {
                await Respond(context, $"I believe something went wrong. Are you already tagged as \"Alert\"?");
            }
        }
        
        public async Task Unregister(MessageContext context)
        {
            try
            {
                var member = await museifu.GetMemberAsync(context.User.Id);
                var role = museifu.Roles.FirstOrDefault(roles => roles.Name == "Alert");

                await member.RevokeRoleAsync(role);

                var everybody = museifu.Members.ToList<DiscordMember>();
                TaggedMembers = everybody.Where(user => user.Roles.ToList<DiscordRole>().Contains(role)).ToList<DiscordMember>();

                await Respond(context, $"No problem, I'll stop messaging you. :wink:\n\t");
            }
            catch
            {
                await Respond(context, $"On second thought, I don't think I've tagged you yet...");
            }
        }

        public async Task Refresh()
        {
            refreshRequested = true;

            timer = await timer.ModifyAsync($"Need a refresh? Sure, I can do that for ya. One moment, please...");
            
            string schedule = await Schedule.GenerateSchedule();

            timer = await timer.ModifyAsync($"Refreshed for **{nickname}** at **{DateTime.Now.ToString("h:mm tt")} (North American Eastern Standard Time)**! :sunglasses:\n\n" +
                $"{schedule}\n" +
                $"Please feel free to help us keep track of **field bosses** and **Golden Bells** by letting me know where you spot them!\n\n" +
                $"You are also welcome to share this timer with any friendly spirits who cross your path.\n\n" +
                $"https://discord.gg/ndVAvQ3");
        }

        public async Task Refresh(string code)
        {
            int index = 0;
            string time = "";
            string zone = "";

            try
            {
                index = Schedule.Codes.IndexOf(code);
                time = Schedule.AdjustedTime(Schedule.Relations[index]);
                zone = Schedule.Zones[index];
            }
            catch
            {
                timer = await timer.ModifyAsync($"Sorry, but I couldn't find that time zone...");

                await Task.Delay(2000);

                await Refresh();

                return;
            }

            refreshRequested = true;

            timer = await timer.ModifyAsync($"Need a refresh? Sure, I can do that for ya. One moment, please...");

            string schedule = await Schedule.GenerateSchedule();

            timer = await timer.ModifyAsync($"Refreshed for **{nickname}** at **{time} {code.ToUpper()} ({zone})**! :sunglasses:\n\n" +
                $"{schedule}\n" +
                $"Please feel free to help us keep track of **field bosses** and **Golden Bells** by letting me know where you spot them!\n\n" +
                $"You are also welcome to share this timer with any friendly spirits who cross your path.\n\n" +
                $"https://discord.gg/ndVAvQ3");
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

            string server = Server(message.Split(' ')[serverNameStartpoint]) + " " + message.Split(' ')[serverNameStartpoint + 1];

            Schedule.ActivateFieldBoss(boss, server);

        }

        /*****************************************************
        **               EXTRACTION FUNCTIONS               **
        *****************************************************/

        public string Nickname(string fullname)
        {
            var nickname = "";

            try
            {
                if (fullname.Contains("(") && fullname.Contains(")"))
                {
                    var start = fullname.IndexOf("(") + 1;
                    var end = fullname.IndexOf(")");
                    var size = end - start;
                    nickname = fullname.Substring(start, size);
                }
                else
                {
                    var start = fullname.IndexOf(";") + 2;
                    var end = fullname.IndexOf("#");
                    var size = end - start;
                    nickname = fullname.Substring(start, size);
                }
            } catch
            {
                return fullname;
            }

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

        public static string Server(string abbreviation)
        {
            abbreviation = abbreviation.ToLower();

            if (abbreviation.StartsWith("ar"))
            {
                return "Arsha";
            }
            else if (abbreviation.StartsWith("bal"))
            {
                return "Balenos";
            }
            else if (abbreviation.StartsWith("cal"))
            {
                return "Calpheon";
            }
            else if (abbreviation.StartsWith("dr"))
            {
                return "Drieghan";
            }
            else if (abbreviation.StartsWith("kam"))
            {
                return "Kamasylvia";
            }
            else if (abbreviation.StartsWith("med"))
            {
                return "Mediah";
            }
            else if (abbreviation.StartsWith("olv"))
            {
                return "Olvia";
            }
            else if (abbreviation.StartsWith("ser"))
            {
                return "Serendia";
            }
            else if (abbreviation.StartsWith("val"))
            {
                return "Valencia";
            }
            else if (abbreviation.StartsWith("vel"))
            {
                return "Velia";
            }

            return "Somewhere";

        }

        /*******************************************************
        **             CONDITIONS OF CONVERSATION             **
        *******************************************************/

        public static bool Received(MessageContext context)
        {
            return context != null;
        }

        public static bool RequestingUntag(string message)
        {
            return message.Contains("untag me");
        }

        public static bool RequestingTag(string message)
        {
            return message.Contains("tag me");
        }

        public static bool Grateful(string message)
        {
            return message.Contains("thank") || message.Contains("appreciate this") || message.Contains("appreciate it") || message.Contains("appreciate you");
        }

        public static bool WantsLink(string message)
        {
            return message.Contains("?")
                            && (message.Contains("discord link") || message.Contains("this discord") || message.Contains("link here") || message.Contains("this server")
                                || message.Contains("permanent link") || message.Contains("this channel") || message.Contains("museifu discord"));
        }

        public static bool OfferingHelp(string message)
        {
            return message.Contains("how?") || message.Contains("help");
        }

        public static bool Howdy(string message)
        {
            return message.EndsWith("?") && message.Contains("how are you");
        }

        public static bool Farewell(string message)
        {
            return message.Contains("farewell") || message.Contains("peace") || message.Contains("see ya") || message.Contains("bye") || message.Contains("later")
                                || message.Contains("seeya") || message.Contains("deuces") || message.Contains("fare well") || message.Contains("gotta go") || message.Contains("time to go")
                                || message.Contains("ciao") || message.Contains("ttyl");
        }

        public static bool Blank(string message)
        {
            return message == "";
        }

        public static bool ActivatingFieldBoss(string message)
        {
            return message.Contains(" is up on ");
        }

        public static bool DeactivatingFieldBoss(string message)
        {
            return message.EndsWith("dead") || message.EndsWith("dead.") || message.EndsWith("dead!")
                        || message.EndsWith("died") || message.EndsWith("died.") || message.EndsWith("died!");
        }

        public static bool WhatIsBell(string message)
        {
            return message.Contains("what is a bell") || message.Contains("what's a bell")
                        || message.Contains("what is a golden bell") || message.Contains("what's a golden bell")
                        || message.EndsWith("bell?") || message.EndsWith("bells?");
        }

        public static bool ActivatingBell(string message)
        {
            return message.StartsWith("bell");
        }

        public static bool DeactivatingBell(string message)
        {
            return message.StartsWith("kill bell");
        }

        public static bool SleepCommand(string message)
        {
            return message.Contains("time to sleep");
        }

        public static bool RefreshTimeZoneCode(string message)
        {
            return message.StartsWith("refresh") && message.Split(' ').Length > 1 && Schedule.Codes.Contains(message.Split(' ')[1].Replace(".", "").Replace("!", "").Replace("?", ""));
        }

        public static bool TimeZoneCode(string message)
        {

            return Schedule.Codes.Contains(message.Trim().Split(' ')[0].Replace(".", "").Replace("!", "").Replace("?", "")) && message.Trim().Split(' ').Length == 1;
        }

        public static bool RequestingRefresh(string message)
        {
            return (
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
    }
}
