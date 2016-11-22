﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.MappingConventions.PropertyMatchingRules
{
    /// <summary>
    /// Two properties match if targetName = sourceName + suffix.
    /// Name case can be optionally ignored.
    /// </summary>
    public class SuffixMatching : PropertyMatchingRuleBase
    {
        public bool IgnoreCase { get; set; }
        public string[] Suffixes { get; set; }

        public SuffixMatching()
            : this( new string[] { "Dto", "DataTransferObject" } ) { }

        public SuffixMatching( params string[] suffixes )
        {
            this.IgnoreCase = false;
            this.Suffixes = suffixes;
        }

        public override bool IsCompliant( PropertyInfo source, PropertyInfo target )
        {
            var comparisonType = this.IgnoreCase ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return this.Suffixes.Any( ( suffix ) =>
                target.Name.Equals( source.Name + suffix, comparisonType ) );
        }
    }
}
