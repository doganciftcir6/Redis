using RedisExampleApp.API.Models;
using RedisExampleApp.Cache;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisExampleApp.API.Repositories
{
    public class ProductRepositoryWithCacheDecorator : IProductRepository
    {
        //Benim db ile çalışma ihtiyacım var yani data redis catch'de yoksa önce datayı catchlemem lazım bu yüzden datanın önce bana db den gelmesi lazım. Gerçek db bana buradan gelecek.
        //burası ProductRepository classını örneklemiş olacak. Program.cs tarfında yaptığımız ayardan dolayı. Ama burasının dışında her yer ProductRepositoryWithCacheDecorator örnekleyecek.
        private readonly IProductRepository _productRepository;
        //Burada aynı zamanda Catche ile ilgili kodlarda olacak
        private readonly RedisService _redisService;
        //Redisteki default 16 dbden birine bağlanıp o db üzerinden işlem yapmak için
        private readonly IDatabase _cacheRepository;
        //Rediste Oluşturacağım Hash tabloma sabit bir isim vereyim
        private const string productKey = "productCaches";
        public ProductRepositoryWithCacheDecorator(IProductRepository productRepository, RedisService redisService)
        {
            _productRepository = productRepository;
            _redisService = redisService;
            //2. database'te çalışalım redisteki
            _cacheRepository = _redisService.GetDB(2);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            //önce datayı asıl db ye ekle
            var newProduct = await _productRepository.CreateAsync(product);
            //key catche'de var mı kontrol et
            if (!await _cacheRepository.KeyExistsAsync(productKey))
            {
                //Bu key cahcede varsa
                //datayı şimdi birde catche'e JSON olarak ekle, cachein keyi, datanın keyi ve valuesi.
                await _cacheRepository.HashSetAsync(productKey, product.Id, JsonSerializer.Serialize(newProduct));
            }
            return newProduct;
        }

        public async Task<List<Product>> GetAsync()
        {
            //önce data cachede var mı yok mu tespit et
            if (!await _cacheRepository.KeyExistsAsync(productKey))
            {
                //data catche'de yok datalar asıl dbden gelsin cache'e yüklensin ve bana dönsün
                return await LoadCacheFromDbAsync();
            }
            //data catche'de varsa catchelenmiş datayı dönmem lazım
            var products = new List<Product>();
            //catcheden datayı çek
            var cacheProducts = await _cacheRepository.HashGetAllAsync(productKey);
            //her bir catchelenmiş datayı Product sınıfının proplarına aktar, Deserialize json datayı c# classına aktarır.
            foreach (var item in cacheProducts.ToList())
            {
                var product = JsonSerializer.Deserialize<Product>(item.Value);
                //aktarılmış productı product listesine ekle
                products.Add(product);
            }
            return products;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            //data cachede var mı bak
            if (await _cacheRepository.KeyExistsAsync(productKey))
            {
                //data var datayı catcheden al
                var product = await _cacheRepository.HashGetAsync(productKey, id);
                //catcheden gelen data varsa onu Product sınıfının proplarına aktar ve dön, Deserialize json datayı c# classına aktarır.
                return product.HasValue ? JsonSerializer.Deserialize<Product>(product) : null;
            }
            //catchede data yok ise, datayı asıl dbden çek cache'e kaydet ve göster
            var products = await LoadCacheFromDbAsync();
            return products.FirstOrDefault(x => x.Id == id);
        }

        //Productları gerçek db den çekip cachledikten sonra dönecek olan metodum
        private async Task<List<Product>> LoadCacheFromDbAsync()
        {
            //önce tüm datayı db den al
            var products = await _productRepository.GetAsync();
            //sonra cachele
            products.ForEach(x =>
            {
                //cachein keyini ver sonra kayıdın keyi olarak product id yi ver value olarak ise product classının tüm proplarını dolu datasını verip cachliyoruz. C# sınıfımızı JSON veriye dönüştürüp o şekilde redise kaydediyoruz Serialize ile.
                _cacheRepository.HashSetAsync(productKey, x.Id, JsonSerializer.Serialize(x));
            });
            //artık çektiğim ürünü dönebiliriz
            return products;
        }
    }
}
