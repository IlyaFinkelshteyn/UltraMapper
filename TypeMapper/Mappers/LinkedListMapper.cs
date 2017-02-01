﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class LinkedListMapper : CollectionMapper
    {
        public override bool CanHandle( MemberMapping mapping )
        {
            return mapping.TargetProperty.MemberInfo.GetMemberType()
                .GetGenericTypeDefinition() == typeof( LinkedList<> );
        }

        protected override MethodInfo GetTargetCollectionAddMethod( CollectionMapperContext context )
        {
            return context.TargetPropertyType.GetMethod(
                "AddLast", new[] { context.TargetElementType } );
        }
    }
}