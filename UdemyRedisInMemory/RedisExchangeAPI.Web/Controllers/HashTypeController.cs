using Microsoft.AspNetCore.Mvc;
using RedisExchangeAPI.Web.Services;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

namespace RedisExchangeAPI.Web.Controllers
{
    //eğer ctor kısımlarının tekrara düştüğünü düşünürsek böyle yapabiliriz. BaseController
    public class HashTypeController : BaseController
    {
        //her action metotta aynı keyi kullanıcam dolayısıyla burada belirleyelim
        private string hashKey = "sozluk";
        public HashTypeController(RedisService redisService) : base(redisService)
        {
        }

        public IActionResult Index()
        {
            //hashden data okuma işlemi yapalım
            Dictionary<string, string> list = new Dictionary<string, string>();
            //bu key rediste varmı kontrol
            if (db.KeyExists(hashKey))
            {
                //rediste var data okumaya hazır
                db.HashGetAll(hashKey).ToList().ForEach(x =>
                {
                    list.Add(x.Name, x.Value);
                });
            }
            return View(list);
        }

        [HttpPost]
        public IActionResult Add(string name, string value)
        {
            //Hash içine eleman ekleyelim
            db.HashSet(hashKey, name, value);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteItem(string name)
        {
            //Hash içinden eleman silelim
            db.HashDelete(hashKey, name);
            return RedirectToAction(nameof(Index));
        }
    }
}
