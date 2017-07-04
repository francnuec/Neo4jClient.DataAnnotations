using Neo4jClient.Cypher;
using NSubstitute;
using System;
using Xunit;
using Neo4jClient.DataAnnotations;
using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Tests.Models;
using System.Linq.Expressions;

namespace Neo4jClient.DataAnnotations.Tests
{
    public class PathExtensionTests
    {
        [Fact]
        public void ARBAreAllNull_InvalidOperationException()
        {
            var builder = Substitute.For<IPathBuilder>();

            var ex = Assert.Throws<InvalidOperationException>(() => builder.Pattern(null, null, null));

            Assert.Equal(Messages.NullARBParametersError, ex.Message);
        }

        [Fact]
        public void RelationshipIsNull_ArgumentNullException()
        {
            var builder = Substitute.For<IPathBuilder>();

            var ex = Assert.Throws<ArgumentNullException>(() =>
            builder.Pattern((Expression<Func<ActorNode, MovieNode>>)null));

            Assert.Equal("relationship", ex.ParamName);
        }

        [Fact]
        public void BeginRelationshipIsNull_ArgumentNullException()
        {
            var builder = Substitute.For<IPathBuilder>();

            var ex = Assert.Throws<ArgumentNullException>(() =>
            builder.Pattern((Expression<Func<ActorNode, MovieActorRelationship>>)null, (r) => r.Movie));

            Assert.Equal("beginRelationship", ex.ParamName);
        }

        [Fact]
        public void ExtensionRBAreBothNull_NoInvalidOperationException()
        {
            var query = Substitute.For<ICypherFluentQuery>();

            var builder = new DummyPath((p) => null, query);

            var path = builder.Pattern((ActorNode actor) => actor.Movies, (r) => r.Movie)
                .Extend(null, null, RelationshipDirection.Incoming);

            //if we reached this point, test passed.
        }

        [Fact]
        public void ExtensionPattern_TrueIsExtensionProperty()
        {
            var query = Substitute.For<ICypherFluentQuery>();

            var builder = new DummyPath((p) => null, query);

            var path = builder.Pattern((ActorNode actor) => actor.Movies, (r) => r.Movie)
                .Extend("R2", "C", RelationshipDirection.Incoming);

            var pattern = path.Pattern;

            Assert.NotNull(pattern);
            Assert.Equal(pattern.AParameter, null);
            Assert.Equal(pattern.RParameter, "R2"); //test this to make sure we are interacting with the right pattern here, and not a previous one.
            Assert.Equal(pattern.BParameter, "C");
            Assert.True(pattern.isExtension);
        }

        [Fact]
        public void BothConstraintsAndPropsAreSet_InvalidOperationException()
        {
            var query = Substitute.For<ICypherFluentQuery>();

            var builder = new DummyPath((p) => null, query);

            var ex = Assert.Throws<InvalidOperationException>(() => builder.Pattern((ActorNode actor) => actor.Movies, (r) => r.Movie)
                .Constrain((actor) => actor.Name == "Ellen Pompeo" && actor.Born == 1969)
                .Prop(() => new { Name = "Ellen Pompeo", Born = 1969 }));

            Assert.Equal(string.Format(Messages.PropsAndConstraintsClashError, "A"), ex.Message);
        }
    }
}
