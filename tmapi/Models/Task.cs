namespace tmapi.Models
{
    public class Task
    {
        public int Id { get; set; }  // Unique identifier for each task
        public string Title { get; set; }  // Title of the task
        public string Description { get; set; }  // Detailed description of the task
        public int Status { get; set; }  // Status of the task (0 = Pending, 1 = In Progress, 2 = Completed)
        public DateTime CreatedAt { get; set; }  // Date and time when the task was created
        public DateTime DueDate { get; set; }  // Due date for the task completion
        public DateTime? CompletedAt { get; set; }  // Date and time when the task was completed (nullable)
        public int Progress { get; set; }  // Progress of the task in percentage (0 to 100)
        public string Priority { get; set; }  // Priority of the task (Low, Medium, High)
        public string CreatedBy { get; set; }  // Username of the person who created the task
    }

}
