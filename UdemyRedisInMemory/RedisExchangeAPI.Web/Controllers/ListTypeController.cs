using Microsoft.AspNetCore.Mvc;
using RedisExchangeAPI.Web.Services;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

namespace RedisExchangeAPI.Web.Controllers
{
    public class ListTypeController : Controller
    {
        //RedisService kullanabilmek için
        private readonly RedisService _redisService;
        //eğer her action metotta çalışacağım db aynı ise hepsinde tek tek aynı tanımlamayı yapmak yerine db tanımlamasını burada yapabiliriz
        private readonly IDatabase db;
        //her action metotta aynı keyi kullanıcam dolayısıyla burada belirleyelim
        private string listKey = "names";
        public ListTypeController(RedisService redisService)
        {
            _redisService = redisService;
            //önce redisteki default 16 dbden birisini seç
            db = _redisService.GetDb(1);
        }

        public IActionResult Index()
        {
            //redisten datalarımızı listeleyelim
            List<string> list = new List<string>();
            //önce rediste bu key var mı kontrol
            if (db.KeyExists(listKey))
            {
                //key rediste var data okumaya müsait
                //eğet bu metotda başlangıç bitiş indexi vermezsek tüm dataları alır
                db.ListRange(listKey).ToList().ForEach(x =>
                {
                    list.Add(x.ToString());
                });
            }
            return View(list);
        }

        [HttpPost]
        public IActionResult Add(string name)
        {
            //Liste data kaydetme işlemi,
            //buraya datayı Viewdan yollayalım daha gerçekçi olsun
            //datayı listenin en sağına yani sonuna kaydeder ListRightPush.
            //Left versiyonu ise listenin en başına ekleme yapacaktır.
            db.ListRightPush(listKey, name);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteItem(string name)
        {
            //şimdi listeden eleman silme işlemi yapacağız
            //asenkron kullanırsak uygulamanın performansını arttırır
            //uygulama ayağa kalktığı zaman var olan Threadler çok daha efektif bir şekilde kullanılacaktır. Bu sayede uygulama daha performanslı ve responselarınız daha hızlı olur. Daha fazla requesti karşılayabilirsiniz.
            //Asenkron kullanıldığı zaman Thread yönetimini çok daha kolay bir şekilde yapar.
            //Ama asenkron kullanmazsak bu metottaki işler bitinceye kadar Threadi tutacağından dolayı o Threadi bırakmayacağından dolayı Thread havuzunuzda çok fazla Thread olmayacaktır. Ama asenkronda Thread havuzundan bir Thread aldığı zaman asenkron metot işi bittiği zaman o Threadi direkt olarak o havuza bırakır ihtiyaç olduğu zaman tekrar kullanır ama asenkron kullanmadığınızda Thread havuzundan bir Thread aldığı zaman işlem bitinceye kadar işlem 1 dakika da sürse 1 dakika boyunca o Threadi tutar ve böylece Thread havuzunuzda çok fazla Thread kalmadığından dolayı çok performanslı çalışmaz uygulamamız. O yüzden mümkün olduğunca asenkron kullanmaya çalışalım.
            db.ListRemoveAsync(listKey, name).Wait();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteFirstItem()
        {
            //şimdi listenin en başındaki elemanı silme işlemi yapalım. ListLeftPop İLE
            //ListRightPop ise listenin son elemanını silecektir en sağdaki.
            db.ListLeftPop(listKey);
            return RedirectToAction(nameof(Index));
        }
    }
}
