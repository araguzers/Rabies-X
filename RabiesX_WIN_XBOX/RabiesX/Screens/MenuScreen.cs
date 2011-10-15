#region File Description
//-----------------------------------------------------------------------------
// MenuScreen.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;
#endregion

namespace RabiesX
{
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    abstract class MenuScreen : GameScreen
    {
        #region Fields

        ContentManager content;

        List<MenuEntry> menuEntries = new List<MenuEntry>();
        int selectedEntry = 0;
        string menuTitle;

        // Set sound effects to use for selecting menu items.
        public static SoundEffect soundMenu1;
        public static SoundEffect soundMenu2;
        public static SoundEffectInstance soundMenu1Instance;
        public static SoundEffectInstance soundMenu2Instance;
        string keyboardEntry;

        // Check if user input is idle.
        int inputCheck = 0;

        #endregion

        #region Properties


        /// <summary>
        /// Gets the list of menu entries, so derived classes can add
        /// or change the menu contents.
        /// </summary>
        protected IList<MenuEntry> MenuEntries
        {
            get { return menuEntries; }
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public MenuScreen(string menuTitle)
        {
            this.menuTitle = menuTitle;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Loads sound effects content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent MenuScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            // Load menu entries sound effects.
            soundMenu1 = content.Load<SoundEffect>("Audio\\Waves\\menu_1");

            // Load menu background sound effects.
            if (soundMenu2 == null)
            {
                soundMenu2 = content.Load<SoundEffect>("Audio\\Waves\\menu_2");
                soundMenu2Instance = soundMenu2.CreateInstance();
                soundMenu2Instance.IsLooped.Equals(true);
            }

            if (soundMenu2Instance.State == SoundState.Stopped)
            {
                soundMenu2Instance.IsLooped.Equals(true);
                soundMenu2Instance.Play(); // play menu sound effect
            }
        }

        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            // Move to the previous menu entry?
            if (input.IsMenuUp(ControllingPlayer))
            {
                selectedEntry--;

                if (selectedEntry < 0)
                    selectedEntry = menuEntries.Count - 1;

                soundMenu1Instance = soundMenu1.CreateInstance();
                soundMenu1Instance.Volume = 0.50f;
                soundMenu1Instance.Play(); // play menu sound effect
            }

            // Move to the next menu entry?
            if (input.IsMenuDown(ControllingPlayer))
            {
                selectedEntry++;

                if (selectedEntry >= menuEntries.Count)
                    selectedEntry = 0;

                soundMenu1Instance = soundMenu1.CreateInstance();
                soundMenu1Instance.Volume = 0.50f;
                soundMenu1Instance.Play(); // play menu sound effect
            }

            // Accept or cancel the menu? We pass in our ControllingPlayer, which may
            // either be null (to accept input from any player) or a specific index.
            // If we pass a null controlling player, the InputState helper returns to
            // us which player actually provided the input. We pass that through to
            // OnSelectEntry and OnCancel, so they can tell which player triggered them.
            PlayerIndex playerIndex;

            if (input.IsMenuSelect(ControllingPlayer, out playerIndex))
            {
                OnSelectEntry(selectedEntry, playerIndex);
            }
            else if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
            {
                OnCancel(playerIndex);
            }

            if (input.CurrentKeyboardStates[(int)playerIndex].IsKeyDown(Keys.Escape))
            {
                keyboardEntry = "Escape";
            }
            else if (input.CurrentKeyboardStates[(int)playerIndex].IsKeyDown(Keys.Enter) || input.CurrentKeyboardStates[(int)playerIndex].IsKeyDown(Keys.Space))
            {
                keyboardEntry = "Enter";
            } 
            else
            {
                keyboardEntry = ""; // default entry for other keyboard input
            }
            inputCheck = 1; // new user input found
        }


        /// <summary>
        /// Handler for when the user has chosen a menu entry.
        /// </summary>
        protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex)
        {
            menuEntries[entryIndex].OnSelectEntry(playerIndex);
        }


        /// <summary>
        /// Handler for when the user has cancelled the menu.
        /// </summary>
        protected virtual void OnCancel(PlayerIndex playerIndex)
        {
            ExitScreen();
        }


        /// <summary>
        /// Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        /// </summary>
        protected void OnCancel(object sender, PlayerIndexEventArgs e)
        {
            OnCancel(e.PlayerIndex);
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the screen the chance to position the menu entries. By default
        /// all menu entries are lined up in a vertical list, centered on the screen.
        /// </summary>
        protected virtual void UpdateMenuEntryLocations()
        {
            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // start at Y = 175; each X value is generated per entry
            Vector2 position = new Vector2(0f, 175f);

            // update each menu entry's location in turn
            for (int i = 0; i < menuEntries.Count; i++)
            {
                MenuEntry menuEntry = menuEntries[i];
                
                // each entry is to be centered horizontally
                position.X = ScreenManager.GraphicsDevice.Viewport.Width / 2 - menuEntry.GetWidth(this) / 2;

                if (ScreenState == ScreenState.TransitionOn)
                    position.X -= transitionOffset * 256;
                else
                    position.X += transitionOffset * 512;

                // set the entry's position
                menuEntry.Position = position;

                // move down for the next entry the size of this entry
                position.Y += menuEntry.GetHeight(this);
            }
        }


        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update each nested MenuEntry object.
            for (int i = 0; i < menuEntries.Count; i++)
            {
                bool isSelected = IsActive && (i == selectedEntry);

                menuEntries[i].Update(this, isSelected, gameTime);
            }

            // Update menu background music.
            UpdateBGM(menuEntries[selectedEntry].Text); 
        }

