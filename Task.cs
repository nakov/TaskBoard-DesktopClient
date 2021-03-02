namespace TaskBoard_DesktopClient
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Board Board { get; set; }
        public string DateCreated { get; set; }
        public string DateModified { get; set; }
    }
}