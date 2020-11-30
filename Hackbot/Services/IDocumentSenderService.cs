using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hackbot.Services
{
    /// <summary>
    /// Сервис, который позволяет отослать файл указанному пользователю
    /// </summary>
    public interface IDocumentSenderService
    {
        /// <summary>
        /// Отослать файл пользователю
        /// </summary>
        /// <param name="chatId">ID пользователя, которому нужно отослать файл</param>
        /// <param name="document">Отсылаемый документ</param>
        /// <param name="caption">Описание файла</param>
        public void SendDocument(long chatId, Stream document, string caption);
    }
}
