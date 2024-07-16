namespace TasksBlazor.Models.Enums
{
    public class Status
    {
        private Status(string value) { Value = value; Name = GetName(value); }

        public string Value { get; private set; }
        public string Name { get; private set; }

        public static Status ToDo { get { return new Status("ToDo"); } }
        public static Status InProgress { get { return new Status("InProgress"); } }
        public static Status Done { get { return new Status("Done"); } }

        private string GetName(string value)
        {
            switch (value)
            {
                case "ToDo":
                    return "To do";
                case "InProgress":
                    return "In progress";
                case "Done":
                    return "Done";
                default:
                    return "Value Not Found";
            }
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
