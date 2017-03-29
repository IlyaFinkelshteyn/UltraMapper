﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMapper.Internals;
using TypeMapper.Mappers.MapperContexts;

namespace TypeMapper.Mappers
{
    public class ReferenceMapperContext : MapperContext
    {
        public ConstructorInfo ReturnTypeConstructor { get; protected set; }
        public MethodInfo AddObjectPairToReturnList { get; internal set; }

        public ParameterExpression ReturnObject { get; protected set; }

        public ConstantExpression SourceNullValue { get; protected set; }
        public ConstantExpression TargetNullValue { get; protected set; }

        public ParameterExpression ReferenceTracker { get; protected set; }

        public ReferenceMapperContext( Type source, Type target )
             : base( source, target )
        {
            var returnType = typeof( List<ObjectPair> );

            ReturnObject = Expression.Variable( returnType, "returnObject" );
            ReturnTypeConstructor = returnType.GetConstructors().First();

            AddObjectPairToReturnList = returnType.GetMethod( nameof( List<ObjectPair>.Add ) );

            ReferenceTracker = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            if( !SourceInstance.Type.IsValueType )
                SourceNullValue = Expression.Constant( null, SourceInstance.Type );

            if( !TargetInstance.Type.IsValueType )
                TargetNullValue = Expression.Constant( null, TargetInstance.Type );
        }
    }
}
