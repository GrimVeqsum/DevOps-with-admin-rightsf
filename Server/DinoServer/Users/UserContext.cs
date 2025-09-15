//using BooksLab.DataBase;
using Microsoft.EntityFrameworkCore;


namespace DinoServer.Users;

public class UserContext : DbContext
{
    public int UserId { get; set; }

    private bool CurrentUser { get; }
    
    private readonly DbContextOptions<UserContext> _configureOptions;

    //Set через который происходит взаимодействие с БД
    public virtual DbSet<User> Users => Set<User>();


    public UserContext(DbContextOptions<UserContext> configureOptions) : base(configureOptions)
    {
        _configureOptions = configureOptions;
        Database.EnsureCreated();
    }

    //настройка Базы данных
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //MYSQL
        //base.OnConfiguring(optionsBuilder);
        /*optionsBuilder.UseMySql(
            "server=localhost;user=user;password=password;database=books;", 
            new MySqlServerVersion(new Version(8, 0, 11))
        );*/
        //SQLite
        //optionsBuilder.UseSqlite("Data Source=book.db");
    }

    //синхронное добавление книги в БД
    public void AddBook(User user)
    {
        Users.Add(user);
        SaveChanges();
        Console.WriteLine("Книга добавлена в каталог!");
    }
    
    //Ассинхронное добавление книги в БД
    public async Task<User> AddUserAsync(User user)
    {
        await Users.AddAsync(user);
        await SaveChangesAsync();
        Console.WriteLine("Книга добавлена в каталог!");
        return user;
    }
}
