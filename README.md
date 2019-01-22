# Neo4jClient.DataAnnotations #
Use POCO classes in the [Neo4jClient](https://github.com/Readify/Neo4jClient) library ORM style. Annotate with [`System.ComponentModel.DataAnnotations.Schema`](https://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations.schema) attributes. Supports Complex Types too.

Get it on [Nuget](https://www.nuget.org/packages/Neo4jClient.DataAnnotations):

> **PM> Install-Package Neo4jClient.DataAnnotations**

### Quick Intro ###
----------
I use [Entity Framework](https://github.com/aspnet/EntityFramework) with SQL databases. So when I needed a no-sql database like Neo4j, I always wanted something similar to and with the ease of Entity Framework, especially so I could reuse my existing models to make my Neo4j queries. Hence, this library follows the same annotations pattern as Entity Framework, and allows POCO models in Neo4j pattern descriptions. If you've ever used Entity Framework, understanding and integrating this library is a piece of cake.

For our quick introduction into this library, let's try to model the popular Neo4j actors-movies example with annotations. Here's our `ActorNode` model:

    [Table("Actor")]
    public class ActorNode
    { 
	    public ActorNode()
	    {
	    	Address = new Address();
	    }
	    
	    public string Name { get; set; }
        public int Born { get; set; }
        public Address Address { get; set; }
        public string[] Roles { get; set; }
        
        [Column("ACTED_IN")]
        [InverseProperty("Actors")]
        public ICollection<MovieNode> Movies { get; set; }
    }

Here are a few things to note from this `ActorNode` model class:

 - The `TableAttribute` annotation gives the Neo4j label for the node. So while we have an `ActorNode` model class, our actual node label for this class would be `Actor`. If no `TableAttribute` is found in the class hierarchy (that is, including base classes), the class name is used instead. If multiple table attributes are found in the class hierarchy, they would all be added as labels (e.g. `:User:Actor:Person`).
 
 - The `Movies` property, which is a collection, says that an actor can act in many movies.
 
 - The `InversePropertyAttribute` annotation points to an `Actors` property on the other end of the relationship (which is the `MovieNode` class), describing the many-to-many relationship that exists between actors and movies. That is, an actor acts in many movies while each movie also has many actors.

 - The `ColumnAttribute` annotation indicates that an outgoing relationship named `ACTED_IN` exists between actors and movies. That is, we just described the Neo4j pattern: `(actor:Actor)-[acted_in:ACTED_IN]->(movie:Movie)`. Only one `ColumnAttribute` is needed between the two models, and the model that hosts this `ColumnAttribute` is the start of the outgoing direction of the relationship that ends in the other model. If no `ColumnAttribute` is found on either end of a relationship, the name of the specified property is used, and the direction will also need to be explicitly specified.

 - The `Address` property is a complex property. That is, it's return type (`Address` class) is annotated with the `ComplexTypeAttribute`. By default, all properties with non-primitive or array return types are taken as navigational properties (that is, they help describe relationships between models) and hence, automatically removed from the final json output as Neo4j cannot store complex object graphs directly into a node. However, we often need these complex graphs in our models. So to make this possible, you can annotate any class that would serve as a complex object with the `ComplexTypeAttribute`. `Neo4jClient.DataAnnotations` takes care of serializing this object for you by exploding its properties, and including them as scalars of the main json output, just like Entity Framework would. 

 - Notice how the complex property `Address` is deliberately initialized at the `ActorNode` constructor? This has a meaning, and is required. In order for the library to appropriately figure out how to serialize these complex properties, they must always have a value set to them at the point of serialization. If they are found to be null, an exception is raised. Complex properties should never be null.

Here are our `Address` and `MovieNode` models. Same observations apply as in the `ActorNode` class.
        
    [ComplexType]
    public class Address
    {
        public string AddressLine { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }
    
    [Table("Movie")]
    public class MovieNode
    {
	    public int Year { get; set; }
	    public string Title { get; set; }
	    
	    [InverseProperty("Movies")]
	    public ICollection<ActorNode> Actors { get; set; }
    }

Now that we have described our models, we need to use them in our Cypher queries. To do this, we'd use the fluent interface this library exposes on the Neo4jClient methods. This interface also allows you to modify your patterns on demand, overriding select existing data attributes. Remember to add:

    using Neo4jClient.DataAnnotations;

Let's match each actor with all movies they acted in using our models.

In Cypher:

    MATCH (actor:Actor)-[acted_in:ACTED_IN]->(movie:Movie)
    RETURN actor, COLLECT(movie) AS movies

With annotations: 

    var results = graphClient.WithAnnotations().Cypher
    .Match(path => path.Pattern<ActorNode, MovieNode>("actor", "acted_in", "movie"))
    .Return((actor, movie) => new {
        Actor = actor.As<ActorNode>(),
        Movies = movie.CollectAs<MovieNode>()
    })
    .Results

Looks pretty simple right? The `Match` method used here is one of many extension methods overloaded on the existing Neo4jClient methods that allows you to describe your patterns via annotated POCO models. These extensions from this library do not affect the existing Neo4jClient methods in any way, and you can safely combine them as per your needs.

Let's take another example to explain constraints on Cypher MATCH statements. Let's match Tom Hanks's co-actors for each of his movies.

In Cypher:

    MATCH (tom:Actor { Name: "Tom Hanks" })-[:ACTED_IN]->(movie)<-[:ACTED_IN]-(coActor)
    RETURN movie, COLLECT(coActor) AS coActors

In this library, the example Cypher query above describes an extended path, because it involves two connected patterns. To reproduce this query with annotations, we would employ lambda expressions to explicitly show our relationships, rather than depend on `DataAnnotations.Schema` attributes to figure it out for us. This is another great way to use this library.

With annotations:

    var results = graphClient.WithAnnotations().Cypher
    .Match(path => path
    .Pattern((ActorNode tom) => tom.Movies, "movie")
    .Constrain(tom => tom.Name == "Tom Hanks")
    .Extend((MovieNode movie) => movie.Actors, "coActor"))
    .Return((movie, coActor) => new {
        Movie = movie.As<MovieNode>(),
        CoActors = coActor.CollectAs<ActorNode>()
    })
    .Results

The above examples employ really simple scenarios to explain this library. The real advantage you'd get from using this library would be in describing really complex patterns (with maybe complex properties too), and then the library accurately interprets your intentions.

### Neo4jClient Integration ###
----------
To use this library with `Neo4jClient` in your project, you must register it with your `IGraphClient` instance so as to make needed configuration changes before any code that uses the `Neo4jClient` library is called. You must call the `IGraphClient.WithAnnotations` method, or the `IGraphClient.WithAnnotationsConverter` method, for each ***new instance*** of `IGraphClient` that you use. You're permitted to call just one of them. These methods attach a default instance of the `AnnotationsContext` class to the `IGraphClient` instance. Again, although you can safely call any of the methods multiple times as you wish, you really only need to make the call just once for each new instance of `IGraphClient` that you use.

Also, ideally, this library needs to know all your entity types (i.e., model classes) early on so as to best determine how to construct the class hierarchies. For simple classes with no inheritances, you may skip adding any entity types. However, if your models have derived types, especially for complex type models, it's best to input all entity types at the point of registration. This is done by subclassing the `AnnotationsContext` class, then adding the entity types as properties of that class the same way it is done in `EntityFramework` context classes. For instance, this is a sample context class used in our tests:

    public class AppContext : AnnotationsContext
    {
        public AppContext(IGraphClient graphClient, EntityResolver resolver, EntityService entityService) 
                : base(graphClient, resolver, entityService)
        {
        }

        public AppContext(IGraphClient graphClient, EntityConverter converter, EntityService entityService) 
                : base(graphClient, converter, entityService)
        {
        }

        public virtual EntitySet<PersonNode> Persons { get; set; }
        public virtual EntitySet<DirectorNode> Directors { get; set; }
        public virtual EntitySet<MovieNode> Movies { get; set; }
        public virtual EntitySet<MovieExtraNode> MovieExtras { get; set; }
        public virtual EntitySet<ActorNode> Actors { get; set; }
        public virtual EntitySet<Address> Addresses { get; set; }
        public virtual EntitySet<Location> Locations { get; set; }
    }
    
Note that, unlike `EntityFramework`, even the `ComplexType` classes are added to this context class. It is planned that in a later release of this library, support for `LINQ` operations will be added to the library through these `EntitySet` properties. For now, they just help the library discover all your entity types early on, and ahead of usage.

Now, you can proceed to attach the new context class to your `IGraphClient` instance by calling the `IGraphClient.WithAnnotations<AppContext>` method. For projects that utilize dependency containers, you should instead configure the `AnnotationsContext` class as a service, then require it as a dependency in your classes. In ASP.NET core projects, this is best done within the `ConfigureServices` method of your `Startup` class using the provided helper methods. For example:

    public void ConfigureServices(IServiceCollection services)
    {
        //Add framework services.
        
        //Add your choice of IGraphClient as a service.
	
        //Add Neo4jClient.DataAnnotations as a service
        services.AddNeo4jAnnotations<AppContext>(); 
	
        //or simply, services.AddNeo4jAnnotations(), for the default context instance.
    }
    
----------
... and we're done.

Please see this [wiki page](https://github.com/francnuec/Neo4jClient.DataAnnotations/wiki) for more examples. You can also check the unit tests to get an idea of other features available in the library. To see this library used in an actual project, head over to my [Neo4j.AspNetCore.Identity](https://github.com/francnuec/Neo4j.AspNetCore.Identity) project to study the code and sample.

If you encounter an exception anywhere, kindly raise an issue so we can deal with it. If you're are trying to figure out how to describe your models, or a Neo4j pattern, ask your question in an issue too and hopefully we can use your scenario as an example for everyone else.

Cheers.

>  
>  
> Written with [StackEdit](https://stackedit.io/).
