using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using ProsuctActorService.Interfaces;
using Contracts;

namespace ProsuctActorService
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class ProsuctActorService : Actor, IProsuctActorService
    {
        private string productState = "ProductState";
        public ProsuctActorService(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
        }

        public async Task AddProductAsync(Product product, CancellationToken cancellationToken)
        {
           await this.StateManager.AddOrUpdateStateAsync(productState, product, (k,v)=>v, cancellationToken);

           await this.StateManager.SaveStateAsync();
        }

        public async Task<Product> GetProductAsync(CancellationToken cancellationToken)
        {
            var product = await this.StateManager.GetStateAsync<Product>(productState, cancellationToken);
            
            return product;
        }
        protected override Task OnPreActorMethodAsync(ActorMethodContext actorMethodContext)
        {
            ActorEventSource.Current.ActorMessage(this, $"{actorMethodContext.MethodName} will start now.");
            return base.OnPostActorMethodAsync(actorMethodContext);
        }
        protected override Task OnPostActorMethodAsync(ActorMethodContext actorMethodContext)
        {
            ActorEventSource.Current.ActorMessage(this, $"{actorMethodContext.MethodName} has finished.");
            return base.OnPostActorMethodAsync(actorMethodContext);
        }
        protected override Task OnDeactivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor deactivated.");

            return base.OnDeactivateAsync();
        }


        protected  override Task OnActivateAsync()
        {
            var dataPackage = this.ActorService.Context.CodePackageActivationContext.GetDataPackageObject("Data");
            var dataPath = Path.Combine(dataPackage.Path, "test.csv");
            var contents = File.ReadAllText(dataPath);

            var dbPort = Environment.GetEnvironmentVariable("DbPort");
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            return this.StateManager.TryAddStateAsync("count", 0);
        }
        
    }
}
