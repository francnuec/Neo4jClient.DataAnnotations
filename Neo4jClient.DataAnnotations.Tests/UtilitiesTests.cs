using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Cypher.Functions;
using Neo4jClient.DataAnnotations.Expressions;
using Neo4jClient.DataAnnotations.Tests.Models;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class UtilitiesTests
    {
        private static List<object[]> _varsData;

        private static List<object[]> _finalPropsData;

        public static List<object[]> VarsData
        {
            get
            {
                if (_varsData == null)
                {
                    var data = new[]
                    {
                        new object[]
                            {(Expression<Func<ActorNode>>) (() => CypherVariables.Get<ActorNode>("actor")), "actor"},

                        new object[]
                        {
                            (Expression<Func<int>>) (() =>
                                (int) CypherVariables.Get("actor")["address_location_latitude"]),
                            "actor.address_location_latitude"
                        },

                        new object[]
                        {
                            (Expression<Func<string>>) (() => ((string[]) CypherVariables.Get("actor")["roles"])[0]),
                            "actor.roles[0]"
                        },

                        new object[]
                        {
                            (Expression<Func<string>>) (() => (CypherVariables.Get("actor")["roles"] as string[])[0]),
                            "actor.roles[0]"
                        },

                        new object[]
                        {
                            (Expression<Func<string>>) (() =>
                                CypherVariables.Get<ActorNode>("actor").Roles.ElementAt(2)),
                            "actor.Roles[2]"
                        },

                        new object[]
                        {
                            (Expression<Func<Location>>) (() =>
                                (CypherVariables.Get<ActorNode>("actor").Address as AddressWithComplexType).Location),
                            "actor.NewAddressName_Location"
                        },

                        new object[]
                        {
                            (Expression<Func<double>>) (() =>
                                (CypherVariables.Get<ActorNode>("actor").Address as AddressWithComplexType).Location
                                .Latitude),
                            "actor.NewAddressName_Location_Latitude"
                        },

                        //recursive params
                        new object[]
                        {
                            (Expression<Func<string>>) (() =>
                                (CypherVariables.Get("actor")["roles"] as string[])[
                                    (CypherVariables.Get("actor2")["roles"] as string[])[2]._As<int>()]),
                            "actor.roles[actor2.roles[2]]"
                        },

                        //2 levels recursion
                        new object[]
                        {
                            (Expression<Func<string>>) (() => CypherVariables.Get("actor")["roles"]._As<string[]>()[
                                (CypherVariables.Get("actor2")["roles"] as string[])[
                                    CypherVariables.Get<ActorNode>("actor3").Born
                                ]._As<int>()]),
                            "actor.roles[actor2.roles[actor3.Born]]"
                        },

                        //toString
                        new object[]
                        {
                            (Expression<Func<string>>) (() => CypherVariables.Get<ActorNode>("actor").Born.ToString()),
                            "toString(actor.Born)"
                        },

                        //string size
                        new object[]
                        {
                            (Expression<Func<int>>) (() =>
                                CypherVariables.Get<ActorNode>("actor").Address.City._().Length),
                            "size(actor.NewAddressName_City)"
                        },

                        //string size
                        new object[]
                        {
                            (Expression<Func<int>>) (() =>
                                CypherFunctions._(CypherVariables.Get<ActorNode>("actor").Address.City).Length),
                            "size(actor.NewAddressName_City)"
                        },

                        //toLower
                        //toUpper
                        new object[]
                        {
                            (Expression<Func<string>>) (() => CypherVariables.Get<ActorNode>("actor").Name.ToLower()
                                .ToUpper()),
                            "toUpper(toLower(actor.Name))"
                        },

                        //trim
                        //lTrim
                        //rTrim
                        new object[]
                        {
                            (Expression<Func<string>>) (() =>
                                CypherVariables.Get<ActorNode>("actor").Name.Trim().TrimStart().TrimEnd()),
                            "rTrim(lTrim(trim(actor.Name)))"
                        },

                        //replace
                        new object[]
                        {
                            (Expression<Func<string>>) (() =>
                                CypherVariables.Get<ActorNode>("actor").Name.Replace("Ellen Pompeo",
                                    (CypherVariables.Get("shondaRhimes")["Name"] as string).Trim())),
                            "replace(actor.Name, \"Ellen Pompeo\", trim(shondaRhimes.Name))"
                        },

                        //substring
                        new object[]
                        {
                            (Expression<Func<string>>) (() =>
                                CypherVariables.Get<ActorNode>("actor").Name.Substring(3,
                                    (CypherVariables.Get("shondaRhimes")["Name"] as string).Length)),
                            "substring(actor.Name, 3, size(shondaRhimes.Name))"
                        },

                        //reverse
                        new object[]
                        {
                            (Expression<Func<string>>) (() =>
                                CypherVariables.Get<ActorNode>("actor").Name.Reverse().ToString()),
                            "toString(reverse(actor.Name))"
                        },

                        //simple filter
                        new object[]
                        {
                            (Expression<Func<string>>) (() =>
                                CypherVariables.Get<ActorNode>("actor").Roles.Where(r => r == "role").ToString()),
                            "toString(filter(r IN actor.Roles WHERE r = \"role\"))"
                        },

                        //complex filter
                        //XOR
                        //NOT
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<string>>) (() => CypherVariables.Get<MovieNode>("movie").Actors
                                .Where(actor => (actor.Actorid != "role") ^ !(actor.Movie.Title.Length >= 10))
                                .ElementAt(0)
                                .Actor.Born.ToString()),
                            "toString(filter(actor IN movie.Actors WHERE (actor.Actorid <> \"role\") "
                            + "XOR (NOT (size(actor.Movie.Title) >= 10)))[0].Actor.Born)"
                        },

                        //extract
                        //aggregate
                        //exponential
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<int>>) (() => CypherVariables.Get<MovieNode>("movie").Actors
                                .Where(actor => actor.Actorid._As<int>() > 1)
                                .Select(actor => actor.Actor.Born)
                                .Aggregate(150,
                                    (totalAge, actorYear) => (int) Math.Pow(totalAge + (2017 - actorYear), 2))),
                            "reduce(totalAge = 150, actorYear IN "
                            + "extract(actor IN filter(actor IN movie.Actors WHERE actor.Actorid > 1) | actor.Actor.Born) "
                            + "| (totalAge + (2017 - actorYear)) ^ 2.0)"
                        },

                        //coalesce
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<MovieNode>("movie").Actors
                                .Select(actor => actor.Actor.Born._As<int?>() ?? 0)),
                            "extract(actor IN movie.Actors | coalesce(actor.Actor.Born, 0))"
                        },

                        //average
                        new object[]
                        {
                            (Expression<Func<double>>) (() => CypherVariables.Get<ActorNode>("actor").Born
                                .Average()),
                            "avg(actor.Born)"
                        },

                        //collect
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<ActorNode>("actor")
                                .Collect()),
                            "collect(actor)"
                        },

                        //aggregate count *
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<ActorNode>("*")
                                .Count()),
                            "count(*)"
                        },

                        //aggregate distinct count
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<ActorNode>("actor").Born.Distinct()
                                .Count()),
                            "count(DISTINCT actor.Born)"
                        },

                        //convert
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<ActorNode>("actors")._AsList()
                                .Where(a => Convert.ToBoolean(
                                    Convert.ToSingle(Convert.ToInt32(a.Born))))),
                            "filter(a IN actors WHERE toBoolean(toFloat(toInteger(a.Born))))"
                        },

                        //arraylength = size
                        //length
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<string[]>("actor.Name")
                                .Length._AsList().Length()),
                            "length(size(actor.Name))"
                        },

                        //list size
                        //head
                        //last
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<List<string>>("actor.Name")
                                .Count._AsList().FirstOrDefault()._AsList().Last()),
                            "last(head(size(actor.Name)))"
                        },

                        //all
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<bool>>) (() => CypherVariables.Get<MovieNode>("movie").Actors
                                .All(actor => actor.Actorid._As<int?>() > 1)),
                            "all(actor IN movie.Actors WHERE actor.Actorid > 1)"
                        },

                        //any
                        //null
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<bool>>) (() => CypherVariables.Get<MovieNode>("movie").Actors
                                .Any(actor => actor.Actorid == null)),
                            "any(actor IN movie.Actors WHERE actor.Actorid IS NULL)"
                        },

                        ////any with random variable
                        //new object[] { (Expression<Func<object>>)(() => CypherVariables.Get<List<string>>("actor.Name")
                        //    .Any()),
                        //    "any(randomVar IN actor.Name WHERE randomVar IS NOT NULL)" },

                        //IN with variable
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<List<ActorNode>>("movie.Actors")
                                .AsEnumerable().Contains(CypherVariables.Get<ActorNode>("actor"))),
                            "actor IN movie.Actors"
                        },

                        //List Contain's IN
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<List<string>>("actor.Name")
                                .Contains("c")),
                            "\"c\" IN actor.Name"
                        },

                        //split
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<string>("actor.Name")
                                .Split(new[] {'.'})),
                            "split(actor.Name, \".\")"
                        },

                        //id
                        //type
                        //properties
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<ActorNode>("actor")
                                ._AsList().Id()._AsList().Type()._AsList().Properties()),
                            "properties(type(id(actor)))"
                        },

                        //id
                        //type
                        //properties
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<object>>) (() =>
                                CypherFunctions.Properties(
                                    CypherFunctions._AsList(
                                        CypherFunctions._AsList(
                                            CypherFunctions._AsList(
                                                    CypherVariables.Get<ActorNode>("actor"))
                                                .Id()).Type()))),
                            "properties(type(id(actor)))"
                        },

                        //timestamp
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<ActorNode>("actors")
                                ._AsList().Where(a => a.Born._As<long>() == CypherFunctions.Timestamp())),
                            "filter(a IN actors WHERE a.Born = timestamp())"
                        },

                        //= == IS NULL
                        //<> == IS NOT NULL
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<ActorNode>("actors")
                                ._AsList().Where(a => a.Name == null && null != a.Born._As<int?>())),
                            "filter(a IN actors WHERE (a.Name IS NULL) AND (a.Born IS NOT NULL))"
                        },

                        //= == IS NULL
                        //<> == IS NOT NULL
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<ActorNode>("actors")
                                ._AsList().Where(a => a.Name.IsNull() && a.Born._As<int?>().IsNotNull())),
                            "filter(a IN actors WHERE a.Name IS NULL AND a.Born IS NOT NULL)"
                        },

                        //= == IS NULL
                        //<> == IS NOT NULL
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<ActorNode>("actors")
                                ._AsList().Where(a =>
                                    CypherFunctions.IsNull(a.Name) && CypherFunctions.IsNotNull(a.Born._As<int?>()))),
                            "filter(a IN actors WHERE a.Name IS NULL AND a.Born IS NOT NULL)"
                        },

                        //List Concat == +
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<MovieNode>("actor1.Movies")
                                ._AsList().Concat(CypherVariables.Get<MovieNode>("actor2.Movies")._AsList())),
                            "actor1.Movies + actor2.Movies"
                        },

                        //List Union
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<MovieNode>("actor1.Movies")
                                ._AsList().Union(CypherVariables.Get<MovieNode>("actor2.Movies")._AsList())),
                            "actor1.Movies + actor2.Movies"
                        },

                        ////List Intersect
                        ////this is purely for testing and not necessarily a good example for use in code
                        //new object[] { (Expression<Func<object>>)(() => CypherVariables.Get<MovieNode>("actor1.Movies")
                        //    ._AsList().Intersect(CypherVariables.Get<MovieNode>("actor2.Movies")._AsList())),
                        //    "filter(randomVar IN actor1.Movies WHERE randomVar IN actor2.Movies)" },

                        //* + aggregate count *
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<ActorNode>("*")
                                ._AsList().Union(CypherFunctions.Count(CypherFunctions.Star)._AsList<ActorNode>())),
                            "* + count(*)"
                        },

                        //* + range
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<int>("*")
                                ._AsList().Union(CypherFunctions.Range(9, 5, -2))),
                            "* + range(9, 5, -2)"
                        },

                        //* + atan2
                        //this is purely for testing and not necessarily a good example for use in code
                        new object[]
                        {
                            (Expression<Func<object>>) (() => CypherVariables.Get<int>("*")
                                ._AsList().Union(
                                    Math.Atan2(CypherVariables.Get<int>("aInt"), 2.76457687)._AsList<int>())),
                            "* + atan2(aInt, 2.76457687)"
                        }
                    };

                    _varsData = TestUtilities.TestContextData
                        .SelectMany(cd => data.Select(pd => pd.Union(cd).ToArray()))
                        .ToList();
                }

                return _varsData;
            }
        } //add the testcontexts

        public static List<object[]> FinalPropertiesData
        {
            get
            {
                if (_finalPropsData == null)
                {
                    var data = new[]
                    {
                        new object[]
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"Name", "\"Ellen Pompeo\""},
                                {"Born", "1969.0"},
                                {"NewAddressName_AddressLine", "null"},
                                {"NewAddressName_City", "\"Los Angeles\""},
                                {"NewAddressName_State", "\"California\""},
                                {"NewAddressName_Country", "\"US\""},
                                {"NewAddressName_Location_Latitude", "34.0522"},
                                {"NewAddressName_Location_Longitude", "-118.2437"}
                            },
                            (Expression<Func<object>>) (() => new Dictionary<string, object>
                            {
                                {"Name", TestUtilities.Actor.Name},
                                {"Born", Convert.ToSingle(TestUtilities.Actor.Born)},
                                {"Address", TestUtilities.Actor.Address}
                            })
                        },
                        new object[]
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"Name", "\"Shonda Rhimes\""},
                                {"Born", "1671"},
                                {"NewAddressName_AddressLine", "ellenPompeo.NewAddressName_AddressLine"},
                                {"NewAddressName_City", "ellenPompeo.NewAddressName_City"},
                                {"NewAddressName_State", "ellenPompeo.NewAddressName_State"},
                                {"NewAddressName_Country", "ellenPompeo.NewAddressName_Country"}
                            },
                            (Expression<Func<Dictionary<string, object>, bool>>) (a =>
                                a["Address"] == CypherVariables.Get<ActorNode>("ellenPompeo").Address
                                && (int) a["Born"] == 1671 && a["Name"] == "Shonda Rhimes")
                        },
                        new object[]
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"Name", "\"Ellen Pompeo\""},
                                {"Born", "1969"},
                                {"NewAddressName_AddressLine", "null"},
                                {"NewAddressName_City", "\"Los Angeles\""},
                                {"NewAddressName_State", "\"California\""},
                                {"NewAddressName_Country", "\"US\""}
                            },
                            (Expression<Func<object>>) (() => new
                            {
                                TestUtilities.NormalAddressActor.Name,
                                TestUtilities.NormalAddressActor.Born,
                                TestUtilities.NormalAddressActor.Address
                            })
                        },
                        new object[]
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"Name", "\"Ellen Pompeo\""},
                                {"Born", "1969"},
                                {"Address_AddressLine", "shondaRhimes.NewAddressName_AddressLine"},
                                {"Address_City", "null"},
                                {"Address_State", "null"},
                                {"Address_Country", "null"},
                                {"Address_Location_Latitude", "4.0"},
                                {"Address_Location_Longitude", "ellenPompeo.NewAddressName_Location_Longitude"}
                            },
                            (Expression<Func<object>>) (() => new
                            {
                                TestUtilities.NormalAddressActor.Name,
                                TestUtilities.NormalAddressActor.Born,
                                Address = new AddressWithComplexType
                                {
                                    //Use this style only if you're sure all the properties here are assigned, 
                                    //because this address object would replace the instance address property entirely.
                                    //Also note that there's a good chance the parameters set inline here wouldn't make it to the generated pattern.
                                    //This was done mainly for testing. 
                                    //Use a => a.Address.Location.Longitude == (double)CypherVariables.Get("ellenPompeo")["NewAddressName_Location_Longitude"] instead.

                                    AddressLine = CypherVariables.Get<ActorNode>("shondaRhimes").Address.AddressLine,
                                    Location = new Location
                                    {
                                        Latitude = 4.0,
                                        Longitude = (double) CypherVariables.Get("ellenPompeo")[
                                            "NewAddressName_Location_Longitude"]
                                    }
                                }
                            })
                        },
                        new object[]
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"Name", "\"Ellen Pompeo\""},
                                {"Born", "1969"},
                                {"NewAddressName_AddressLine", "null"},
                                {"NewAddressName_City", "\"Los Angeles\""},
                                {"NewAddressName_State", "\"California\""},
                                {"NewAddressName_Country", "\"US\""},
                                {"NewAddressName_Location_Latitude", "34.0522"},
                                {"NewAddressName_Location_Longitude", "-118.2437"}
                            },
                            (Expression<Func<object>>) (() => new
                                {TestUtilities.Actor.Name, TestUtilities.Actor.Born, TestUtilities.Actor.Address})
                        },
                        new object[]
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"Name", "\"Ellen Pompeo\""},
                                {"Born", "1969"},
                                {"Address_AddressLine", "null"},
                                {"Address_City", "\"Los Angeles\""},
                                {"Address_State", "\"California\""},
                                {"Address_Country", "\"US\""},
                                {"Address_Location_Latitude", "34.0522"},
                                {"Address_Location_Longitude", "-118.2437"}
                            },
                            (Expression<Func<object>>) (() => new
                            {
                                TestUtilities.Actor.Name, TestUtilities.Actor.Born,
                                Address = TestUtilities.Actor.Address
                                    ._() /*name escape should keep the name as it is and not attempt inferring it from reflection.*/
                            })
                        },
                        new object[]
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"Name", "\"Shonda Rhimes\""},
                                {"NewAddressName_AddressLine", "shondaRhimes.NewAddressName_AddressLine"},
                                {"NewAddressName_City", "null"},
                                {"NewAddressName_State", "null"},
                                {"NewAddressName_Country", "null"},
                                {"NewAddressName_Location_Latitude", "4.0"},
                                {"NewAddressName_Location_Longitude", "ellenPompeo.NewAddressName_Location_Longitude"},
                                {"TestForeignKeyId", 5}
                            },
                            (Expression<Func<ActorNode<int>, bool>>) (a => a.Address == new AddressWithComplexType
                            {
                                //Use this style only if you're sure all the properties here are assigned, 
                                //because this address object would replace the instance address property entirely.
                                //Also note that there's a good chance the parameters set inline here wouldn't make it to the generated pattern.
                                //This was done mainly for testing. 
                                //Use a => a.Address.Location.Longitude == (double)CypherVariables.Get("ellenPompeo")["NewAddressName_Location_Longitude"] instead.

                                AddressLine = CypherVariables.Get<ActorNode>("shondaRhimes").Address.AddressLine,
                                Location = new Location
                                {
                                    Latitude = 4.0,
                                    Longitude = (double) CypherVariables.Get("ellenPompeo")[
                                        "NewAddressName_Location_Longitude"]
                                }
                            } && a.Name == "Shonda Rhimes" && a.TestForeignKeyId == 5)
                        },
                        new object[]
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"Name", "\"Shonda Rhimes\""},
                                {"NewAddressName_Location_Longitude", "ellenPompeo.NewAddressName_Location_Longitude"}
                            },
                            (Expression<Func<ActorNode, bool>>) (a =>
                                (a.Address as AddressWithComplexType).Location.Longitude == new AddressWithComplexType
                                {
                                    //Using this style, variables set inline of a member access may or may not make it to the generated pattern, or even throw an exception.
                                    //This is because this MemberInit may be taken as an object value, since it was accessed, and then used directly.
                                    //This was done mainly for testing. 
                                    //Use a => a.Address.Location.Longitude == (double)CypherVariables.Get("ellenPompeo")["NewAddressName_Location_Longitude"] instead.

                                    AddressLine = CypherVariables.Get<ActorNode>("shondaRhimes").Address.AddressLine,
                                    Location = new Location
                                    {
                                        Latitude = 4.0,
                                        Longitude = (double) CypherVariables.Get("ellenPompeo")[
                                            "NewAddressName_Location_Longitude"]
                                    }
                                }.Location.Longitude && a.Name == "Shonda Rhimes")
                            //typeof(InvalidOperationException), string.Format(Messages.AmbiguousVarsPathError, "shondaRhimes.NewAddressName_AddressLine")
                        },
                        new object[]
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"Name", "\"Shonda Rhimes\""},
                                {"NewAddressName_AddressLine", "shondaRhimes.NewAddressName_AddressLine"},
                                {"NewAddressName_City", "null"},
                                {"NewAddressName_State", "null"},
                                {"NewAddressName_Country", "null"},
                                {"NewAddressName_Location_Latitude", "4.0"},
                                {"NewAddressName_Location_Longitude", "ellenPompeo.NewAddressName_Location_Longitude"}
                            },
                            (Expression<Func<ActorNode, bool>>) (a => a.Address == new AddressWithComplexType
                            {
                                //Use this style only if you're sure all the properties here are assigned, 
                                //because this address object would replace the instance address property entirely.
                                //Also note that there's a good chance the parameters set inline here wouldn't make it to the generated pattern.
                                //This was done mainly for testing. 
                                //Use a => a.Address.Location.Longitude == (double)CypherVariables.Get("ellenPompeo")["NewAddressName_Location_Longitude"] instead.

                                AddressLine = CypherVariables.Get<ActorNode>("shondaRhimes").Address.AddressLine,
                                Location = new Location
                                {
                                    Latitude = 4.0,
                                    Longitude = (double) CypherVariables.Get("ellenPompeo")[
                                        "NewAddressName_Location_Longitude"]
                                }
                            } && a.Name == "Shonda Rhimes")
                        },
                        new object[]
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"Name", "\"Shonda Rhimes\""},
                                {"NewAddressName_AddressLine", "shondaRhimes.NewAddressName_AddressLine"}
                            },
                            (Expression<Func<ActorNode, bool>>) (a =>
                                (a.Address as AddressWithComplexType).AddressLine ==
                                CypherVariables.Get<ActorNode>("shondaRhimes").Address.AddressLine
                                && a.Name == "Shonda Rhimes")
                        },
                        new object[]
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"Name", "\"Ellen Pompeo\""},
                                {"Born", "1969"},
                                {"Roles", "null"},
                                {"NewAddressName_AddressLine", "null"},
                                {"NewAddressName_City", "\"Los Angeles\""},
                                {"NewAddressName_State", "\"California\""},
                                {"NewAddressName_Country", "\"US\""},
                                {"NewAddressName_Location_Latitude", "34.0522"},
                                {"NewAddressName_Location_Longitude", "-118.2437"},
                                {"TestForeignKeyId", "0"},
                                {"TestMarkedFK", "0"},
                                {"TestGenericForeignKeyId", "null"}
                            },
                            (Expression<Func<object>>) (() => TestUtilities.Actor)
                        },
                        new object[]
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"Name", "\"Shonda Rhimes\""},
                                {"Born", "ellenPompeo.Born"}
                            },
                            (Expression<Func<ActorNode, bool>>) (a =>
                                a.Born == CypherVariables.Get<ActorNode>("ellenPompeo").Born &&
                                a.Name == "Shonda Rhimes")
                        },
                        new object[]
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"Address_AddressLine", "null"},
                                {"Address_City", "\"Los Angeles\""},
                                {"Address_State", "\"California\""},
                                {"Address_Country", "\"US\""},
                                {"Address_Location_Latitude", "34.0522"},
                                {"Address_Location_Longitude", "-118.2437"},
                                {
                                    "Coordinates",
                                    $"[{(TestUtilities.Actor.Address as AddressWithComplexType).Location.Latitude},shondaRhimes.NewAddressName_Location_Longitude]"
                                }
                            },
                            (Expression<Func<object>>) (() => new
                            {
                                //the following is purely a test, and not necessarily a good example for neo4j cypher.
                                Address = (TestUtilities.Actor.Address as AddressWithComplexType)._(),
                                Coordinates = new[]
                                {
                                    (TestUtilities.Actor.Address as AddressWithComplexType).Location.Latitude,
                                    (double) CypherVariables.Get("shondaRhimes")["NewAddressName_Location_Longitude"]
                                }
                            })
                        },
                        new object[]
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"Location_Latitude", "A.Location_Latitude"},
                                {"Location_Longitude", "56.9"}
                            },
                            (Expression<Func<object>>) (() => new
                            {
                                //the following is purely a test, and not necessarily a good example for neo4j cypher.
                                //avoid using inline CypherVariables with outer member access.
                                new AddressWithComplexType
                                {
                                    AddressLine = CypherVariables.Get("A")["AddressLine"] as string,
                                    Location = new Location
                                    {
                                        Latitude = (double) CypherVariables.Get("A")["Location_Latitude"],
                                        Longitude = 56.90
                                    }
                                }.Location
                            })
                            //typeof(InvalidOperationException), string.Format(Messages.AmbiguousVarsPathError, "A.AddressLine")
                        },
                        new object[]
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"Name", "\"Ellen Pompeo\""},
                                {"Born", "shondaRhimes.Born"},
                                {"Roles", "[\"Meredith Grey\"]"},
                                {"Age", "\"47\""} //because we assigned a 47 string and not a 47 int.
                            },
                            (Expression<Func<object>>) (() =>
                                new
                                {
                                    Name = "Ellen Pompeo",
                                    CypherVariables.Get<ActorNode>("shondaRhimes").Born,
                                    Roles = new[] {"Meredith Grey"},
                                    Age = 47.ToString()
                                })
                        },
                        //this is purely a test, and not the best example
                        new object[]
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"Name", "Ellen Pompeo"},
                                {"Born", "toBoolean(shondaRhimes.Born)"},
                                {"Roles", "[DISTINCT \"Meredith Grey\"]"},
                                {"Age", "\"47\""} //because we assigned a 47 string and not a 47 int.
                            },
                            (Expression<Func<object>>) (() =>
                                new
                                {
                                    Name = new JRaw("Ellen Pompeo"), //._AsRaw(),
                                    Born = Convert.ToBoolean(CypherVariables.Get<ActorNode>("shondaRhimes").Born),
                                    Roles = new[] {"Meredith Grey".Distinct()},
                                    Age = 47.ToString()
                                })
                        }
                    };
                    //add the testcontexts
                    _finalPropsData = TestUtilities.TestContextData.SelectMany(cd => data.Select(pd =>
                    {
                        IEnumerable<object> pEnum = pd;
                        if (pd.Length == 2)
                            //add defaults for expectedExceptionType and expectedExceptionMessage
                            pEnum = pEnum.Append(null).Append(null);
                        foreach (var item in cd) pEnum = pEnum.Append(item);

                        return pEnum.ToArray();
                    })).ToList();
                }

                return _finalPropsData;
            }
        }

        [Theory]
        [MemberData(nameof(VarsData), MemberType = typeof(UtilitiesTests))]
        public void VarsSerialization<T>(Expression<Func<T>> expression, string expectedText,
            string testContextName, TestContext testContext)
        {
            //var testContext = new ConverterTestContext();

            var retrievedMembers = ExpressionUtilities.GetSimpleMemberAccessStretch
                (testContext.AnnotationsContext.EntityService, expression.Body, out var val);

            Assert.True(Utils.Utilities.HasVars(retrievedMembers));

            var expressionVisitor = new FunctionExpressionVisitor(testContext.QueryContext);
            expressionVisitor.Visit(expression.Body);
            var varText = expressionVisitor.Builder.ToString();

            Assert.Equal(expectedText, varText);
        }

        [Theory]
        [MemberData(nameof(FinalPropertiesData), MemberType = typeof(UtilitiesTests))]
        public void FinalPropertiesResolution(Dictionary<string, dynamic> expected, LambdaExpression expression,
            Type expectedExceptionType, string expectedExceptionMessage,
            string testContextName, TestContext testContext)
        {
            Func<JObject> action = () => CypherUtilities.GetFinalProperties
                (expression, testContext.QueryContext, out var hasFuncs);

            if (expectedExceptionType != null)
            {
                var exception = Assert.Throws(expectedExceptionType, () => action.Invoke());

                if (expectedExceptionMessage != null)
                    Assert.Equal(expectedExceptionMessage, exception.Message);
            }
            else
            {
                var finalPropsObj = action.Invoke();
                TestUtilities.TestFinalPropertiesForEquality(testContext.Serializer, expected, finalPropsObj);
            }
        }
    }
}