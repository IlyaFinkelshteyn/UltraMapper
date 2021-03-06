﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    /*- Stack<T> and other LIFO collections require the list to be read in reverse  
    * to preserve order and have a specular clone */
    public class StackMapper : CollectionMapperViaTemporaryCollection
    {
        public StackMapper( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return base.CanHandle( source, target ) &&
                target.GetGenericTypeDefinition() == typeof( Stack<> );
        }

        protected override MethodInfo GetTargetCollectionInsertionMethod( CollectionMapperContext context )
        {
            return context.TargetInstance.Type.GetMethod( "Push" );
        }

        protected override Type GetTemporaryCollectionType( CollectionMapperContext context )
        {
            return typeof( Stack<> ).MakeGenericType( context.SourceCollectionElementType );
        }
    }

    public abstract class CollectionMapperViaTemporaryCollection : CollectionMapper
    {
        public CollectionMapperViaTemporaryCollection( Configuration configuration )
            : base( configuration ) { }

        protected override Expression GetExpressionBody( ReferenceMapperContext contextObj )
        {
            var context = contextObj as CollectionMapperContext;

            //1. Reverse the Stack by creating a new temporary Stack passing source as input
            //2. Read items from the newly created temporary stack and add items to the target

            var paramType = new Type[] { typeof( IEnumerable<> )
                .MakeGenericType( context.SourceCollectionElementType ) };

            var tempCollectionType = this.GetTemporaryCollectionType( context );
            var tempCollectionConstructorInfo = tempCollectionType.GetConstructor( paramType );
            var tempCollection = Expression.Parameter( tempCollectionType, "tempCollection" );

            var newTempCollectionExp = Expression.New( tempCollectionConstructorInfo, context.SourceInstance );

            if( context.IsTargetElementTypeBuiltIn )
            {
                return Expression.Block
                (
                    new[] { tempCollection },

                    Expression.Assign( tempCollection, newTempCollectionExp ),
                    SimpleCollectionLoop( context, tempCollection, context.TargetInstance )
                );
            }

            return Expression.Block
            (
                new[] { tempCollection },

                Expression.Assign( tempCollection, newTempCollectionExp ),
                CollectionLoopWithReferenceTracking( context, tempCollection, context.TargetInstance )
            );
        }

        protected virtual Type GetTemporaryCollectionType( CollectionMapperContext context )
        {
            return typeof( List<> ).MakeGenericType( context.SourceCollectionElementType );
        }
    }

    //public class ReadOnlyCollectionMapper : CollectionMapperViaTemporaryCollection
    //{
    //    public ReadOnlyCollectionMapper( TypeConfigurator configuration )
    //        : base( configuration ) { }

    //    public override bool CanHandle( Type source, Type target )
    //    {
    //        return base.CanHandle( source, target ) &&
    //           target.ImplementsInterface( typeof( IReadOnlyCollection<> ) );
    //    }
    //}
}
