using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omron.Actors
{
    public delegate void MenuItemPressed(int itemKey);

    public class ActorMenu
    {
        public Actor Actor;
        public ActorMenu(Actor actor)
            :this(actor, new string[0])
        {
        }

        public ActorMenu(Actor actor, string[] coms)
        {
            Actor = actor;
            Commands = coms;
            DisplayBar = false;
        }

        public string[] Commands;
        public float BarValue;
        public bool DisplayBar;

        public event MenuItemPressed CommandInvoked;
        public void OnCommandInvoked(int item)
        {
            if (CommandInvoked != null)
                CommandInvoked(item);
        }

        public string Info = "";
    }
}
