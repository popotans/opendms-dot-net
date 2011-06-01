using System;
using System.Collections.Generic;

namespace Common.Work.Chaining
{
    public class TaskChain
    {
        private List<Tasks.Base> _commands = null;
        private int _atStep = 0;

        public int StepPosition { get { return _atStep; } }

        public int StepCount { get { return _commands.Count; } }

        public int PercentComplete
        {
            get
            {
                return _commands[_atStep].PercentComplete;
            }
        }

        public TaskChain()
        {
            _commands = new List<Tasks.Base>();
        }

        public TaskChain(List<Tasks.Base> commands)
        {
            _commands = commands;
        }

        public void Add(Tasks.Base command)
        {
            _commands.Add(command);
        }

        public bool Execute(Storage.Version resource, out Tasks.Base failingCommand, 
            out string errorMessage)
        {
            for (int i=0; i<_commands.Count; i++)
            {
                if (!_commands[i].Execute(resource, out errorMessage))
                {
                    failingCommand = _commands[i];
                    return false;
                }
            }

            failingCommand = null;
            errorMessage = null;
            return true;
        }
    }
}
