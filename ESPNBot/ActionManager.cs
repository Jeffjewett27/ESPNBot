using System;
using System.Collections.Generic;
using System.Text;

namespace ESPNBot
{
    class ActionManager
    {
        private List<Action> actions;
        private List<Action>.Enumerator actionEnumerator;

        public ActionManager()
        {
            actions = new List<Action>();
        }

        public void AddAction(Action action)
        {
            actions.Add(action);
        }

        public void RunActions()
        {
            foreach (Action action in actions)
            {
                action.Invoke();
            }
            actions.Clear();
        }
    }
}
