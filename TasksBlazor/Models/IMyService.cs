namespace TasksBlazor.Models
{
    public interface IMyService
    {
        event Action RefreshRequested;
        void CallRequestRefresh();
    }

    public class MyService : IMyService
    {
        public event Action RefreshRequested;
        public void CallRequestRefresh()
        {
            RefreshRequested?.Invoke();
        }
    }
}
