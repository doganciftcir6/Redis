using IDistributedCacheRedisApp.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IDistributedCacheRedisApp.Web.Controllers
{
    public class ProductsController : Controller
    {
        //controller veya herhangi bir classta Redisi kullanmak için
        private readonly IDistributedCache _distributedCache;
        public ProductsController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<IActionResult> Index()
        {
            DistributedCacheEntryOptions cacheEntryOptions = new DistributedCacheEntryOptions();
            cacheEntryOptions.AbsoluteExpiration = DateTime.Now.AddMinutes(1);
            //Complex Type için önce JsonSerialize yapalım
            Product product = new Product() { Id = 1, Name = "Kalem", Price = 100 };
            //bu nesne önerğini json veriye dönüştür
            string jsonProduct = JsonConvert.SerializeObject(product);
            await _distributedCache.SetStringAsync("product:1", jsonProduct, cacheEntryOptions);
            //birde ikinci yol olan BinarySerialize işlemini gösterelim
            //bu sefer data json formatında binary olarak tutulacaktır rediste
            //json datayı byte dönüştürürüz
            Byte[] byteProduct = Encoding.UTF8.GetBytes(jsonProduct);
            _distributedCache.Set("product:2", byteProduct);
            return View();
        }
        public IActionResult Show()
        {
            //Complex type okumak
            string jsonProduct = _distributedCache.GetString("product:1");
            //json datayı kendi classımıza dönüştürelim, Deseriliaze işlemi yani.
            Product p = JsonConvert.DeserializeObject<Product>(jsonProduct);
            ViewBag.Name = p;
            //ikinci yol olan Binary datayı okumak için ise
            //önce byte gelen datayı bir stringe dönüştüemem lazım
            Byte[] byteProduct = _distributedCache.Get("product:2");
            string jsonByteProduct = Encoding.UTF8.GetString(byteProduct);
            //elimde string data var bunu classımıza deserliaze edip döüştürelim
            Product pbyte = JsonConvert.DeserializeObject<Product>(jsonByteProduct);
            ViewBag.NameByte = pbyte;
            return View();
        }
        public IActionResult Delete()
        {
            //silme işlemi yapalımü key değeri bekliyor
            _distributedCache.Remove("name");
            return View();
        }

        public IActionResult ImageCache()
        {
            //resmi byte dizisine dönüştürmem gerekiyor
            //dosya yolunu al
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroote/images/telefonupdateson.jpg");
            byte[] imageByte = System.IO.File.ReadAllBytes(path);
            //artık elimde byte dizisi var catch yapabiliriz
            _distributedCache.Set("resim", imageByte);
            return View();
        }
        public IActionResult ImageUrl()
        {
            //buraya istek atarsam artık resim redis üzerinden bana gelecektir.
            byte[] resimByte = _distributedCache.Get("resim");
            return File(resimByte, "image/jpg");
        }
    }
}
