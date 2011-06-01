using System;

namespace Common.Work.Tasks
{
    public interface ITask
    {
        bool Execute(Storage.Version resource, out string errorMessage);
    }
}
