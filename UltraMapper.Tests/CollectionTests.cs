﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UltraMapper.Internals;
using UltraMapper.MappingConventions;

namespace UltraMapper.Tests
{
    [TestClass]
    public class CollectionTests
    {
        private static Random _random = new Random();

        //IComparable is required to test sorted collections
        private class ComplexType : IComparable<ComplexType>
        {
            public int A { get; set; }
            public InnerType InnerType { get; set; }

            public int CompareTo( ComplexType other )
            {
                return this.A.CompareTo( other.A );
            }

            public override int GetHashCode()
            {
                return this.A;
            }

            public override bool Equals( object obj )
            {
                var otherObj = obj as ComplexType;
                if( otherObj == null ) return false;

                return this.A.Equals( otherObj?.A ) &&
                    (this.InnerType == null && otherObj.InnerType == null) ||
                    ((this.InnerType != null && otherObj.InnerType != null) &&
                        this.InnerType.Equals( otherObj.InnerType ));
            }
        }

        private class InnerType
        {
            public string String { get; set; }
        }

        private class GenericCollections<T>
        {
            public HashSet<T> HashSet { get; set; }
            public SortedSet<T> SortedSet { get; set; }
            public List<T> List { get; set; }
            public Stack<T> Stack { get; set; }
            public Queue<T> Queue { get; set; }
            public LinkedList<T> LinkedList { get; set; }
            public ObservableCollection<T> ObservableCollection { get; set; }

            public GenericCollections( bool initializeRandomly )
            {
                this.List = new List<T>();
                this.HashSet = new HashSet<T>();
                this.SortedSet = new SortedSet<T>();
                this.Stack = new Stack<T>();
                this.Queue = new Queue<T>();
                this.LinkedList = new LinkedList<T>();
                this.ObservableCollection = new ObservableCollection<T>();

                if( initializeRandomly )
                    InitializeRandomly();
            }

            private void InitializeRandomly()
            {
                var elementType = typeof( T );

                for( int i = 0; i < 10; i++ )
                {
                    T value = (T)Convert.ChangeType( i,
                        elementType.GetUnderlyingTypeIfNullable() );

                    this.List.Add( value );
                    this.HashSet.Add( value );
                    this.SortedSet.Add( value );
                    this.Stack.Push( value );
                    this.Queue.Enqueue( value );
                    this.LinkedList.AddLast( value );
                    this.ObservableCollection.Add( value );
                }
            }
        }

