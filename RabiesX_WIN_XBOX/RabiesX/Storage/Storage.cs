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
        public bool MetJackson { get; set; }

        public Storage()
        {
            MetJackson = false;
        }

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

        public void Load(StorageDevice device, string storagename, string loadname, Character character)
        {
            IAsyncResult result = device.BeginOpenContainer(storagename, null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();
            string line;
            string[] words;
            Stream stream = container.OpenFile(loadname, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream);
            if (character is Protagonist)
            {
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    words = line.Split(' ');
                    if (words[0] == "Health")
                        character.Health = Convert.ToInt16(words[1]);
                    else if (words[0] == "Defense")
                        character.Defense = Convert.ToInt16(words[1]);
                    else if (words[0] == "Attack")
                        character.Attack = Convert.ToInt16(words[1]);
                    else if (words[0] == "HitsToWound")
                        character.HitsToWound = Convert.ToInt16(words[1]);
                    else if (words[0] == "Alive")
                    {
                        if (words[1] == "yes")
                            character.Alive = true;
                        else
                            character.Alive = false;
                    }
                    else if (words[0] == "MaximumPlasma")
                        ((Protagonist)character).plasmaGun.MaximumPlasma = Convert.ToInt16(words[1]);
                    else if (words[0] == "Plasma")
                        ((Protagonist)character).plasmaGun.Plasma = Convert.ToInt16(words[1]);
                    else if (words[0] == "Empty")
                    {
                        if (words[1] == "no")
                            ((Protagonist)character).plasmaGun.Empty = false;
                        else
                            ((Protagonist)character).plasmaGun.Empty = true;
                    }
                }
            }
            else if (character is Antagonist)
            {
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    words = line.Split(' ');
                    if (words[0] == "Met")
                    {
                        if (words[1] == "no")
                        {
                            MetJackson = false;
                            break;
                        }
                        MetJackson = true;
                    }
                    else if (words[0] == "Health")
                        character.Health = Convert.ToInt16(words[1]);
                    else if (words[0] == "Defense")
                        character.Defense = Convert.ToInt16(words[1]);
                    else if (words[0] == "Attack")
                        character.Attack = Convert.ToInt16(words[1]);
                    else if (words[0] == "HitsToWound")
                        character.HitsToWound = Convert.ToInt16(words[1]);
                    else if (words[0] == "Alive")
                    {
                        if (words[1] == "yes")
                            character.Alive = true;
                        else
                            character.Alive = false;
                    }
                    else if (words[0] == "MaximumDurability")
                        ((Antagonist)character).sword.MaximumDurability = Convert.ToInt16(words[1]);
                    else if (words[0] == "Durability")
                        ((Antagonist)character).sword.Durability = Convert.ToInt16(words[1]);
                    else if (words[0] == "Broken")
                    {
                        if (words[1] == "no")
                            ((Antagonist)character).sword.Broken = false;
                        else
                            ((Antagonist)character).sword.Broken = true;
                    }
                }
            }
            reader.Close();
            stream.Close();
            container.Dispose();
        }

        public void Save(StorageDevice device, string storagename, string savename, Character character)
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

        public void Rename(StorageDevice device, string storagename, string oldsavename, string newsavename)
        {
            IAsyncResult result = device.BeginOpenContainer(storagename, null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();
            if (container.FileExists(oldsavename) && !container.FileExists(newsavename))
            {
                Stream oldStream = container.OpenFile(oldsavename, FileMode.Open);
                Stream newStream = container.CreateFile(newsavename);
                oldStream.CopyTo(newStream);
                oldStream.Close();
                newStream.Close();
                container.DeleteFile(oldsavename);
            }
            container.Dispose();
        }

        public void Copy(StorageDevice device, string storagename, string savename, string copyname)
        {
            IAsyncResult result = device.BeginOpenContainer(storagename, null, null);            
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();
            if (container.FileExists(savename) && !container.FileExists(copyname))
            {
                Stream stream = container.OpenFile(savename, FileMode.Open);
                Stream copyStream = container.CreateFile(copyname);
                stream.CopyTo(copyStream);
                stream.Close();
                copyStream.Close();
            }
            container.Dispose();
        }

        private static void Display(StorageDevice device, string storagename)
        {
            IAsyncResult result = device.BeginOpenContainer(storagename, null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();
            string[] savedGames = container.GetFileNames();
            foreach (string game in savedGames)
                Console.WriteLine(savedGames);
            container.Dispose();
        }


        private static void Delete(StorageDevice device, string storagename, string deletename)
        {
            IAsyncResult result = device.BeginOpenContainer(storagename, null, null);            
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();
            if (container.FileExists(deletename))
                container.DeleteFile(deletename);
            container.Dispose();
        }

    }
}
