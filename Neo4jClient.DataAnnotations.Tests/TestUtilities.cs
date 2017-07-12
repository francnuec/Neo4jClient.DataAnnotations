using Neo4jClient.DataAnnotations.Tests.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class TestUtilities
    {
        public static ActorNode Actor = new ActorNode<int>()
        {
            Name = "Ellen Pompeo",
            Born = 1969,
            Address = new AddressWithComplexType()
            {
                City = "Los Angeles",
                State = "California",
                Country = "US",
                Location = new Location()
                {
                    Latitude = 34.0522,
                    Longitude = -118.2437
                }
            }
        };

        public static void AddEntityTypes()
        {
            var entityTypes = new Type[] { typeof(PersonNode), typeof(DirectorNode), typeof(MovieNode), typeof(MovieExtraNode),
                typeof(ActorNode), typeof(Address), typeof(AddressWithComplexType), typeof(Location) };

            foreach (var entityType in entityTypes)
                Neo4jAnnotations.AddEntityType(entityType);
        }

        //public class inner
        //{
        //    public static ActorNode something = new ActorNode<int>()
        //    {
        //        Name = "Ellen Pompeo",
        //        Born = 1969,
        //        Address = new AddressWithComplexType()
        //        {
        //            City = "Los Angeles",
        //            State = "California",
        //            Country = "US",
        //            Location = new Location()
        //            {
        //                Latitude = 34.0522,
        //                Longitude = -118.2437
        //            }
        //        }
        //    };
        //}



        //public void checkvariable()
        //{
            
        //    //var constant = Expression.Constant(something);
        //    //var addr = Expression.PropertyOrField(constant, "Address");
        //    //var lambda = Expression.Lambda(addr);
        //    //var d = lambda.ExecuteExpression<Address>();

        //    Expression<Func<object>> l0 = () => TestUtilities.inner.something; //something._().Address;
        //    var retrieved = Utilities.GetSimpleMemberAccessStretch(l0.Body, out var val);

        //    Expression<Func<object>> l = () => TestUtilities.inner.something.Address;
        //    var retrieved2 = Utilities.GetSimpleMemberAccessStretch(l.Body, out var val2);
        //}
    }
}
