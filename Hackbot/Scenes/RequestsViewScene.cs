using Hackbot.Structures;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hackbot.Scenes
{
    // TODO Реализовать меню просмотра поданых заявок на вступление в команды
    /// <summary>
    /// Меню просмотра поданых заявок на вступление в команды
    /// </summary>
    public class RequestsViewScene : Scene
    {
        public RequestsViewScene()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }

        public override Task<SceneResult> GetResult(RecievedMessage ans)
        {
            throw new NotImplementedException();
        }

        // TODO Удалить выбранную заявку
        // TODO Редактировать выбранную заявку
    }
}
