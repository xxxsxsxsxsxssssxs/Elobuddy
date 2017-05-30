using EloBuddy.SDK.Events;

namespace Sexsimiko7AIO.Yasuo.EvadePlus
{
    internal static class Program
    {
        private static SkillshotDetector _skillshotDetector;
        public static Sexsimiko7AIO.Yasuo.EvadePlus.EvadePlus Evade;

        public static void EvadeMain()
        {
            Loading.OnLoadingComplete += delegate
            {
                _skillshotDetector = new SkillshotDetector();
                Evade = new Sexsimiko7AIO.Yasuo.EvadePlus.EvadePlus(_skillshotDetector);
                EvadeMenu.CreateMenu();
            };

        }
    }
}
