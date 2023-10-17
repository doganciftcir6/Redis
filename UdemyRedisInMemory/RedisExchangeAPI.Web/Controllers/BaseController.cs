using Microsoft.AspNetCore.Mvc;
using RedisExchangeAPI.Web.Services;
using StackExchange.Redis;

namespace RedisExchangeAPI.Web.Controllers
{
    public class BaseController : Controller
    {
        //RedisService kullanabilmek için
        private readonly RedisService _redisService;
        //eğer her action metotta çalışacağım db aynı ise hepsinde tek tek aynı tanımlamayı yapmak yerine db tanımlamasını burada yapabiliriz
        protected readonly IDatabase db;

        public BaseController(RedisService redisService)
        {
            _redisService = redisService;
            //önce redisteki default 16 dbden birisini seç
            db = _redisService.GetDb(5);
        }
    }
}
