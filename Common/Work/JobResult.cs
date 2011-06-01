using System;

namespace Common.Work
{
    public class JobResult
    {
        public JobBase Job { get; set; }
        public JobArgs InputArgs { get; set; }
        public Storage.Version Resource { get; set; }

        public bool GuidChanged
        {
            get 
            {
                if (InputArgs == null || Resource == null || InputArgs.Resource == null)
                    return false;

                return InputArgs.Resource.Guid != Resource.Guid;
            }
        }
    }
}
