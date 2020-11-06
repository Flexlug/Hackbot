using Hackbot.Structures;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hackbot.Scenes
{
    // TODO Реализовать меню управления командой
    /// <summary>
    /// Меню управления командой от лица командира
    /// </summary>
    public class CaptainGuildEditScene : Scene
    {
        public CaptainGuildEditScene()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }

        public override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            throw new NotImplementedException();
        }

        // TODO Редактировать имя команды
        // TODO Редактировать описание команды
        // TODO Исключить участника
        // TODO Просмотреть заявки
    }
}
