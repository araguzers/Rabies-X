using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabiesX
{
    public class Protagonist : Character
    {
        public PlasmaGun plasmaGun {get; set;}
        public bool CanFire { get; set; }

        public Protagonist(int startHealth, int startDefense, int startHitsToWound, int startStrength, int maxAmmo)
            : base(startHealth, startDefense, startHitsToWound, startStrength)
        {
            plasmaGun = new PlasmaGun(maxAmmo);
        }

        public void BoostGun(int boostAmount)
        {
            plasmaGun.Boost(boostAmount);
        }

        public void RechargeGun()
        {
            if(plasmaGun.Plasma < plasmaGun.MaximumPlasma)
                plasmaGun.Recharge();
        }

        public void Shoot()
        {
            if(!plasmaGun.Empty)
                plasmaGun.Waste(1);
        }
    }
}
