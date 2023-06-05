using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBotAccountSystem.Auth.Prototypes;

namespace DiscordBotAccountSystem.Auth
{
    public class CCAdmin : InteractionModuleBase<SocketInteractionContext>
    {
        #region Reboot
        public async Task adminmenu_reboot_delegate(SocketModal modal)
        {
            List<SocketMessageComponentData> components = modal.Data.Components.ToList();
            string adminmenu_modal_reboot_confirmation = components.First(x => x.CustomId == "adminmenu_modal_reboot_confirmation").Value;
            if (adminmenu_modal_reboot_confirmation != modal.User.Username)
            {
                await modal.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                    {
                        Title = "Никнеймы не совпадают"
                    }.WithColor(0xff0000).Build();
                });
                Context.Client.ModalSubmitted -= adminmenu_reboot_delegate;
                return;
            }
            modal.UpdateAsync(x =>
            {
                x.Embed = new EmbedBuilder()
                {
                    Title = "Выполняется перезапуск"
                }.WithColor(0x03d600).Build();
                x.Components = null;
            }).Wait();
            Process currentProcess = Process.GetCurrentProcess();
            Process.Start(new ProcessStartInfo()
            {
                FileName = currentProcess.MainModule.FileName,
                UseShellExecute = true
            });
            Context.Client.ModalSubmitted -= adminmenu_reboot_delegate;
            currentProcess.Kill();
        }
        [ComponentInteraction("adminmenu_reboot")]
        public async Task adminmenu_reboot()
        {
            CPPerms userPerms = await CGlobal.DataBaseHandle.FetchPermissions(Context.User.Id);
            if (userPerms != null && !userPerms.AdminMenu)
            {
                await RespondAsync(embed: new EmbedBuilder()
                {
                    Title = "Ошибка использования",
                    Description = "Меню аннулировано в связи с отсутствием у вас права " + nameof(userPerms.AdminMenu)
                }.WithColor(0xff0000).Build(), ephemeral: true);
                return;
            }
            Context.Client.ModalSubmitted += adminmenu_reboot_delegate;
            await RespondWithModalAsync(new ModalBuilder()
            {
                CustomId = "adminmenu_modal_reboot",
                Title = "Перезагрузить бота"
            }.AddTextInput(new TextInputBuilder()
            {
                CustomId = "adminmenu_modal_reboot_confirmation",
                Label = "Подтвердите перезапуск бота",
                Required = true,
                MinLength = Context.User.Username.Length,
                MaxLength = Context.User.Username.Length,
                Placeholder = "Введите свой никнейм (не аккаунта в боте)",
                Style = TextInputStyle.Short
            }).Build());
        }
        #endregion
        #region Change group
        public async Task adminmenu_changegroup_delegate(SocketModal modal)
        {
            List<SocketMessageComponentData> components = modal.Data.Components.ToList();
            ulong adminmenu_modal_changegroup_changegroupId = 0;
            string adminmenu_modal_changegroup_group = components.Find(x => x.CustomId == "adminmenu_modal_changegroup_group").Value;
            try { adminmenu_modal_changegroup_changegroupId = ulong.Parse(components.Find(x => x.CustomId == "adminmenu_modal_changegroup_changegroupId").Value); }
            catch
            {
                await modal.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                    {
                        Title = "Неверный аргумент",
                        Description = $"Аргумент {nameof(adminmenu_modal_changegroup_changegroupId)} выражен как String. Требуется: UInt64"
                    }.WithColor(0xff0000).Build();
                    x.Components = null;
                });
                Context.Client.ModalSubmitted -= adminmenu_changegroup_delegate;
                return;
            }
            CPAccount user = await CGlobal.DataBaseHandle.FindProfile(adminmenu_modal_changegroup_changegroupId);
            CPAccount localuser = await CGlobal.DataBaseHandle.FindProfile(modal.User.Id);
            if (user == null)
            {
                await modal.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                    {
                        Title = "Пользователя нет в базе данных бота"
                    }.WithColor(0xff0000).Build();
                    x.Components = null;
                });
                Context.Client.ModalSubmitted -= adminmenu_changegroup_delegate;
                return;
            }
            if (adminmenu_modal_changegroup_group == "admin" && !localuser.Immunity)
            {
                await modal.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                    {
                        Title = "Не удалось изменить группу пользователя",
                        Description = "Для установки группы admin нужен флаг \"Маршал\""
                    }.WithColor(0xff0000).Build();
                    x.Components = null;
                });
                Context.Client.ModalSubmitted -= adminmenu_changegroup_delegate;
                return;
            }
            bool isChanged = await CGlobal.DataBaseHandle.ChangeGroup(user.Id, modal.User.Id, adminmenu_modal_changegroup_group);
            if (adminmenu_modal_changegroup_group == user.Group)
                isChanged = false;
            if (user.Id == modal.User.Id)
                isChanged = false;
            if (!isChanged)
            {
                await modal.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                    {
                        Title = "Не удалось изменить группу пользователя",
                        Description = "Это могло произойти по следующим причинам:\n" +
                        "1. Пользователь имеет флаг \"Маршал\"\n" +
                        "2. Ошибка на стороне бота\n" +
                        "3. Вы пытаетесь установить группу самому себе\n" +
                        "4. Пользователь уже в этой группе"
                    }.WithColor(0xff0000).Build();
                    x.Components = null;
                });
                Context.Client.ModalSubmitted -= adminmenu_changegroup_delegate;
                return;
            }
            await modal.UpdateAsync(x =>
            {
                x.Embed = new EmbedBuilder()
                {
                    Title = "Группа пользователя изменена"
                }.WithFields(new EmbedFieldBuilder()
                {
                    Name = "Было:",
                    Value = "```" + user.Group + "```",
                    IsInline = false
                },
                new EmbedFieldBuilder()
                {
                    Name = "Стало:",
                    Value = "```" + adminmenu_modal_changegroup_group + "```",
                    IsInline = false
                }).WithColor(0x03d600).Build();
                x.Components = null;
            });
            Context.Client.ModalSubmitted -= adminmenu_changegroup_delegate;
        }
        [ComponentInteraction("adminmenu_changegroup")]
        public async Task adminmenu_changegroup()
        {
            CPPerms userPerms = await CGlobal.DataBaseHandle.FetchPermissions(Context.User.Id);
            if (userPerms != null && !userPerms.AdminMenu)
            {
                await RespondAsync(embed: new EmbedBuilder()
                {
                    Title = "Ошибка использования",
                    Description = "Меню аннулировано в связи с отсутствием у вас права " + nameof(userPerms.AdminMenu)
                }.WithColor(0xff0000).Build(), ephemeral: true);
                return;
            }
            Context.Client.ModalSubmitted += adminmenu_changegroup_delegate;
            await RespondWithModalAsync(new ModalBuilder()
            {
                Title = "Изменить группу",
                CustomId = "adminmenu_modal_changegroup"
            }.AddTextInput(new TextInputBuilder()
            {
                CustomId = "adminmenu_modal_changegroup_changegroupId",
                Label = "ID пользователя",
                Placeholder = "Укажите ID пользователя, группу которого нужно изменить",
                Style = TextInputStyle.Short,
                Required = true
            })
             .AddTextInput(new TextInputBuilder()
             {
                 CustomId = "adminmenu_modal_changegroup_group",
                 Label = "Группа",
                 Placeholder = "Группа, которую нужно задать пользователю",
                 Style = TextInputStyle.Short,
                 Required = true,
                 Value = "user"
             }).Build());
        }
        #endregion
        #region Anonymous message
        public async Task adminmenu_anonymousmsg_delegate(SocketModal modal)
        {
            List<SocketMessageComponentData> components = modal.Data.Components.ToList();
            ulong adminmenu_modal_anonymousmsg_channelid = 0;
            string adminmenu_modal_anonymousmsg_msg = components.First(x => x.CustomId == "adminmenu_modal_anonymousmsg_msg").Value;
            try { adminmenu_modal_anonymousmsg_channelid = ulong.Parse(components.First(x => x.CustomId == "adminmenu_modal_anonymousmsg_channelid").Value); }
            catch
            {
                await modal.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                    {
                        Title = "Неверный аргумент",
                        Description = $"Аргумент {nameof(adminmenu_modal_anonymousmsg_channelid)} выражен как String. Требуется: UInt64"
                    }.WithColor(0xff0000).Build();
                    x.Components = null;
                });
                Context.Client.ModalSubmitted -= adminmenu_anonymousmsg_delegate;
                return;
            }
            Context.Client.ModalSubmitted -= adminmenu_anonymousmsg_delegate;
            try
            {
                IMessageChannel channel = await modal.GetChannelAsync();
                await channel.SendMessageAsync(adminmenu_modal_anonymousmsg_msg);
            }
            catch (Exception ex)
            {
                await modal.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                    {
                        Title = "Невозможно отправить сообщение",
                        Description = $"```{ex.Message}```"
                    }.WithColor(0xff0000).Build();
                    x.Components = null;
                });
                return;
            }
            await modal.UpdateAsync(x =>
            {
                x.Embed = new EmbedBuilder()
                {
                    Title = "Сообщение отправлено",
                    Description = "```" + adminmenu_modal_anonymousmsg_msg + "```\n" +
                    $"Отправлено в <#{adminmenu_modal_anonymousmsg_channelid}>"
                }.WithColor(0x03d600).Build();
                x.Components = null;
            });
        }
        [ComponentInteraction("adminmenu_anonymousmsg")]
        public async Task adminmenu_anonymousmsg()
        {
            CPPerms userPerms = await CGlobal.DataBaseHandle.FetchPermissions(Context.User.Id);
            if (userPerms != null && !userPerms.AdminMenu)
            {
                await RespondAsync(embed: new EmbedBuilder()
                {
                    Title = "Ошибка использования",
                    Description = "Меню аннулировано в связи с отсутствием у вас права " + nameof(userPerms.AdminMenu)
                }.WithColor(0xff0000).Build(), ephemeral: true);
                return;
            }
            Context.Client.ModalSubmitted += adminmenu_anonymousmsg_delegate;
            await RespondWithModalAsync(new ModalBuilder()
            {
                Title = "Анонимное сообщение",
                CustomId = "adminmenu_modal_anonymousmsg"
            }.AddTextInput(new TextInputBuilder()
            {
                CustomId = "adminmenu_modal_anonymousmsg_channelid",
                Label = "ID канала",
                MinLength = 18,
                Placeholder = "Канал для отправки сообщения",
                Required = true,
                Style = TextInputStyle.Short
            })
            .AddTextInput(new TextInputBuilder()
            {
                CustomId = "adminmenu_modal_anonymousmsg_msg",
                Label = "Сообщение",
                Required = true,
                Style = TextInputStyle.Paragraph
            }).Build());
        }
        #endregion
        #region Unban
        private async Task adminmenu_unban_delegate(SocketModal modal)
        {
            List<SocketMessageComponentData> components = modal.Data.Components.ToList();
            ulong adminmenu_modal_unban_banId = 0;
            try { adminmenu_modal_unban_banId = ulong.Parse(components.Find(x => x.CustomId == "adminmenu_modal_unban_banId").Value); }
            catch
            {
                await modal.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                    {
                        Title = "Неверный аргумент",
                        Description = $"Аргумент {nameof(adminmenu_modal_unban_banId)} выражен как String. Требуется: UInt64"
                    }.WithColor(0xff0000).Build();
                    x.Components = null;
                });
                Context.Client.ModalSubmitted -= adminmenu_unban_delegate;
                return;
            }
            CPAccount user = await CGlobal.DataBaseHandle.FindProfile(adminmenu_modal_unban_banId);
            if (user == null)
            {
                await modal.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                    {
                        Title = "Пользователя нет в базе данных бота"
                    }.WithColor(0xff0000).Build();
                    x.Components = null;
                });
                Context.Client.ModalSubmitted -= adminmenu_unban_delegate;
                return;
            }
            bool isUnbanned = await CGlobal.DataBaseHandle.UnbanProfile(user.Id);
            if (!isUnbanned)
            {
                await modal.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                    {
                        Title = "Не удалось разбанить пользователя",
                        Description = "Это могло произойти по следующим причинам:\n" +
                        "1. Пользователь не найден\n" +
                        "2. Пользователь не забанен\n" +
                        "3. Ошибка на стороне бота"
                    }.WithColor(0xff0000).Build();
                    x.Components = null;
                });
                Context.Client.ModalSubmitted -= adminmenu_unban_delegate;
                return;
            }
            await modal.UpdateAsync(x =>
            {
                x.Embed = new EmbedBuilder()
                {
                    Title = $"С пользователя {user.Username} сняты все ограничения"
                }.WithColor(0x03d600).Build();
                x.Components = null;
            });
            Context.Client.ModalSubmitted -= adminmenu_unban_delegate;
        }
        [ComponentInteraction("adminmenu_unban")]
        public async Task adminmenu_unban()
        {
            CPPerms userPerms = await CGlobal.DataBaseHandle.FetchPermissions(Context.User.Id);
            if (userPerms != null && !userPerms.AdminMenu)
            {
                await RespondAsync(embed: new EmbedBuilder()
                {
                    Title = "Ошибка использования",
                    Description = "Меню аннулировано в связи с отсутствием у вас права " + nameof(userPerms.AdminMenu)
                }.WithColor(0xff0000).Build(), ephemeral: true);
                return;
            }
            Context.Client.ModalSubmitted += adminmenu_unban_delegate;
            await RespondWithModalAsync(new ModalBuilder()
            {
                Title = "Разбанить",
                CustomId = "adminmenu_modal_unban"
            }.AddTextInput(new TextInputBuilder()
            {
                CustomId = "adminmenu_modal_unban_banId",
                Label = "ID пользователя",
                Placeholder = "Укажите ID пользователя, которого нужно разбанить",
                Style = TextInputStyle.Short,
                Required = true
            }).Build());
        }
        #endregion
        #region Ban
        public async Task adminmenu_ban_delegate(SocketModal modal)
        {
            List<SocketMessageComponentData> components = modal.Data.Components.ToList();
            ulong adminmenu_modal_ban_banLength = 0;
            ulong adminmenu_modal_ban_banId = 0;
            string adminmenu_modal_ban_banReason = components.First(x => x.CustomId == "adminmenu_modal_ban_banReason").Value;
            try { adminmenu_modal_ban_banLength = ulong.Parse(components.Find(x => x.CustomId == "adminmenu_modal_ban_banLength").Value); }
            catch
            {
                await modal.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                    {
                        Title = "Неверный аргумент",
                        Description = $"Аргумент {nameof(adminmenu_modal_ban_banLength)} выражен как String. Требуется: Int32"
                    }.WithColor(0xff0000).Build();
                    x.Components = null;
                });
                Context.Client.ModalSubmitted -= adminmenu_ban_delegate;
                return;
            }
            try { adminmenu_modal_ban_banId = ulong.Parse(components.Find(x => x.CustomId == "adminmenu_modal_ban_banId").Value); }
            catch
            {
                await modal.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                    {
                        Title = "Неверный аргумент",
                        Description = $"Аргумент {nameof(adminmenu_modal_ban_banId)} выражен как String. Требуется: Int32"
                    }.Build();
                    x.Components = null;
                });
                Context.Client.ModalSubmitted -= adminmenu_ban_delegate;
                return;
            }
            CPAccount user = await CGlobal.DataBaseHandle.FindProfile(adminmenu_modal_ban_banId);
            if (user == null)
            {
                await modal.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                    {
                        Title = "Пользователя нет в базе данных бота"
                    }.Build();
                    x.Components = null;
                });
                Context.Client.ModalSubmitted -= adminmenu_ban_delegate;
                return;
            }
            bool isBanned = await CGlobal.DataBaseHandle.BanProfile(user.Id, modal.User.Id, adminmenu_modal_ban_banLength, adminmenu_modal_ban_banReason);
            if (!isBanned)
            {
                await modal.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                    {
                        Title = "Не удалось забанить пользователя",
                        Description = "Это могло произойти по следующим причинам:\n" +
                        "1. Пользователь не найден\n" +
                        "2. Пользователь имеет флаг \"Маршал\"\n" +
                        "3. Вы пытаетесь забанить самого себя\n" +
                        "4. Ошибка на стороне бота"
                    }.WithColor(0xff0000).Build();
                    x.Components = null;
                });
                Context.Client.ModalSubmitted -= adminmenu_ban_delegate;
                return;
            }
            EmbedFieldBuilder[] ubanInfoFields = {
                        new EmbedFieldBuilder()
                        {
                            Name = "Забанен",
                            Value = string.Format("<t:{0}:f> (<t:{0}:R>)", DateTimeOffset.Now.ToUnixTimeSeconds()),
                            IsInline = true,
                        },
                        new EmbedFieldBuilder()
                        {
                            Name = "Будет разбанен",
                            Value = string.Format("<t:{0}:f> (<t:{0}:R>)", (ulong)DateTimeOffset.Now.ToUnixTimeSeconds() + (ulong)(adminmenu_modal_ban_banLength == 0 ? 2473986640 - (ulong)DateTimeOffset.Now.ToUnixTimeSeconds() : adminmenu_modal_ban_banLength)),
                            IsInline = true,
                        },
                        new EmbedFieldBuilder()
                        {
                            Name = "Причина бана",
                            Value = "```" + adminmenu_modal_ban_banReason + "```",
                            IsInline = false
                        }
                    };
            await modal.UpdateAsync(x =>
            {
                x.Embed = new EmbedBuilder()
                {
                    Title = $"На пользователя {user.Username} наложены ограничения",
                    Description = "Отчёт о его блокировке:"
                }.WithColor(0x03d600).WithFields(ubanInfoFields).Build();
                x.Components = null;
            });
            Context.Client.ModalSubmitted -= adminmenu_ban_delegate;
        }
        [ComponentInteraction("adminmenu_ban")]
        public async Task adminmenu_ban()
        {
            CPPerms userPerms = await CGlobal.DataBaseHandle.FetchPermissions(Context.User.Id);
            if (userPerms != null && !userPerms.AdminMenu)
            {
                await RespondAsync(embed: new EmbedBuilder()
                {
                    Title = "Ошибка использования",
                    Description = "Меню аннулировано в связи с отсутствием у вас права " + nameof(userPerms.AdminMenu)
                }.WithColor(0xff0000).Build(), ephemeral: true);
                return;
            }
            Context.Client.ModalSubmitted += adminmenu_ban_delegate;
            await RespondWithModalAsync(new ModalBuilder()
            {
                Title = "Забанить",
                CustomId = "adminmenu_modal_ban"
            }.AddTextInput(new TextInputBuilder()
            {
                CustomId = "adminmenu_modal_ban_banId",
                Label = "ID пользователя",
                Placeholder = "Укажите ID пользователя, которого нужно забанить",
                Style = TextInputStyle.Short,
                Required = true
            })
            .AddTextInput(new TextInputBuilder()
            {
                CustomId = "adminmenu_modal_ban_banLength",
                Label = "Время бана",
                Placeholder = "Укажите время бана (в секундах)",
                Style = TextInputStyle.Short,
                Required = true,
                Value = "0"
            })
             .AddTextInput(new TextInputBuilder()
             {
                 CustomId = "adminmenu_modal_ban_banReason",
                 Label = "Причина бана",
                 Placeholder = "Укажите причину бана",
                 Style = TextInputStyle.Paragraph,
                 Value = "No reason",
                 Required = true
             }).Build());
        }
        #endregion

        [SlashCommand("admin", "Bot admin menu")]
        public async Task admin()
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
            CPPerms localPerms = await CGlobal.DataBaseHandle.FetchPermissions(localUser.Id);
            if (!localPerms.AdminMenu)
            {
                await RespondAsync(embed: new EmbedBuilder()
                {
                    Title = "У вас недостаточно прав",
                    Description = string.Format("Для выполнения этого действия вам необходимы разрешения: \n```{0}```", nameof(localPerms.AdminMenu))
                }.WithColor(0xff0000).Build(), ephemeral: true);
                return;
            }

            ButtonBuilder[] banButton = {
                new ButtonBuilder()
                {
                    CustomId = "adminmenu_ban",
                    Style = ButtonStyle.Danger,
                    Label = "Забанить"
                },
                new ButtonBuilder()
                {
                    CustomId = "adminmenu_unban",
                    Style = ButtonStyle.Success,
                    Label = "Разбанить"
                },
                new ButtonBuilder()
                {
                    CustomId = "adminmenu_changegroup",
                    Style = ButtonStyle.Primary,
                    Label = "Изменить группу"
                },
                new ButtonBuilder()
                {
                    CustomId = "adminmenu_anonymousmsg",
                    Style = ButtonStyle.Secondary,
                    Label = "Анонимное сообщение"
                },
                new ButtonBuilder()
                {
                    CustomId = "adminmenu_reboot",
                    Style = ButtonStyle.Secondary,
                    Label = "Перезагрузить бота",
                    IsDisabled = !localUser.Immunity
                }
            };
            var components = new ComponentBuilder();
            foreach (var button in banButton)
                components.WithButton(button);

            await RespondAsync(components: components.Build(), ephemeral: true);

        }
    }
}
