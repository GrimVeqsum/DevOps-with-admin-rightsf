namespace DinoServer.Users;

public class User
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public int Score { get; set; }

    public User()
    {
        Name = string.Empty;
    }

    // Конструктор класса
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public User(string name, int score)
    {
        Name = name;
        Score = score;
    }

    // Перегрузка функции ToString
    public override string ToString() => $"{Name}, {Score}";

    public override bool Equals(object? obj) =>
        obj is User user && Name == user.Name && Score == user.Score;

    public override int GetHashCode() => HashCode.Combine(Name, Score);
}
