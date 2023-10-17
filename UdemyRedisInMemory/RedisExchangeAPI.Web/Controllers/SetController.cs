using Microsoft.AspNetCore.Mvc;
using RedisExchangeAPI.Web.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedisExchangeAPI.Web.Controllers
{
    public class SetController : Controller
    {
        //RedisService kullanabilmek için
        private readonly RedisService _redisService;
        //eğer her action metotta çalışacağım db aynı ise hepsinde tek tek aynı tanımlamayı yapmak yerine db tanımlamasını burada yapabiliriz
        private readonly IDatabase db;
        //her action metotta aynı keyi kullanıcam dolayısıyla burada belirleyelim
        private string listKey = "hashnames";

        public SetController(RedisService redisService)
        {
            _redisService = redisService;
            //önce redisteki default 16 dbden birisini seç
            db = _redisService.GetDb(2);
        }

        public IActionResult Index()
        {
            //setten veri listeleme yapalım
            HashSet<string> namesList = new HashSet<string>();
            //önce bu key rediste var mı diye bakalım
            if (db.KeyExists(listKey))
            {
                //key varmış okumaya müsait
                db.SetMembers(listKey).ToList().ForEach(x =>
                {
                    namesList.Add(x.ToString());
                });
            }
            return View(namesList);
        }

        [HttpPost]
        public IActionResult Add(string name)
        {
            //set'e bir eleman ekleyelim
            db.SetAdd(listKey, name);
            //birde bu keye ömür verelim bu sefer
            //eğer ömrü artmasın her seferinde diyorsak Slideing özelliğini kapatmak için. Absolute Expression yani
            if (!db.KeyExists(listKey))
            {
                db.KeyExpire(listKey, DateTime.Now.AddMinutes(5));
            }
            //Slinding Expression yani bu action her çalıştığında 5 dk ömrü artar
            db.KeyExpire(listKey, DateTime.Now.AddMinutes(5));
        
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteItem(string name)
        {
            //eğer bu action metot içerisinde veya bir metot içerisinde birden çok asenkron işlem varsa direkt ana metotu asenkrona çevirmek daha mantıklı Wait() veya Result kullanmak yerine.
            //setten eleman silme işlemi yapalım
            //buradaki awaitin yaptığı şey by işlem sonuçlanana kadar bir alt satıra geçme diyor. Yani aslında otomatik bir callback mekanizması kuruyoruz. Normal bir callback mekanizmasından ne yapardık bir istek yapıyoruz sonuç geldiği zaman çalış dediğimiz bir metot ayarlıyorduk burada bir metot ayarlamana gerek yok aslında awaitin altında yazılacak olan diğer kodlar sizin callback metodunuz oluyor özünde bakarsak.
            await db.SetRemoveAsync(listKey, name);
            return RedirectToAction(nameof(Index));
        }
    }
}
