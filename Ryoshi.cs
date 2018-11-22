using DSharpPlus;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using System.Linq;
using System.Diagnostics;
using System;

/**************************************************************************
**                               Ryoshi                                  **
**************************************************************************/

namespace Ryoshi
{
    public class Ryoshi
    {

        static DiscordClient discord;
        static CommandsNextExtension commands;
        static InteractivityExtension interactivity;

        public Ryoshi(string[] arguments)
        {
            MainAsync(arguments).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void Start()
        {
        }

        public void Stop()
        {

        }

        static async Task MainAsync(string[] arguments)
        {
            //Commander.StartGakushi(Process.Start("c:\\Users\\aisom\\Documents\\Ryoshi\\Ryoshi\\bin\\Debug\\gakushi.bat", ">NUL"));

            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = "MY DISCORD TOKEN GOES HERE... USE YOUR OWN ;P",
                TokenType = TokenType.Bot,
                AutoReconnect = true
            });

            discord.GuildMemberAdded += async data => {
                var member = data.Member;
                var role = data.Guild.Roles.FirstOrDefault(roles => roles.Name == "Tsukonin");
                await member.GrantRoleAsync(role);
            };
            
            Magician.DisappearConsole();

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new [] { "hola, ", "oi, ", "yo, ", "hey, ", "sup, ", "aloha, ",
                                            "hola ", "oi ", "yo ", "hey ", "sup ", "aloha " }
            });

            commands.RegisterCommands<Commander>();

            interactivity = discord.UseInteractivity(new InteractivityConfiguration { });

            await discord.ConnectAsync();

            await Task.Delay(-1);
        }
    }
}
