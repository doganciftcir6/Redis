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

//Request Respnense'a d�n���nceye kadar tek bir nesne �rne�i al AddScoped ile, Request Response'a d�n��t��� anda bu nesne �rne�i y�k�lacak.
builder.Services.AddScoped<IProductService, ProductService>();
//Cache i�in uygulamaya diyece�im ki sen ProductRepositoryWithCacheDecorator class�n�n d���nda uygulaman�n herhangi bir yerinde dependncyinjection olarak IProductRepositoryi g�r�rsen art�k ProductRepositoryWithCacheDecorator s�n�f�ndan nesne �rne�i �reteceksin ama ProductRepositoryWithCacheDecorator class�n�n i�inde dependencyinjectionda sen IProductRepository interfacesini g�r�rsen bu sefer bizim normal ProductRepository class�m�z�ndan nesne �rne�i alacaks�n diyece�iz mant�k b�yle �al��acak. 
builder.Services.AddScoped<IProductRepository>( sp =>
{
    var appDbContext = sp.GetRequiredService<AppDbContext>();
    var productRepository = new ProductRepository(appDbContext);
    var redisService = sp.GetRequiredService<RedisService>();

    return new ProductRepositoryWithCacheDecorator(productRepository, redisService);
});

//InMemoryDb ba�lant�s�, Contexti dependency olarak ge�ebilmek i�in ayr�ca
//Dependency olarak burada AppDbContextten bir �rnek al�nd���nda buradaki opt ayar� olu�turdu�umuz AppDbContext'in ctoruna gidecek oradaki ctordaki base kodu ise bu kendisine gelen ayar� ef core s�n�f� olan DbContextin ctoruna bu ayar� g�nderecek b�yle i�liyor.
//yani hem AppDbContext'i dependency injection olarak ge�ebilmeyi sa�lad�k hemde opt ayarlar�n� burada yani daha merkezi bir yerde yapm�� olduk.
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseInMemoryDatabase("myDatabase");
});

//RedisServie kullan�m� i�in 2 yol mevcut;
//ClassLibraryde yazd���m�z RedisService'yi uygulaman�n herhangi bir class�n�n ctorunda dependency injection olarak kullanabilmem i�in onu DI container'a service olarak eklemem laz�m.
//RedisServicenin ctorundaki parametreyi () i�erisinde veriyoruz. Burada istedi�i url bilgisini verece�iz. Buradaki sp metotun ald��� parametre s�sl� parantezlerin i�i ise metotun g�vdesidir.
builder.Services.AddSingleton<RedisService>(sp =>
{
    return new RedisService(builder.Configuration["CacheOptions:Url"]);
});
//Her RedisService kullanaca��m classta her seferinde ba�lanaca��m redis database'ini ser�mek yerine burada bu i�lemi bir kerede yapabilirim istersem. Bu sayede art�k direkt olarak dependency injectionda IDatabase'i ge�ersem otomatik olarak 0. redis databwse'ine ba�lant� kurulacakt�r her seferinde bu GetDB() metotunu �al��t�rmama gerek kalmayacakt�r.
builder.Services.AddSingleton<StackExchange.Redis.IDatabase>(sp =>
{
    var redisService = sp.GetRequiredService<RedisService>();
    return redisService.GetDB(0);
});

var app = builder.Build();

//Normal ger�ek bir database kullansayd�m buna gerek yoktu ama InMemoryDb kulland���m i�in datalar�m� g�rebilmem i�in dbyi olu�turmam gerekiyor , yapm�� oldu�u i�lem her aya�a kalkt���nda database'i s�f�rdan olu�turur EnsureCreated() metodu.
//using ile scope olu�tural�m bu scope'tan dbContext olu�turdu�umda s�sl� parantez bitti�inde yani ben bu dbContexti kulland�ktan sonra memoryden despose olmas�n� sa�l�yoruz.
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
