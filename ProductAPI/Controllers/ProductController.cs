using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Threading.Tasks;
using ProsuctActorService.Interfaces;

namespace ProductAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        [HttpGet]
        public async Task<Product> GetProductById([FromQuery] int id)
        {
            var actorId = new ActorId(id);
            var proxy = ActorProxy.Create<IProsuctActorService>(actorId, new Uri("fabric:/ProductActorApp/ProsuctActorServiceActorService"));
            var product = await proxy.GetProductAsync(new System.Threading.CancellationToken());
            return product;
        }

        [HttpPost]
        public async Task AddProduct([FromBody] Product product )
        {
            var actorId = new ActorId(product.Id);
            var proxy = ActorProxy.Create<IProsuctActorService>(actorId, new Uri("fabric:/ProductActorApp/ProsuctActorServiceActorService"));
            await proxy.AddProductAsync(product, new System.Threading.CancellationToken());
        }

        [HttpDelete]
        public async Task DeleteProduct([FromQuery] int productId)
        {
            var actorId = new ActorId(productId);
            var proxy = ActorServiceProxy.Create( new Uri("fabric:/ProductActorApp/ProsuctActorServiceActorService"),
                actorId);
            await proxy.DeleteActorAsync(actorId, new System.Threading.CancellationToken());
        }
    }
}
