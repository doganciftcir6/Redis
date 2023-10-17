using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedisExampleApp.Cache
{
    public class RedisService
    {
        //Redise bağlanabilmek için ConnectionMultiplexer sınıfına ihtiyacım var.
        private readonly ConnectionMultiplexer _multiplexer;
        //Bu ctorun parametresine url Program.cs taraınfan gelecek
        public RedisService(string url)
        {
            _multiplexer = ConnectionMultiplexer.Connect(url);
        }

        //Bize redisten default 16 dbden seçtiğimiz birisine bağlanammızı sağlayacak olan metot
        //Database'e bağlanıp onun üstünden redis işlemleri gerçekleştireceğiz.
        public IDatabase GetDB(int dbIndex)
        {
            return _multiplexer.GetDatabase(dbIndex);
        } 
    }
}
