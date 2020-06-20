#define GremlinServer
//#define CosmosDB
//#define AWSNeptune
//#define JanusGraph

using System;
using System.Linq;
using System.Threading.Tasks;
using ExRam.Gremlinq.Core;
using ExRam.Gremlinq.Providers.WebSocket;
using Gremlin.Net.Process.Traversal;
using Microsoft.Extensions.Logging;

// Put this into static scope to access the default GremlinQuerySource as "g". 
using static ExRam.Gremlinq.Core.GremlinQuerySource;

namespace ExRam.Gremlinq.Samples
{
    public class Program
    {
        private Person _marko;
        private Person _peter;
        private readonly IGremlinQuerySource _g;

        public Program()
        {
            var logger = LoggerFactory
                .Create(builder => builder
                    .AddFilter(__ => true)
                    .AddConsole())
                .CreateLogger("Queries");

            _g = g
                .ConfigureEnvironment(env => env
                    .UseLogger(logger)
                    .UseModel(GraphModel
                        .FromBaseTypes<Vertex, Edge>(lookup => lookup
                            .IncludeAssembliesOfBaseTypes())
                        .ConfigureProperties(model => model
                            .ConfigureElement<Vertex>(conf => conf
                                .IgnoreOnUpdate(x => x.PartitionKey))))

                    .UseGremlinServer(builder => builder
                        .AtLocalhost()
                        .ConfigureQueryLoggingOptions(o => o
                            .SetQueryLoggingVerbosity(QueryLoggingVerbosity.None))));

        }

        public async Task Run()
        {
            await Create_the_graph();
            await FindMutualFriendsBetweenPeterAndMarko();
            Console.Write("Press any key...");
            Console.Read();
        }

        private async Task FindMutualFriendsBetweenPeterAndMarko()
        {

            var mutualFriends = await _g.V(_marko.Id).Both<Knows>()
                .Where(__ => __.Both<Knows>())
                .As((f, __) => f.V(_peter.Id)
                    .Where(s => s == f).Dedup().OfType<Person>());

            if (mutualFriends.Length > 0)
            {
                foreach (var mutualFriend in mutualFriends)
                {
                    Console.WriteLine(mutualFriend.Name);
                }
            }
            else
            {
                Console.WriteLine("No mutual friends found");
            }
        }


        private async Task Create_the_graph()
        {
            // Create a graph very similar to the one
            // found at http://tinkerpop.apache.org/docs/current/reference/#graph-computing.

            // Uncomment to delete the whole graph on every run.
            await _g.V().Drop();

            _marko = await _g
                .AddV(new Person { Name = "Marko", Age = 29 })
                .FirstAsync();

            var vadas = await _g
                .AddV(new Person { Name = "Vadas", Age = 27 })
                .FirstAsync();

            var josh = await _g
                .AddV(new Person { Name = "Josh", Age = 32 })
                .FirstAsync();

            _peter = await _g
               .AddV(new Person { Name = "Peter", Age = 35 })
               .FirstAsync();

            var daniel = await _g
                .AddV(new Person
                {
                    Name = "Daniel",
                    Age = 37
                })
                .FirstAsync();


            var stefan = await _g
                .AddV(new Person
                {
                    Name = "Stefan",
                    Age = 37,
                })
                .FirstAsync();



            var torrey = await _g
                .AddV(new Person
                {
                    Name = "Torrey",
                    Age = 37,
                    PhoneNumbers = new[]
                    {
                        "+491234567",
                        "+492345678"
                    }
                })
                .FirstAsync();

            #region marko and josh

            await _g
                  .V(_marko.Id)
                  .AddE<Knows>()
                  .To(__ => __
                      .V(josh.Id))
                  .FirstAsync();


            await _g
                .V(josh.Id)
                .AddE<Knows>()
                .To(__ => __
                    .V(_marko.Id))
                .FirstAsync();
            #endregion
            #region marko and  vadas

            await _g
                .V(_marko.Id)
                .AddE<Knows>()
                .To(__ => __
                    .V(vadas.Id))
                .FirstAsync();

            await _g
                .V(vadas.Id)
                .AddE<Knows>()
                .To(__ => __
                    .V(_marko.Id))
                .FirstAsync();

            #endregion
            #region marko and daniel

            await _g
                .V(_marko.Id)
                .AddE<Knows>()
                .To(__ => __
                    .V(daniel.Id))
                .FirstAsync();

            await _g
                .V(daniel.Id)
                .AddE<Knows>()
                .To(__ => __
                    .V(_marko.Id))
                .FirstAsync();

            #endregion
            #region marko and stefan

            await _g
                .V(_marko.Id)
                .AddE<Knows>()
                .To(__ => __
                    .V(stefan.Id))
                .FirstAsync();

            await _g
                .V(stefan.Id)
                .AddE<Knows>()
                .To(__ => __
                    .V(_marko.Id))
                .FirstAsync();

            #endregion

            #region peter  and vadas

            await _g
                .V(_peter.Id)
                .AddE<Knows>()
                .To(__ => __
                    .V(vadas.Id))
                .FirstAsync();

            await _g
                .V(vadas.Id)
                .AddE<Knows>()
                .To(__ => __
                    .V(_peter.Id))
                .FirstAsync();
            #endregion
            #region peter  and torrey

            await _g
                .V(_peter.Id)
                .AddE<Knows>()
                .To(__ => __
                    .V(torrey.Id))
                .FirstAsync();

            await _g
                .V(torrey.Id)
                .AddE<Knows>()
                .To(__ => __
                    .V(_peter.Id))
                .FirstAsync();
            #endregion
            #region peter  and josh

            await _g
                .V(_peter.Id)
                .AddE<Knows>()
                .To(__ => __
                    .V(josh.Id))
                .FirstAsync();

            await _g
                .V(josh.Id)
                .AddE<Knows>()
                .To(__ => __
                    .V(_peter.Id))
                .FirstAsync();
            #endregion


        }
        private static async Task Main()
        {
            await new Program().Run();
        }
    }
}
