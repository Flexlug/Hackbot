using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Hackbot.Controllers;
using Hackbot.Structures;
using NLog;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hackbot
{
    /// <summary>
    /// Главный исполняемый класс бота
    /// </summary>
    class Bot
    {
        /// <summary>
        /// Токен бота
        /// </summary>
        private readonly string _token;

        /// <summary>
        /// Клиент бота
        /// </summary>
        private ITelegramBotClient botClient;

        /// <summary>
        /// Контроллер сцен и диалогов
        /// </summary>
        private SceneController sceneController = new SceneController();

        private Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Инициализировать исполняемый класс с ботом
        /// </summary>
        /// <param name="_settings">Параметры запуска бота</param>
        public Bot(Settings _settings)
        {
            _token = _settings.Token;
        }

        /// <summary>
        /// Запустить бота
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            botClient = new TelegramBotClient(_token);

            User self = await botClient.GetMeAsync();
            logger.Info($"Bot started! {self.Id} {self.FirstName}");

            botClient.OnMessage += BotClient_OnMessage;
            botClient.OnCallbackQuery += BotClient_OnCallbackQuery;
            botClient.StartReceiving();
        }

        /// <summary>
        /// Обработка Inline кнопок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BotClient_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            logger.Debug($"Got inline message: data: {e.CallbackQuery.Data}, text: {e.CallbackQuery.Message.Text}");
            await ComputeRecievedMessage(new RecievedMessage()
            {
                InlineData = e.CallbackQuery.Data,
                Chat = e.CallbackQuery.Message.Chat,
                Text = e.CallbackQuery.Message.Text,
                From = e.CallbackQuery.From
            });
        }

        /// <summary>
        /// Вызывается при получении нового сообщения, которое не является результатом работы Inline кнопки
        /// </summary>
        /// <param name="sender">Вызывающий объект</param>
        /// <param name="e">Параметры события</param>
        private async void BotClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            switch (e.Message.Type)
            {
                case MessageType.Text:
                    logger.Debug($"Got text message: {e.Message.Text}");

                    await ComputeRecievedMessage(new RecievedMessage()
                    {
                        InlineData = string.Empty,
                        Chat = e.Message.Chat,
                        Text = e.Message.Text,
                        From = e.Message.From,
                    });
                    break;

                default:
                    await botClient.SendTextMessageAsync(e.Message.Chat, "Поддерживаются только текстовые сообщения.");
                    break;
            }
        }

        /// <summary>
        /// Обработать полученное от пользователя сообщение
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private async Task ComputeRecievedMessage(RecievedMessage msg)
        {
            SceneResult res = await sceneController.GenerateResult(msg);
            await botClient.SendTextMessageAsync(msg.Chat.Id, res.Answer, replyMarkup: res.KeyboardMarkup);
        }
    }
}
