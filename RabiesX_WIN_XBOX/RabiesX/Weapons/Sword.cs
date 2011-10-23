using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabiesX
{
    public class Sword
    {
        public int MaximumDurability {get; set;}
        public int Durability { get; set; }
        public bool Broken { get; set; }

        public Sword() { }

        public Sword(int maximum)
        {
            MaximumDurability = maximum;
            Durability = MaximumDurability;
            Broken = false;
        }

        public void Boost(int boost)
        {
            MaximumDurability += boost;
        }

        public void Repair()
        {
            Durability = MaximumDurability;
        }

        public void WearOut(int damage)
        {
            Durability -= damage;
            if (Durability == 0)
                Broken = true;
        }
    }
}
