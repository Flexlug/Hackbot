using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Telegram.Bot;
using NLog;

namespace Hackbot.Services.Implementations
{
    public class DocumentSenderService : IDocumentSenderService
    {
        private ITelegramBotClient client;
        private ILogger logger = LogManager.GetCurrentClassLogger();

        public async void SendDocument(long chatId, Stream document, string caption)
        {
            try
            {
                await client.SendDocumentAsync(chatId, new Telegram.Bot.Types.InputFiles.InputOnlineFile(document, caption), caption);
            }
            catch(Exception e)
            {
                logger.Error($"Couldn't send file to {chatId}. Caption: {document}. Document is {(document == null ? "NULL": "NOT NULL")}. Exception: {e}");
            }
        }


        #region Singleton

        private static DocumentSenderService _instance = null;

        public static void Initialize(ITelegramBotClient client)
        {
            _instance = new DocumentSenderService(client);
        }

        public static DocumentSenderService GetInstance()
        {
            return _instance;
        }

        private DocumentSenderService(ITelegramBotClient client)
        {
            this.client = client;
        }

        #endregion
    }
}
