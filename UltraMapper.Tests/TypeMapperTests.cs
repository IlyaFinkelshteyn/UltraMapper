﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using UltraMapper.MappingConventions;
using UltraMapper.MappingExpressionBuilders;
using System.Linq;

namespace UltraMapper.Tests
{
    [TestClass]
    public class ConventionTests
    {
        public class SourceClass
        {
            public int A { get; set; } = 31;
            public long B { get; set; } = 33;
            public int C { get; set; } = 71;
            public int D { get; set; } = 73;
            public int E { get; set; } = 101;
        }

        public class TargetClass
        {
            public long A { get; set; }
            public int B { get; set; }
        }

        public class TargetClassDto
        {
            public long ADto { get; set; }
            public int ADataTransferObject { get; set; }

            public long BDataTransferObject { get; set; }

            public int Cdto { get; set; }
            public int Ddatatransferobject { get; set; }

            public int E { get; set; }
        }

        [TestMethod]
        public void ExactNameAndImplicitlyConvertibleTypeConventionTest()
        {
            var source = new SourceClass();
            var target = new TargetClass();

            var mapper = new UltraMapper();
            mapper.Map( source, target );

            Assert.IsTrue( source.A == target.A );
            Assert.IsTrue( source.B == target.B );
        }

        [TestMethod]
        public void ExactNameAndTypeConventionTest()
        {
            var source = new SourceClass();
            var target = new TargetClass();

            var config = new Configuration( cfg =>
            {
                cfg.MappingConvention.MatchingRules.GetOrAdd<TypeMatchingRule>( ruleConfig =>
                {
                    ruleConfig.AllowImplicitConversions = false;
                    ruleConfig.AllowExplicitConversions = false;
                } );
            } );

            var mapper = new UltraMapper( config );
            mapper.Map( source, target );

            Assert.IsTrue( source.A != target.A );
            Assert.IsTrue( source.B != target.B );
        }

        [TestMethod]
        public void SuffixNameAndTypeConventionTest()
        {
            var source = new SourceClass();
            var target = new TargetClassDto();

            var mapper = new UltraMapper( cfg =>
            {
                cfg.MappingConvention.MatchingRules
                    .GetOrAdd<TypeMatchingRule>( ruleConfig => ruleConfig.AllowImplicitConversions = true )
                    .GetOrAdd<ExactNameMatching>( ruleConfig => ruleConfig.IgnoreCase = true )
                    .GetOrAdd<SuffixMatching>( ruleConfig => ruleConfig.IgnoreCase = true )
                    .Respect( ( rule1, rule2, rule3 ) => rule1 & (rule2 | rule3) );
            } );

            mapper.Map( source, target );

            Assert.IsTrue( source.A == target.ADto );
            Assert.IsTrue( source.B == target.BDataTransferObject );
            Assert.IsTrue( source.C == target.Cdto );
            Assert.IsTrue( source.D == target.Ddatatransferobject );
            Assert.IsTrue( source.E == target.E );
        }
    }
}
