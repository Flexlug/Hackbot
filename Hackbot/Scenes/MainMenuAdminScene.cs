﻿using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Hackbot.Scenes;
using Hackbot.Services;
using Hackbot.Services.DbContexts;
using Hackbot.Services.Implementations;
using Hackbot.Structures;

using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using NLog;

namespace Hackbot.Scenes
{
    /// <summary>
    /// Главное меню, которое доступно только администраторам
    /// </summary>
    public class MainMenuAdminScene : Scene
    {
        /// <summary>
        /// Логгер для данного класса
        /// </summary>
        private Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Сервис для проверки наличия пользователя в БД администраторов
        /// </summary>
        IAdminCredentialsService credentials;

        public MainMenuAdminScene()
        {
            credentials = AdminCredentialsService.GetInstance();

            ReplyKeyboard = new InlineKeyboardMarkup(
                new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Зарегистрировать команду", "")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Поиск команды", "")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("ADMIN", "")
                    }
                });
        }

        public async override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            // проверим наличие id пользователя в БД админов
            if (!await credentials.CheckAsync(ans.From.Id))
            {
                logger.Warn($"Login as admin attempt: {ans.From.Id} refused");
                return MainMenu("Доступ запрещен");
            }

            logger.Warn($"Login as admin attempt: {ans.From.Id} granted");
            return Respond("Доступ разрешен");
        }
    }
}
