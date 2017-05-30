using EloBuddy;

namespace Sexsimiko7AIO.Yasuo.TargetedSpells
{

    // Credits to Dektus for Spells/SpellClass
    
    public class TargetSpellData
    {
        public string ChampionName { get; set; }
        public SpellSlot Spellslot { get; set; }
        public string Name { get; set; }
        public int Delay { get; set; }

        public TargetSpellData(string champ, string spellname, SpellSlot slot, int delay = 0)
        {
            ChampionName = champ;
            Name = spellname;
            Spellslot = slot;
            Delay = delay;
        }
    }
}
