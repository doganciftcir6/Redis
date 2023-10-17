using Microsoft.AspNetCore.Mvc;
using RedisExchangeAPI.Web.Services;
using StackExchange.Redis;
using System;
using System.Data;

namespace RedisExchangeAPI.Web.Controllers
{
    public class StringTypeController : Controller
    {
        //Yazdığımız RedisService'ı kullanmak için
        private readonly RedisService _redisService;
        //eğer her action metotta çalışacağım db aynı ise hepsinde tek tek aynı tanımlamayı yapmak yerine db tanımlamasını burada yapabiliriz
        private readonly IDatabase db;
        public StringTypeController(RedisService redisService)
        {
            _redisService = redisService;
            //önce redisteki default 16 dbden birisini seç
            db = _redisService.GetDb(0);
        }

        public IActionResult Index()
        {
            //redise string data kaydedelim
            //önce redisteki default 16 dbden birisini seç
            //var db = _redisService.GetDb(0);
            db.StringSet("name", "Fatih Çakıroğlu");
            db.StringSet("ziyaretci", 100);
            //eğer Complex Type bir şey kaydedeceksek, classı json veriye veya binary veriye dönüştürerek kayıt yapabiliyoruz daha önce yaptığımızın aynı mantığı
            Byte[] resimByte = default(byte[]);
            db.StringSet("resim", resimByte);
            return View();
        }

        public IActionResult Show()
        {
            //şimdi redisten veri gösterme yapacağız
            //önce redisteki default 16 dbden birisini seç
            //var db = _redisService.GetDb(0);
            //önce veri rediste var mı diye kontrol yapabiliriz, key vererek
            var value = db.StringGet("name");
            //ziyaretci bilgisini bir arttıma işlemi yapabiliriz, key vererek ve 1 1 arttır diyerek
            db.StringIncrement("ziyaretci", 1);
            //ziyaretci bilgisini bir düşürme işlemi yapabiliriz, key vererek ve 1 1 düşür diyerek
            //eğer asenkron işlemde async await ikilisini kullanmak istemiyorsak ve bu metot geriye bir data dönüyorsa Result propunu kullanabiliriz. Eğer metot geriye bir şey dönmeseydi veya geriye dönen değerle ilgilenmiyorsak ise .Wait() kullanabilirdik. Bu şekilde direkt olarak sonucu alabiliyoruz bu komutu direkt olarak asenkron bir şekilde çalıştır diyoruz programa.
            var count = db.StringDecrementAsync("ziyaretci", 1).Result;
            //şimdi birde rediste bulunan bir dataya ek yapalım, key ver ve başlangıç ve bitiş indexlerini ver.
            var value2 = db.StringGetRange("name", 0, 3);
            //redisteki datanın lenghtini'de alabiliriz key vererek
            var value3 = db.StringLength("name");
            if (value.HasValue)
            {
                //data var
                ViewBag.value = value.ToString();
            }
            return View();
        }

    }
}
