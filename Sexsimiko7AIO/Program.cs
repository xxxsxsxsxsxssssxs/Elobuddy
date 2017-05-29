using EloBuddy;
using EloBuddy.SDK.Events;

namespace Sexsimiko7AIO
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(System.EventArgs args)
        {
            if (Player.Instance.Hero == Champion.Yasuo)
            {
                new Yasuo.YasuoLoad();
            }
            else if (Player.Instance.Hero == Champion.Lucian)
            {
                
            }
            else
            {
                Chat.Print("CHAMPION NOT SUPPORTED");
                Chat.Print("<b>Supported Champions: Yasuo, Lucian.</b>");
            }
        }
    }
}
