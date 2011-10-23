using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabiesX
{
    public class PlasmaGun
    {
        public int MaximumPlasma { get; set; }
        public int Plasma { get; set; }
        public bool Empty { get; set; }

        public PlasmaGun() { }

        public PlasmaGun(int maximum)
        {
            MaximumPlasma = maximum;
            Plasma = MaximumPlasma;
            Empty = false;
        }

        public void Boost(int boost)
        {
            MaximumPlasma += boost;
        }

        public void Recharge()
        {
            Plasma = MaximumPlasma;
        }

        public void Waste(int plasma)
        {
            Plasma -= plasma;
            if (Plasma == 0)
                Empty = true;
        }
    }
}