        [TestMethod]
        public void CollectionItemsAndMembersMapping()
        {
            List<int> source = Enumerable.Range( 0, 10 ).ToList();
            source.Capacity = 100;
            List<double> target = new List<double>() { 1, 2, 3 };

            Assert.IsTrue( !source.SequenceEqual(
                    target.Select( item => (int)item ) ) );

            var ultraMapper = new UltraMapper();
            ultraMapper.Map( source, target );

            Assert.IsTrue( source.SequenceEqual(
                target.Select( item => (int)item ) ) );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void MergeCollections()
        {
            List<int> source = Enumerable.Range( 0, 10 ).ToList();
            source.Capacity = 100;
            List<double> target = new List<double>() { 1, 2, 3 };

            Assert.IsTrue( !source.SequenceEqual(
                    target.Select( item => (int)item ) ) );

            var ultraMapper = new UltraMapper( cfg =>
            {
                cfg.ReferenceMappingStrategy =
                    ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL;
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( source.SequenceEqual(
                target.Select( item => (int)item ) ) );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void PrimitiveCollection()
        {
            var excludeTypes = new TypeCode[]
            {
                TypeCode.Empty,
                TypeCode.DBNull,
                TypeCode.DateTime, //DateTime is not managed
                TypeCode.Object,
                TypeCode.Boolean, //Bool flattens value to 0 or 1 so hashset differ too much. change the verifier to take care of conversions
            };

            var types = Enum.GetValues( typeof( TypeCode ) ).Cast<TypeCode>()
                .Except( excludeTypes )
                .Select( typeCode => TypeExtensions.GetType( typeCode ) ).ToList();

            foreach( var sourceElementType in types )
            {
                foreach( var targetElementType in types )
                {
                    //for the following pairs a conversion is known
                    //to be harder (not possible or convention-based), 
                    //so here we just skip that few cases

                    if( sourceElementType == typeof( string ) &&
                        targetElementType == typeof( bool ) ) continue;

                    if( sourceElementType == typeof( char ) &&
                        targetElementType == typeof( bool ) ) continue;

                    if( sourceElementType == typeof( bool ) &&
                        targetElementType == typeof( char ) ) continue;


                    var sourceType = typeof( GenericCollections<> )
                        .MakeGenericType( sourceElementType );

                    var targetType = typeof( GenericCollections<> )
                        .MakeGenericType( targetElementType );

                    var sourceTypeCtor = ConstructorFactory.CreateConstructor<bool>( sourceType );
                    var targetTypeCtor = ConstructorFactory.CreateConstructor<bool>( targetType );

                    var source = sourceTypeCtor( true );
                    var target = targetTypeCtor( false );

                    var ultraMapper = new UltraMapper();
                    ultraMapper.Map( source, target );

                    bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
                    Assert.IsTrue( isResultOk );
                }
            }
        }

        [TestMethod]
        public void NullablePrimitiveCollection()
        {
            //DateTime is not managed
            var nullableTypes = new Type[]
            {
               // typeof( bool? ),//Bool flattens value to 0 or 1 so hashset differ too much. change the verifier to take care of conversions
                typeof( char? ),
                typeof( sbyte? ),
                typeof( byte? ),
                typeof( int? ),
                typeof( uint? ),
                typeof( int? ),
                typeof( uint? ),
                typeof( int? ),
                typeof( uint? ),
                typeof( float? ),
                typeof( double? ),
                typeof( decimal? ),
                //typeof( string )
            };

            foreach( var sourceElementType in nullableTypes )
            {
                foreach( var targetElementType in nullableTypes )
                {
                    if( sourceElementType == typeof( char? ) &&
                        targetElementType == typeof( bool? ) ) continue;

                    if( sourceElementType == typeof( bool? ) &&
                        targetElementType == typeof( char? ) ) continue;

                    //for the following pairs a conversion is known
                    //to be harder (not possible or convention-based), 
                    //so here we just skip that few cases
                    if( sourceElementType == typeof( bool? ) &&
                        targetElementType == typeof( string ) ) continue;

                    if( sourceElementType == typeof( string ) &&
                        targetElementType == typeof( bool? ) ) continue;

                    var sourceType = typeof( GenericCollections<> )
                        .MakeGenericType( sourceElementType );

                    var targetType = typeof( GenericCollections<> )
                        .MakeGenericType( targetElementType );

                    var sourceTypeCtor = ConstructorFactory.CreateConstructor<bool>( sourceType );
                    var targetTypeCtor = ConstructorFactory.CreateConstructor<bool>( targetType );

                    var source = sourceTypeCtor( true );
                    var target = targetTypeCtor( true );

                    var ultraMapper = new UltraMapper();
                    ultraMapper.Map( source, target );

                    bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
                    Assert.IsTrue( isResultOk );
                }
            }
        }

        [TestMethod]
        public void ComplexCollection()
        {
            var innerType = new InnerType() { String = "test" };

            var source = new GenericCollections<ComplexType>( false );
            for( int i = 0; i < 3; i++ )
            {
                source.List.Add( new ComplexType() { A = i, InnerType = innerType } );
                source.HashSet.Add( new ComplexType() { A = i, InnerType = innerType } );
                source.SortedSet.Add( new ComplexType() { A = i, InnerType = innerType } );
                source.Stack.Push( new ComplexType() { A = i, InnerType = innerType } );
                source.Queue.Enqueue( new ComplexType() { A = i, InnerType = innerType } );
                source.LinkedList.AddLast( new ComplexType() { A = i, InnerType = innerType } );
                source.ObservableCollection.Add( new ComplexType() { A = i, InnerType = innerType } );
            }

            var target = new GenericCollections<ComplexType>( false );

            var ultraMapper = new UltraMapper();
            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );

            Assert.IsTrue( !Object.ReferenceEquals( source.HashSet.First().InnerType, target.HashSet.First().InnerType ) );

            Assert.IsTrue( target.List.Concat( target.HashSet.Concat( target.SortedSet.Concat( target.Stack.Concat(
                target.Queue.Concat( target.LinkedList.Concat( target.ObservableCollection ) ) ) ) ) )
                .Select( it => it.InnerType )
                .All( item => Object.ReferenceEquals( item, target.HashSet.First().InnerType ) ) );
        }

        [TestMethod]
        public void FromPrimitiveCollectionToAnother()
        {
            var sourceProperties = typeof( GenericCollections<int> ).GetProperties();
            var targetProperties = typeof( GenericCollections<double> ).GetProperties();

            var source = new GenericCollections<int>( false );

            //initialize source
            for( int i = 0; i < 50; i++ )
            {
                source.List.Add( i );
                source.HashSet.Add( i );
                source.SortedSet.Add( i );
                source.Stack.Push( i );
                source.Queue.Enqueue( i );
                source.LinkedList.AddLast( i );
                source.ObservableCollection.Add( i );
            }

            foreach( var sourceProp in sourceProperties )
            {
                var target = new GenericCollections<double>( false );

                var ultraMapper = new UltraMapper();
                var typeMappingConfig = ultraMapper.MappingConfiguration.MapTypes( source, target );

                foreach( var targetProp in targetProperties )
                    typeMappingConfig.MapMember( sourceProp, targetProp );

                ultraMapper.Map( source, target );

                bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
                Assert.IsTrue( isResultOk );
            }
        }

        [TestMethod]
        public void FromComplexCollectionToAnother()
        {
            var typeProperties = typeof( GenericCollections<ComplexType> ).GetProperties();

            var source = new GenericCollections<ComplexType>( false );

            //initialize source
            for( int i = 0; i < 50; i++ )
            {
                source.List.Add( new ComplexType() { A = i } );
                source.HashSet.Add( new ComplexType() { A = i } );
                source.SortedSet.Add( new ComplexType() { A = i } );
                source.Stack.Push( new ComplexType() { A = i } );
                source.Queue.Enqueue( new ComplexType() { A = i } );
                source.LinkedList.AddLast( new ComplexType() { A = i } );
                source.ObservableCollection.Add( new ComplexType() { A = i } );
            }

            foreach( var sourceProp in typeProperties )
            {
                var ultraMapper = new UltraMapper( cfg =>
                {
                    //cfg.GlobalConfiguration.IgnoreConventions = true;
                } );

                var target = new GenericCollections<ComplexType>( false );

                var typeMappingConfig = ultraMapper.MappingConfiguration.MapTypes( source, target );
                foreach( var targetProp in typeProperties )
                    typeMappingConfig.MapMember( sourceProp, targetProp );

                ultraMapper.Map( source, target );

                bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
                Assert.IsTrue( isResultOk );
            }
        }

        [TestMethod]
        public void AssignNullCollection()
        {
            var source = new GenericCollections<int>( false )
            {
                List = null,
                HashSet = null,
                SortedSet = null,
                Stack = null,
                Queue = null,
                LinkedList = null,
                ObservableCollection = null
            };

            var target = new GenericCollections<int>( true );

            var ultraMapper = new UltraMapper();
            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void KeepAndClearCollection()
        {
            var source = new GenericCollections<ComplexType>( false );

            //initialize source
            for( int i = 0; i < 50; i++ )
            {
                source.List.Add( new ComplexType() { A = i } );
                source.HashSet.Add( new ComplexType() { A = i } );
                source.SortedSet.Add( new ComplexType() { A = i } );
                source.Stack.Push( new ComplexType() { A = i } );
                source.Queue.Enqueue( new ComplexType() { A = i } );
                source.LinkedList.AddLast( new ComplexType() { A = i } );
                source.ObservableCollection.Add( new ComplexType() { A = i } );
            }

            var target = new GenericCollections<ComplexType>( false )
            {
                List = new List<Tests.CollectionTests.ComplexType>() { new ComplexType() { A = 100 } }
            };

            var ultraMapper = new UltraMapper( cfg =>
            {
                cfg.IgnoreMemberMappingResolvedByConvention = true;

                cfg.MapTypes<ComplexType, ComplexType>( typeCfg =>
                {
                    typeCfg.IgnoreMemberMappingResolvedByConvention = false;
                } );

                cfg.MapTypes<GenericCollections<ComplexType>, GenericCollections<ComplexType>>()
                   .MapMember( a => a.List, b => b.List, memberConfig =>
                   {
                       memberConfig.CollectionMappingStrategy = CollectionMappingStrategies.RESET;
                       memberConfig.ReferenceMappingStrategy = ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL;
                   } );
            } );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void CollectionUpdate()
        {
            var innerType = new InnerType() { String = "test" };

            var source = new GenericCollections<ComplexType>( false );

            //initialize source
            for( int i = 0; i < 50; i++ )
            {
                source.List.Add( new ComplexType() { A = i, InnerType = innerType } );
                source.HashSet.Add( new ComplexType() { A = i } );
                source.SortedSet.Add( new ComplexType() { A = i } );
                source.Stack.Push( new ComplexType() { A = i } );
                source.Queue.Enqueue( new ComplexType() { A = i } );
                source.LinkedList.AddLast( new ComplexType() { A = i } );
                source.ObservableCollection.Add( new ComplexType() { A = i } );
            }

            var target = new GenericCollections<ComplexType>( false );

            var temp = new List<ComplexType>()
            {
                new ComplexType() { A = 1 },
                new ComplexType() { A = 49 },
                new ComplexType() { A = 50 }
            };


            var ultraMapper = new UltraMapper( cfg =>
            {
                cfg.MapTypes( source, target )
                    .MapMember( a => a.List, b => b.List, ( itemA, itemB ) => itemA.A == itemB.A );
            } );

            LinqExtensions.Update( ultraMapper, source.List, temp, new RelayEqualityComparison<ComplexType>( ( itemA, itemB ) => itemA.A == itemB.A ) );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        public static class LinqExtensions
        {
            public static void Update<T>( UltraMapper mapper, IEnumerable<T> source, ICollection<T> target, IEqualityComparer<T> comparer )
                where T : class
            {
                var itemsToRemove = target.Except( source, comparer ).ToList();
                foreach( var item in itemsToRemove ) target.Remove( item );

                List<T> itemsToAdd = new List<T>();
                foreach( var sourceItem in source )
                {
                    bool match = false;
                    foreach( var targetItem in target )
                    {
                        if( comparer.Equals( sourceItem, targetItem ) )
                        {
                            match = true;
                            mapper.Map( sourceItem, targetItem );
                        }
                    }

                    if( !match )
                        itemsToAdd.Add( sourceItem );
                }

                foreach( var item in itemsToAdd ) target.Add( item );
            }
        }

        private class RelayEqualityComparison<T> : IEqualityComparer<T>
        {
            private Func<T, T, bool> _comparer;

            public RelayEqualityComparison( Func<T, T, bool> comparer )
            {
                _comparer = comparer;
            }

            public bool Equals( T x, T y )
            {
                var result = _comparer( x, y );
                return result;
            }

            public int GetHashCode( T obj )
            {
                return -1;
            }
        }
    }
}
