using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

//Credit goes to Mr. Jose Baez-Franceschi. This is from his squirrel game.

namespace RabiesX
{
    /// <summary>
    /// Takes in an action list and changes its status
    /// based on keyboard and gamepad input
    /// </summary>
    class Controller
    {

        /// <summary>
        /// Keeps track of a button and key and its press attributes
        /// </summary>
        public class Action
        {
            public Buttons button; //Button of the action
            public Keys key;       //Key bound to action
            public string name;    //Name of the action
            public bool isPressed; //Are either key or button pressed?
            public float pressTime;//How long are they pressed
            public float pressPower; //Gamepad - strengh of press

            public Action(string name, Buttons button, Keys key)
            {
                this.name = name;
                this.button = button;
                this.key = key;
            }
        };
        //Declared states for comparison and input checks
        private KeyboardState keyState, prevKeyState;
        private GamePadState padState, prevPadState;
        private PlayerIndex index;
        private List<Action> actionList;


        public Controller(PlayerIndex index, List<Action> list)
        {
            this.index = index;
            actionList = list;
        }

        public PlayerIndex PlayerIndex
        {
            get { return index; }
        }
        /// <summary>
        /// Find if an action is activated or pressed
        /// </summary>
        /// <param name="name">Name of an action in the action list</param>
        /// <returns>True or False based on the action</returns>
        public bool isActionPressed(string name)
        {
            Action curAction = actionList.Find(delegate(Action a)
            {
                return a.name == name;
            });

            return curAction.isPressed;

        }

        /// <summary>
        /// Get the hold length of press time from an action
        /// </summary>
        /// <param name="name">Name of an action in the action list</param>
        /// <returns>float containig the length of time pressed</returns>
        public float getActionTime(string name)
        {
            Action curAction = actionList.Find(delegate(Action a)
            {
                return a.name == name;
            });

            return curAction.pressTime;
        }

        /// <summary>
        /// Updates key and pad states and also action values
        /// </summary>
        public void update()
        {
            keyState = Keyboard.GetState();
            padState = GamePad.GetState(index);

            foreach (Action action in actionList)
            {
                if (keyState.IsKeyDown(action.key) ||
                            padState.IsButtonDown(action.button))
                {
                    action.isPressed = true;
                    action.pressTime += 1;
                }
                else if ((keyState.IsKeyUp(action.key) &&
                                prevKeyState.IsKeyDown(action.key))
                        || (padState.IsButtonUp(action.button) &&
                                prevPadState.IsButtonDown(action.button)))
                {
                    action.isPressed = false;
                    action.pressTime = 0;
                }; 
            }
            //Last run - store the current states
            //as the previous state
            prevKeyState = keyState;
            prevPadState = padState;
        }
    }
} 
