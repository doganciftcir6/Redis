using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace RedisExchangeAPI.Web.Services
{
    public class RedisService
    {
        //appsettings içinden veri okumak için
        private readonly IConfiguration _configuration;
        //appsettingsteki verileri değişkenlere atayalım
        private readonly string _redisHost;
        private readonly string _redisPort;
        //Redis server ile haberleşmek için
        private ConnectionMultiplexer _redis;
        //veritabanıyla haberleşeceğimden dolayı veritabanına ihtiyaç var
        public IDatabase db { get; set; }   
        public RedisService(IConfiguration configuration)
        {
            _configuration = configuration;
            _redisHost = configuration["Redis:Host"];
            _redisPort = configuration["Redis:Port"];
        }
        //Redis Server ile haberleşmek için
        //Uygulama ayağa kalktığı zaman bu metotu çağırmam lazım bunun için
        //startup tarafında middlewear olarak tanım yapacağız.
        public void Connect()
        {
            //connectionStringi oluştur
            var configString = $"{_redisHost}:{_redisPort}";
            //Redis Servera bağlan
            _redis = ConnectionMultiplexer.Connect(configString);
        }
        //16 tane default database vardı biz bunlardan birini seçeceğiz
        //parametreye yazacağımız 1 mesela gidecek redisteki db1'i alacak.
        public IDatabase GetDb(int db)
        {
            return _redis.GetDatabase(db);
        }
    }
}
