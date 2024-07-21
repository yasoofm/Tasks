namespace TasksBlazor.Models
{
    public interface DetailsRefreshService
    {
        event Action RefreshRequested;
        void CallRequestRefresh();
    }

    public class MyDetailsService : DetailsRefreshService
    {
        public event Action RefreshRequested;
        public void CallRequestRefresh()
        {
            RefreshRequested?.Invoke();
        }
    }
}
