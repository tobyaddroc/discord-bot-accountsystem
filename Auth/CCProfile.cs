using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBotAccountSystem.Auth.Prototypes;


namespace DiscordBotAccountSystem.Auth
{
    public class CCProfile : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("profile", "Просмотреть профиль")]
        public async Task profile(SocketUser user = null)
        {
            try
            {
                CPAccount localUser = await CGlobal.DataBaseHandle.FindProfile(Context.User.Id);
                if (localUser == null)
                {
                    await RespondAsync(embed: new EmbedBuilder()
                    {
                        Title = "Вы не зарегистрированы",
                        Description = "Для регистрации отправьте команду `/register`"
                    }.WithColor(0xff0000).Build(), ephemeral: true);
                    return;
                }
                CPAccount viewUser = user == null ? localUser : await CGlobal.DataBaseHandle.FindProfile(user.Id);
                if (viewUser == null)
                {
                    await RespondAsync(embed: new EmbedBuilder()
                    {
                        Title = "Пользователь не зарегистрирован в боте"
                    }.WithColor(0xff0000).Build(), ephemeral: true);
                    return;
                }
                CPPerms localPerms = await CGlobal.DataBaseHandle.FetchPermissions(localUser.Id);
                CPPerms viewPerms = await CGlobal.DataBaseHandle.FetchPermissions(viewUser.Id);
                if (user != null && !localPerms.ViewAnotherProfiles)
                {
                    await RespondAsync(embed: new EmbedBuilder()
                    {
                        Title = "У вас недостаточно прав",
                        Description = string.Format("Для выполнения этого действия вам необходимы разрешения: \n```{0}```", nameof(localPerms.ViewAnotherProfiles))
                    }.WithColor(0xff0000).Build(), ephemeral: true);
                    return;
                }
                if (user == null)
                    user = Context.User;
                if (localUser.BanInfo != null && localUser.BanInfo.UnbannedAt > (ulong)DateTimeOffset.Now.ToUnixTimeSeconds())
                {
                    EmbedFieldBuilder[] banInfoFields = {
                        new EmbedFieldBuilder()
                        {
                            Name = "Забанен",
                            Value = string.Format("<t:{0}:f> (<t:{0}:R>)", localUser.BanInfo.BannedAt),
                            IsInline = true,
                        },
                        new EmbedFieldBuilder()
                        {
                            Name = "Будет разбанен",
                            Value = string.Format("<t:{0}:f> (<t:{0}:R>)", localUser.BanInfo.UnbannedAt),
                            IsInline = true,
                        },
                        new EmbedFieldBuilder()
                        {
                            Name = "Причина бана",
                            Value = "```" + localUser.BanInfo.Reason + "```",
                            IsInline = false
                        }
                    };
                    await RespondAsync(embed: new EmbedBuilder()
                    {
                        Title = "Ваш аккаунт заблокирован",
                        Description = "Отчет о блокировке:"
                    }.WithColor(0xff0000).WithFields(banInfoFields).Build(), ephemeral: true);
                    return;
                }
                await RespondAsync(embeds: new Embed[] {
                    new EmbedBuilder()
                    {
                        Title = $"Профиль {viewUser.Username}",
                    }.WithColor(viewPerms.Color).WithThumbnailUrl((user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()).Replace("?size=128", string.Empty)).WithFields(new EmbedFieldBuilder[] {
                        new EmbedFieldBuilder() {
                            Name = "Статус",
                            Value = viewUser.Profile.Phrase,
                            IsInline = true
                        },
                        new EmbedFieldBuilder() {
                            Name = "Проживает",
                            Value = viewUser.Profile.Location,
                            IsInline = true
                        },
                        new EmbedFieldBuilder() {
                            Name = "Группа",
                            Value = viewUser.Group == "admin" ? "Администратор" : viewUser.Group == "premium" ? "Премиум" : viewUser.Group == "user" ? "Пользователь" : "Нет группы",
                            IsInline = true
                        },
                        new EmbedFieldBuilder() {
                            Name = "Зарегистрировался",
                            Value = $"<t:{viewUser.RegisteredAt}:f>",
                            IsInline = true
                        },
                        new EmbedFieldBuilder() {
                            Name = "ID",
                            Value = $"{viewUser.Id}",
                            IsInline = true
                        },
                    }).Build(),
                    viewUser.BanInfo != null && viewUser.BanInfo.UnbannedAt > (ulong)DateTimeOffset.Now.ToUnixTimeSeconds() ? new EmbedBuilder()
                    {
                        Title = "Заблокирован",
                        Description = "Не стоит проводить сделки с этим пользователем до окончания блокировки"
                    }.WithColor(0xff0000).WithFields(new EmbedFieldBuilder()
                        {
                            Name = "Забанен",
                            Value = string.Format("<t:{0}:f> (<t:{0}:R>)", viewUser.BanInfo.BannedAt),
                            IsInline = true,
                        },
                        new EmbedFieldBuilder()
                        {
                            Name = "Будет разбанен",
                            Value = string.Format("<t:{0}:f> (<t:{0}:R>)", viewUser.BanInfo.UnbannedAt),
                            IsInline = true,
                        },
                        new EmbedFieldBuilder()
                        {
                            Name = "Причина бана",
                            Value = "```" + viewUser.BanInfo.Reason + "```",
                            IsInline = false
                        }).Build() : new EmbedBuilder()
                        {
                            Description = "Ограничений нет"
                        }.WithColor(viewPerms.Color).Build()
                }, ephemeral: true);

            }
            catch (NullReferenceException ex)
            {
                await RespondAsync(embed: new Discord.EmbedBuilder()
                {
                    Title = "Ошибка базы данных",
                    Description = $"Бот не может получить доступ к базе данных, скорее всего это произошло по следующим причинам:\n1. Отсутствует файл базы данных\n2. Отсутствует файл настроек локальных политик\n3. Доступ к файлам базы данных затруднен"
                }.WithColor(0xff0000).Build(), ephemeral: true);
                throw;
            }
            catch (Exception ex)
            {
                long crashTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                string dumpFileName = $"except_{crashTimestamp}_{nameof(this.profile)}_{Context.User.Id}.log";
                File.WriteAllText(dumpFileName, ex.ToString());
                await RespondAsync(embed: new EmbedBuilder()
                {
                    Title = "Ошибка на стороне бота",
                    Description = $"Полный отчет был сохранен в файл {dumpFileName}\nКраткий отчет об ошибке:\n```{ex.Message}```"
                }.WithColor(0xff0000).Build(), ephemeral: true);
                throw;
            }
        }
    }
}
