using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabiesX
{
    public abstract class Character
    {
        public int Health { get; set; } //character's hit points.
        public int Defense { get; set; } //the bigger this number, the slower hit points are lost.
        public int Attack { get; set; } //attack strength of the character
        public bool Alive { get; set; } //checks if the character is alive or dead.
        public int HitsToWound {get; set;} //the number of hits it takes to wound the character.

        public Character() { }

        public Character(int startHealth, int startDefense, int startHitsToWound, int startStrength)
        {
            Health = startHealth;
            Defense = startDefense;
            Attack = startStrength;
            HitsToWound = startHitsToWound;
            Alive = true;
        }

        public void Heal(int healthAdded)
        {
            Health += healthAdded;
        }

        public void Wound(int healthDeducted)
        {
            Health -= healthDeducted;
        }

        public void PowerUp(int armor)
        {
            Defense += armor;
        }

        public void PowerDown(int armor)
        {
            Defense -= armor;
        }

        public void Strengthen(int attack)
        {
            Attack += attack;
        }

        public void Die()
        {
            Alive = false;
        }
    }
}
