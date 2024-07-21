namespace TasksBlazor.Models
{
    public interface HomeRefreshService
    {
        event Action RefreshRequested;
        void CallRequestRefresh();
    }

    public class MyHomeService : HomeRefreshService
    {
        public event Action RefreshRequested;
        public void CallRequestRefresh()
        {
            RefreshRequested?.Invoke();
        }
    }
}
