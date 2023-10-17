using RedisExampleApp.API.Models;

namespace RedisExampleApp.API.Services
{
    public interface IProductService
    { 
        //buralarda Product değil Dtolar almalı veya dönmeliyiz normalde
        Task<List<Product>> GetAsync();
        Task<Product> GetByIdAsync(int id);
        Task<Product> CreateAsync(Product product);
    }
}
