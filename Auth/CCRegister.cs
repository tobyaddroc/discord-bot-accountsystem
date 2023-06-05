using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using DiscordBotAccountSystem.Auth.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotAccountSystem.Auth
{
    public class CMRegisterModal : IModal
    {
        public string Title => "Ваш никнейм";
        [InputLabel("Введите никнейм")]
        [ModalTextInput("register_modal", TextInputStyle.Short, "Никнейм будет отображаться в вашем профиле", maxLength: 20)]
        public string RegisterUserName { get; set; }
    }

    public class CCRegister : InteractionModuleBase<SocketInteractionContext>
    {
        [ComponentInteraction("yes")]
        public async Task HandleRegisterInput()
        {
            Context.Client.ModalSubmitted += async modal =>
            {
                try
                {
                    List<SocketMessageComponentData> components = modal.Data.Components.ToList();
                    string register_modal_text = components.First(x => x.CustomId == "register_modal_text").Value;
                    CPAccount check2 = await CGlobal.DataBaseHandle.FindProfile(register_modal_text);
                    if (check2 == null)
                    {
                        CPAccount newAccount = new CPAccount()
                        {
                            Username = register_modal_text,
                            Id = modal.User.Id,
                            Group = "user",
                            Immunity = false,
                            Profile = new CPProfile()
                            {
                                Phrase = "Новичок",
                                Location = Context.Guild.Name
                            },
                            RegisteredAt = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds(),
                            BanInfo = null
                        };
                        await CGlobal.DataBaseHandle.CreateProfile(newAccount);
                        await modal.UpdateAsync(x =>
                        {
                            x.Embed = new EmbedBuilder()
                            {
                                Title = "Вы зарегистрировались в боте как " + newAccount.Username,
                                Description = "Теперь вы можете пользоваться ботом полноценно"
                            }.WithColor(0x03d600).Build();
                            x.Components = null;
                        });
                        try
                        {
                            await Context.Guild.GetUser(Context.User.Id).ModifyAsync(x => x.Nickname = newAccount.Username);
                        }
                        catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                    }
                    else
                    {
                        await modal.UpdateAsync(x =>
                        {
                            x.Embed = new EmbedBuilder()
                            {
                                Title = "Пользователь " + register_modal_text + " уже существует"
                            }.WithColor(0xff0000).Build();
                            x.Components = null;
                        });
                    }
                }
                catch { }
            };
            await RespondWithModalAsync(new ModalBuilder()
            {
                Title = "Регистрация",
                CustomId = "register_modal"
            }.AddTextInput(new TextInputBuilder()
            {
                MinLength = 5,
                MaxLength = 20,
                Required = true,
                Style = TextInputStyle.Short,
                CustomId = "register_modal_text",
                Label = "Ваш никнейм",
                Placeholder = "Никнейм будет отображаться в вашем профиле"
            }).Build());
        }

        [SlashCommand("register", "Зарегистрироваться в боте")]
        public async Task register()
        {
            CPAccount localUser = await CGlobal.DataBaseHandle.FindProfile(Context.User.Id);
            if (localUser != null)
            {
                await RespondAsync(embed: new EmbedBuilder()
                {
                    Title = "У вас уже имеется аккаунт"
                }.WithColor(0xff0000).Build(), ephemeral: true);
                return;
            }
            ButtonBuilder yesbutton = new ButtonBuilder()
            {
                Label = "Пройти регистрацию",
                CustomId = "yes",
                Style = ButtonStyle.Success
            };
            var component = new ComponentBuilder();
            component.WithButton(yesbutton);

            await RespondAsync(embed: new EmbedBuilder()
            {
                Title = "Регистрация",
                Description =
                "Вы проходите процедуру регистрации, обратите внимание на следующие пункты:\n" +
                "1. Вы не можете изменить никнейм после регистрации, для этого нужно обратиться к администратору.\n" +
                "2. Аккаунт нельзя удалить после регистрации\n" +
                "Если вы согласны с данными условиями, нажмите Пройти регистрацию\n" +
                "Если не согласны, удалите этот промпт"
            }.WithColor(0xff8f00).Build(), components: component.Build(), ephemeral: true);
        }

    }
}
