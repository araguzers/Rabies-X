using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;

//Website used to help out with storage code: msdn.microsoft.com

namespace RabiesX
{
    public class Storage
    {
        public void CreateStorage(StorageDevice device, string storagename, string savename)
        {
            IAsyncResult result = device.BeginOpenContainer(storagename, null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();
            if (!container.FileExists(savename))
            {
                Stream file = container.CreateFile(savename);
                file.Close();
            }
            container.Dispose();
        }

        public void OpenStorage(StorageDevice device, string storagename, string savename)
        {
            IAsyncResult result = device.BeginOpenContainer(storagename, null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();
            Stream stream = container.OpenFile(savename, FileMode.Open);
            stream.Close();
            container.Dispose();
        }

        public void SaveGame(StorageDevice device, string storagename, string savename, Character character)
        {
            IAsyncResult result = device.BeginOpenContainer(storagename, null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();
            Stream stream = container.OpenFile(savename, FileMode.Open, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine("Health: " + character.Health);
            writer.WriteLine("Defense: " + character.Defense);
            writer.WriteLine("Attack: " + character.Attack);
            writer.WriteLine("Hits To Wound: " + character.HitsToWound);
            writer.WriteLine("Alive: " + character.Alive);
            if (character is Protagonist)
            {
                writer.WriteLine("Maximum Plasma: " + ((Protagonist)character).plasmaGun.MaximumPlasma);
                writer.WriteLine("Current Plasma: " + ((Protagonist)character).plasmaGun.Plasma);
                writer.WriteLine("Plasma Gun Empty: " + ((Protagonist)character).plasmaGun.Empty);
            }
            else if (character is Antagonist)
            {
                writer.WriteLine("Maximum Durability: " + ((Antagonist)character).sword.MaximumDurability);
                writer.WriteLine("Current Durability: " + ((Antagonist)character).sword.Durability);
                writer.WriteLine("Sword Broken: " + ((Antagonist)character).sword.Broken);
            }
            writer.Close();
            stream.Close();
            container.Dispose();
        }
    }
}
