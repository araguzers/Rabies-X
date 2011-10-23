using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabiesX
{
    public class Enemy : Character
    {
        public int Speed { get; set; }
        public string Type { get; set; }

        public Enemy() : base() {}

        public Enemy(int startHealth, int startDefense, int startHitsToWound, int startStrength, int startSpeed, string type)
            : base(startHealth, startDefense, startHitsToWound, startStrength)
        {
            Type = type;
            Speed = startSpeed;
        }

        public void Quicken(int speed)
        {
            Speed += speed;
        }
    }
}
