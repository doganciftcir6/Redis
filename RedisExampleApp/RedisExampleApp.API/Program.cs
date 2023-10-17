using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RedisExampleApp.API.Models;
using RedisExampleApp.API.Repositories;
using RedisExampleApp.API.Services;
using RedisExampleApp.Cache;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Request Respnense'a dönüþünceye kadar tek bir nesne örneði al AddScoped ile, Request Response'a dönüþtüðü anda bu nesne örneði yýkýlacak.
builder.Services.AddScoped<IProductService, ProductService>();
//Cache için uygulamaya diyeceðim ki sen ProductRepositoryWithCacheDecorator classýnýn dýþýnda uygulamanýn herhangi bir yerinde dependncyinjection olarak IProductRepositoryi görürsen artýk ProductRepositoryWithCacheDecorator sýnýfýndan nesne örneði üreteceksin ama ProductRepositoryWithCacheDecorator classýnýn içinde dependencyinjectionda sen IProductRepository interfacesini görürsen bu sefer bizim normal ProductRepository classýmýzýndan nesne örneði alacaksýn diyeceðiz mantýk böyle çalýþacak. 
builder.Services.AddScoped<IProductRepository>( sp =>
{
    var appDbContext = sp.GetRequiredService<AppDbContext>();
    var productRepository = new ProductRepository(appDbContext);
    var redisService = sp.GetRequiredService<RedisService>();

    return new ProductRepositoryWithCacheDecorator(productRepository, redisService);
});

//InMemoryDb baðlantýsý, Contexti dependency olarak geçebilmek için ayrýca
//Dependency olarak burada AppDbContextten bir örnek alýndýðýnda buradaki opt ayarý oluþturduðumuz AppDbContext'in ctoruna gidecek oradaki ctordaki base kodu ise bu kendisine gelen ayarý ef core sýnýfý olan DbContextin ctoruna bu ayarý gönderecek böyle iþliyor.
//yani hem AppDbContext'i dependency injection olarak geçebilmeyi saðladýk hemde opt ayarlarýný burada yani daha merkezi bir yerde yapmýþ olduk.
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseInMemoryDatabase("myDatabase");
});

//RedisServie kullanýmý için 2 yol mevcut;
//ClassLibraryde yazdýðýmýz RedisService'yi uygulamanýn herhangi bir classýnýn ctorunda dependency injection olarak kullanabilmem için onu DI container'a service olarak eklemem lazým.
//RedisServicenin ctorundaki parametreyi () içerisinde veriyoruz. Burada istediði url bilgisini vereceðiz. Buradaki sp metotun aldýðý parametre süslü parantezlerin içi ise metotun gövdesidir.
builder.Services.AddSingleton<RedisService>(sp =>
{
    return new RedisService(builder.Configuration["CacheOptions:Url"]);
});
//Her RedisService kullanacaðým classta her seferinde baðlanacaðým redis database'ini serçmek yerine burada bu iþlemi bir kerede yapabilirim istersem. Bu sayede artýk direkt olarak dependency injectionda IDatabase'i geçersem otomatik olarak 0. redis databwse'ine baðlantý kurulacaktýr her seferinde bu GetDB() metotunu çalýþtýrmama gerek kalmayacaktýr.
builder.Services.AddSingleton<StackExchange.Redis.IDatabase>(sp =>
{
    var redisService = sp.GetRequiredService<RedisService>();
    return redisService.GetDB(0);
});

var app = builder.Build();

//Normal gerçek bir database kullansaydým buna gerek yoktu ama InMemoryDb kullandýðým için datalarýmý görebilmem için dbyi oluþturmam gerekiyor , yapmýþ olduðu iþlem her ayaða kalktýðýnda database'i sýfýrdan oluþturur EnsureCreated() metodu.
//using ile scope oluþturalým bu scope'tan dbContext oluþturduðumda süslü parantez bittiðinde yani ben bu dbContexti kullandýktan sonra memoryden despose olmasýný saðlýyoruz.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