        // Updates state of background menu music.
        public void UpdateBGM(string menuoption)
        {
            if (menuoption == "Resume Game" && ((keyboardEntry == "Escape") || (keyboardEntry == "Enter")))
            {
                soundMenu2Instance.Stop(); // stop menu sound effect
                keyboardEntry = ""; // default entry for other keyboard input
            } else if (menuoption == "Exit")
            {
                if (soundMenu1Instance.State == SoundState.Stopped)
                {
                    soundMenu2Instance.Play(); // play menu sound effect
                    keyboardEntry = ""; // default entry for other keyboard input
                }
                else if (soundMenu1Instance.State == SoundState.Paused)
                {
                    soundMenu2Instance.Resume(); // resume menu sound effect
                    keyboardEntry = ""; // default entry for other keyboard input
                }
 
            } else if (menuoption == "Options")
            {
                if (keyboardEntry == "Escape")
                {
                    if (soundMenu1Instance.State == SoundState.Stopped)
                    {
                        soundMenu2Instance.Play(); // play menu sound effect
                        keyboardEntry = ""; // default entry for other keyboard input
                    }
                    else if (soundMenu1Instance.State == SoundState.Paused)
                    {
                        soundMenu2Instance.Resume(); // resume menu sound effect
                        keyboardEntry = ""; // default entry for other keyboard input
                    }
                } else 
                {
                    if (soundMenu2Instance.State == SoundState.Stopped)
                    {
                        soundMenu2Instance.Play(); // play menu sound effect
                        keyboardEntry = ""; // default entry for other keyboard input
                    }
                    else if (soundMenu2Instance.State == SoundState.Paused)
                    {
                        soundMenu2Instance.Resume(); // resume menu sound effect
                        keyboardEntry = ""; // default entry for other keyboard input
                    }
                }
            } else if (menuoption == "Play Game")
            {
                if (keyboardEntry == "Escape")
                {
                    if (soundMenu2Instance.State == SoundState.Stopped)
                    {
                        soundMenu2Instance.Play(); // play menu sound effect
                    }
                    else if (soundMenu2Instance.State == SoundState.Paused)
                    {
                        soundMenu2Instance.Resume(); // resume menu sound effect
                    }
                }
                else if (keyboardEntry == "Enter")
                {
                    soundMenu2Instance.Stop(); // stop menu sound effect
                }
            }
        }

        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // make sure our entries are in the right place before we draw them
            UpdateMenuEntryLocations();

            // Check if input is idle.
            if (inputCheck == 1)
            {
                inputCheck = 0; // reset input check to default value
            }

            // If input idle, continue to play menu background effects while on "play game" menu item.
            if (menuEntries[selectedEntry].Text == "Play Game")
            {
                if ((keyboardEntry != "Enter") && (inputCheck == 0))
                {
                    if (soundMenu2Instance.State == SoundState.Stopped)
                    {
                        soundMenu2Instance.Play(); // play menu sound effect
                        keyboardEntry = ""; // default entry for other keyboard input
                    }
                    else if (soundMenu2Instance.State == SoundState.Paused)
                    {
                        soundMenu2Instance.Resume(); // resume menu sound effect
                        keyboardEntry = ""; // default entry for other keyboard input
                    }
                    keyboardEntry = ""; // default entry for other keyboard input
                }
            }

            GraphicsDevice graphics = ScreenManager.GraphicsDevice;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            spriteBatch.Begin();

            // Draw each menu entry in turn.
            for (int i = 0; i < menuEntries.Count; i++)
            {
                MenuEntry menuEntry = menuEntries[i];

                bool isSelected = IsActive && (i == selectedEntry);

                menuEntry.Draw(this, isSelected, gameTime);
            }

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // Draw the menu title centered on the screen
            Vector2 titlePosition = new Vector2(graphics.Viewport.Width / 2, 80);
            Vector2 titleOrigin = font.MeasureString(menuTitle) / 2;
            Color titleColor = new Color(192, 192, 192) * TransitionAlpha;
            float titleScale = 1.25f;

            titlePosition.Y -= transitionOffset * 100;

            spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0,
                                   titleOrigin, titleScale, SpriteEffects.None, 0);

            spriteBatch.End();
        }


        #endregion
    }
}
