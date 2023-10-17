using Microsoft.AspNetCore.Mvc;
using RedisExchangeAPI.Web.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedisExchangeAPI.Web.Controllers
{
    public class SortedSetController : Controller
    {
        //RedisService kullanabilmek için
        private readonly RedisService _redisService;
        //eğer her action metotta çalışacağım db aynı ise hepsinde tek tek aynı tanımlamayı yapmak yerine db tanımlamasını burada yapabiliriz
        private readonly IDatabase db;
        //her action metotta aynı keyi kullanıcam dolayısıyla burada belirleyelim
        private string listKey = "sortedsetnames";

        public SortedSetController(RedisService redisService)
        {
            _redisService = redisService;
            //önce redisteki default 16 dbden birisini seç
            db = _redisService.GetDb(3);
        }

        public IActionResult Index()
        {
            //SortedSetten veri okuyalım
            HashSet<string> list = new HashSet<string>();
            //bu key rediste var mı kontrol
            if (db.KeyExists(listKey))
            {
                //key rediste var data okumaya hazır
                //SortedSetScan metotu redis içinde sıralama nasılsa o sırlamaya göre verileri getirir socre değeri ile beraber, istersek Score propertsi üzerinden x.Score olarak sadece score'a veya x.Element ile sadece valueye erişebiliyoruz. 
                db.SortedSetScan(listKey).ToList().ForEach(x =>
                {
                    list.Add(x.ToString());
                });
                //Birde score değeri büyükten küçüğe doğru verileri sıralayalım, score değeri gelmiyor
                db.SortedSetRangeByRank(listKey, order: Order.Descending).ToList();

            }
            return View(list);
        }

        [HttpPost]
        public IActionResult Add(string name, int score)
        {
            //Soted Set içerisine item ekleyelim
            db.SortedSetAdd(listKey, name, score);
            //key için ömür belirtelim, Sliding Expression
            db.KeyExpire(listKey, DateTime.Now.AddMinutes(1));
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteItem(string name)
        {
            //Soted Set içinden eleman silelim
            db.SortedSetRemove(listKey, name);
            return RedirectToAction(nameof(Index));
        }
    }
}
