using System;
using System.Linq.Expressions;
using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Tests.Models;
using Neo4jClient.DataAnnotations.Utils;
using Xunit;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class PathExtensionTests
    {
        [Fact]
        public void ARBAreAllNull_InvalidOperationException()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var ex = Assert.Throws<InvalidOperationException>(() => builder.Pattern(null, null, null));

            Assert.Equal(Messages.NullARBVariablesError, ex.Message);
        }

        [Fact]
        public void BeginRelationshipIsNull_ArgumentNullException()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var ex = Assert.Throws<ArgumentNullException>(() =>
                builder.Pattern((Expression<Func<ActorNode, MovieActorRelationship>>)null, r => r.Movie));

            Assert.Equal("beginRelationship", ex.ParamName);
        }

        [Fact]
        public void BothConstraintsAndPropsAreSet_InvalidOperationException()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var ex = Assert.Throws<InvalidOperationException>(() => builder
                .Pattern((ActorNode actor) => actor.Movies, r => r.Movie)
                .Constrain(actor => actor.Name == "Ellen Pompeo" && actor.Born == 1969)
                .Prop(() => new { Name = "Ellen Pompeo", Born = 1969 }));

            Assert.Equal(string.Format(Messages.PropsAndConstraintsClashError, "A"), ex.Message);
        }

        [Fact]
        public void ExtensionPattern_TrueIsExtensionProperty()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var path = builder.Pattern((ActorNode actor) => actor.Movies, r => r.Movie)
                .Extend("R2", "C", RelationshipDirection.Incoming);

            var pattern = path.Pattern as Pattern;

            Assert.NotNull(pattern);

            Assert.NotNull(pattern.AVariable);
            Assert.True(pattern.AVarIsAuto);
            Assert.Equal("R2",
                pattern.RVariable); //test this to make sure we are interacting with the right pattern here, and not a previous one.
            Assert.Equal("C", pattern.BVariable);
            Assert.True(pattern.IsExtension);
        }

        [Fact]
        public void ExtensionRBAreBothNull_NoInvalidOperationException()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var path = builder.Pattern((ActorNode actor) => actor.Movies, r => r.Movie)
                .Extend(null, null, RelationshipDirection.Incoming);

            //if we reached this point, test passed.
        }

        [Fact]
        public void NewTypeIsAssignableToOldType()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var path = builder.Pattern<PersonNode>("actor")
                .Constrain(actor => actor.Name == "Ellen Pompeo" && actor.Born == 1969);

            Assert.Equal(typeof(PersonNode), path.Pattern.AType);

            path = path.Type(typeof(ActorNode));

            Assert.Equal(typeof(ActorNode), path.Pattern.AType);
        }

        [Fact]
        public void NewTypeIsNotAssignableToOldType_InvalidOperationException()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var ex = Assert.Throws<InvalidOperationException>(() => builder
                .Pattern((ActorNode actor) => actor.Movies, r => r.Movie)
                .Constrain(actor => actor.Name == "Ellen Pompeo" && actor.Born == 1969)
                .Type(typeof(MovieNode), null, null));

            Assert.Equal(string.Format(Messages.UnassignableTypeError, typeof(MovieNode), typeof(ActorNode)),
                ex.Message);
        }

        [Fact]
        public void RelationshipIsNull_ArgumentNullException()
        {
            var testContext = new ResolverTestContext();
            var query = testContext.Query;

            var builder = new PathBuilder(query);

            var ex = Assert.Throws<ArgumentNullException>(() =>
                builder.Pattern((Expression<Func<ActorNode, MovieNode>>)null));

            Assert.Equal("relationship", ex.ParamName);
        }
    }
}