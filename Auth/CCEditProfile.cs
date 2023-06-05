using Discord;
using Discord.Interactions;
using DiscordBotAccountSystem.Auth.Prototypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotAccountSystem.Auth
{
    public class CCEditProfile : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("editprofile", "Edit profile")]
        public async Task editprofile(string location = null, string phrase = null)
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
                if (location == null && phrase == null)
                {
                    await RespondAsync(embed: new EmbedBuilder()
                    {
                        Title = "Вы не можете вызвать эту команду без аргументов",
                        Description = "Выберите тот элемент профиля в аргументах команды, который вы хотите поменять"
                    }.WithColor(0xff0000).Build(), ephemeral: true);
                    return;
                }
                bool isChanged = await CGlobal.DataBaseHandle.EditProfile(Context.User.Id, new CPProfile()
                {
                    Location = location ?? localUser.Profile.Location,
                    Phrase = phrase ?? localUser.Profile.Phrase,
                });
                if (isChanged)
                {
                    await RespondAsync(embed: new EmbedBuilder()
                    {
                        Title = "Профиль успешно обновлен"
                    }.WithColor(0x03d600).Build(), ephemeral: true);
                }
                else
                {
                    await RespondAsync(embed: new EmbedBuilder()
                    {
                        Title = "Неизвестная ошибка"
                    }.WithColor(0xff0000).Build(), ephemeral: true);
                }
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
                string dumpFileName = $"except_{crashTimestamp}_{nameof(this.editprofile)}_{Context.User.Id}.log";
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
