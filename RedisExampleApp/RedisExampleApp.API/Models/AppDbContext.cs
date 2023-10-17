using Microsoft.EntityFrameworkCore;

namespace RedisExampleApp.API.Models
{
    public class AppDbContext : DbContext
    {
        //Ben AppDbContext'ın configurasyonunu Program.cs tarafında vermek istiyorum bundan dolayı buradaki ctor önemli
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt)
        {

        }

        public DbSet<Product> Products { get; set; }

        //Context ayağa kalktığında InMemoryDb ye data seedlesin her seferinde data kendimiz eklemeyelim
        //Data seedlerken idleri mutlaka kendimiz tanımlamalıyız dikkat
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //EfCore'daki Entitylerimizin Configurasyonlarını yaptığımız yer burası
            modelBuilder.Entity<Product>().HasData(
                new Product() { Id = 1, Name = "Kalem 1" },
                new Product() { Id = 2, Name = "Kalem 2" },
                new Product() { Id = 3, Name = "Kalem 3" });
            base.OnModelCreating(modelBuilder);
        }

        //Program.cs tarafında yapmaktansa burada configuration yapmak istersem bu şekilde yapabilirdim ama ben bu configurationu Program.Cs tarafında yapmak istediğim için ctor oluşturdum.
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseInMemoryDatabase();
        //    base.OnConfiguring(optionsBuilder);
        //}
    }
}
