﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabiesX
{
    public class Antagonist : Character
    {
        public Sword sword { get; set; }
        //public bool CanSlash { get; set; }

        public Antagonist() : base() { }

        public Antagonist(int startHealth, int startDefense, int startHitsToWound, int startStrength, int maxDurability)
            : base(startHealth, startDefense, startHitsToWound, startStrength)
        {
            sword = new Sword(maxDurability);
        }

        public void BoostSword(int boostAmount)
        {
            sword.Boost(boostAmount);
        }

        public void RepairSword()
        {
            if (sword.Durability < sword.MaximumDurability)
                sword.Repair();
        }

        public void Slash()
        {
            if (!sword.Broken)
               sword.WearOut(1);
        }

        public void Collect(Collectible collectible)
        {
            if (collectible.TargetCharacter == false)
            {
                if (collectible.Type == "hammer")
                    RepairSword();
                else if (collectible.Type == "sword sharpener")
                    Attack += collectible.Effect;
                else if (collectible.Type == "mini anvil")
                    BoostSword(collectible.Effect);
            }
            collectible.GetCollected();
        }
    }
}
