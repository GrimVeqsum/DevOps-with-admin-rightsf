namespace DinoServer.Users;

public class User
{
    public int Id { get; set; }          
    public  string Name { get; set; }        
    public  int Score  { get; set; } 
    
    public User()
    {
        
    }
    /* Конструктор класса */
    public User(string name, int score)
    {
        Name = name;
        Score = score;
    }

    // Перегрузка функции ToString
    public override string ToString()
    {
        return $"{Name}, {Score}";
    }
    
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        var book = (User)obj;
        return Name == book.Name &&
               Score == book.Score;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Score);
    }
}