namespace TasksAPI.Models
{
    public class Constants
    {
        public static string UsernameClaim = "username";
        public static string UserIdClaim = "userId";
        public static string LoginEndpoint = "/api/Auth/Login";
        public static string RegisterEndpoint = "/api/Auth/Signup";
        public static string TasksEndpoint = "/api/Tasks";
        public static string MoveTaskEndpoint = "/api/Tasks/Move";
        public static string CategoriesEndpoint = "/api/Categories";
    }
}
