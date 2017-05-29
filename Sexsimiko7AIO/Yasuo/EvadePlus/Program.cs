using EloBuddy.SDK.Events;

namespace Sexsimiko7AIO.Yasuo.EvadePlus
{
    internal static class Program
    {
        private static SkillshotDetector _skillshotDetector;
        public static EvadePlus Evade;

        public static void Eevade()
        {
            
                _skillshotDetector = new SkillshotDetector();
                Evade = new EvadePlus(_skillshotDetector);
                EvadeMenu.CreateMenu();
            

        }
    }
}
