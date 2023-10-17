using InMemoryApp.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace InMemoryApp.Web.Controllers
{
    public class ProductController : Controller
    {
        //catchleme için
        private readonly IMemoryCache _memoryCache;
        public ProductController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public IActionResult Index()
        {
            //önce memoryde zaman keyine sahip data var mı yok mu kontrol edebiliriz.
            //ikinci yol
            if (!_memoryCache.TryGetValue("zaman", out string zamancache))
            {
                //ömür belirtmek
                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();
                options.AbsoluteExpiration = DateTime.Now.AddSeconds(30);
                options.SlidingExpiration = TimeSpan.FromSeconds(10);
                //cache'e önem vermek
                //Bu benim için önemsiz ram dolarsa ilk bu keyi sil demek için
                options.Priority = CacheItemPriority.Low;
                //key silinirse hangi sebepten silindi öğrenmek için
                //benden delege istiyor delegeler metotları işaret eder. 4 parametreli bir metot istiyor.
                options.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    //verileri yine cachede tutalım
                    _memoryCache.Set("callback", $"{key} -> {value} -> sebep: {reason} -> {state}");
                });
           
                //tarih bilgisini catchleyerek memoryde tutalım
                _memoryCache.Set<string>("zaman", DateTime.Now.ToString(), options);
            }
            //artık zamancache üzerinden dataya erişim sağlayabilirim.
            //sınıfımızı yani complextypeları catchleyelim
            Product p = new Product() { Id = 1, Name = "Kalem", Price = 200};
            _memoryCache.Set<Product>("product:1", p);
            return View();
        }
        public IActionResult Show()
        {
            //memorydeki datayı okuyalım
            _memoryCache.TryGetValue("zaman", out string zamancache);
            _memoryCache.TryGetValue("callback", out string callback);
            ViewBag.zaman = zamancache;
            ViewBag.callback = callback;
            ViewBag.product = _memoryCache.Get<Product>("product:1");
            return View();
        }
    }
}
