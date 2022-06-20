namespace ConsoleApp2.Classes
{
    public class Task
    {
        public int Id { get; set; }
        public string Header { get; set; }
        public string UserName { get; set; }
        public string DateCreation { get; set; }
        public string DateExpiration { get; set; }
        public string State { get; set; }
        public string Description { get; set; } 
        
        public Task(int id, string header, string userName, string dateCreation,string dateExpiration, string description)
        {
            Id = id;
            Header = header;
            UserName = userName;
            DateCreation = dateCreation;
            DateExpiration = dateExpiration;
            State = "new";
            Description = description;
        }
    }
}