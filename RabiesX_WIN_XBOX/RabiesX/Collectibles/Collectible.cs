using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabiesX
{
    public class Collectible
    {
        public bool TargetCharacter { get; set; } //true if Araguz, false if Jackson
        public string Type { get; set; } //represents the type of collectible, e.g. "plasma container", "hammer", etc.
        public int Effect { get; set; }

        public Collectible() { }

        public Collectible(string character, string type, int howMuch)
        {
            if (character == "araguz")
                TargetCharacter = true;
            else
                TargetCharacter = false;
            Type = type;
            Effect = howMuch;
        }

        public void GetCollected()
        {
            Effect = 0;
        }
    }
}
