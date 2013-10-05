﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.DataStructures;

namespace Examine.LuceneEngine.Faceting
{
    public class FacetCounts : IEnumerable<FacetCount>
    {
        public static int GrowFactor = 2048;
        
        public LittleBigArray Counts { get; private set; }

        public FacetMap FacetMap { get; private set; }

              
        public void Reset(FacetMap map)
        {
            FacetMap = map;            
            if (Counts == null || Counts.Length < map.Keys.Count)
            {
                Counts = new LittleBigArray(GrowFactor * (1 + map.Keys.Count / GrowFactor));
            }
            else
            {
                Counts.Reset();
            }
        }

        public int this[FacetKey key]
        {
            get { return GetCount(key); }
        }

        public int GetCount(FacetKey key)
        {
            var index = FacetMap.GetIndex(key);
            return index > -1 && index < Counts.Length ? Counts[index] : 0;
        }

        public IEnumerable<FacetCount> GetTopFacets(int count, params string[] fieldNames)
        {
            var facets = fieldNames.IsNullOrEmpty() ? GetNonEmpty()
                : FacetMap.GetByFieldNames(fieldNames).Select(f => new FacetCount(f.Value, Counts[f.Key]));

            return facets.GetTopItems(count, 
                
                new LambdaComparer<FacetCount>((x, y) =>
                    {
                        var c = y.Count.CompareTo(x.Count);
                        return c == 0 ? x.Key.CompareTo(y.Key) : c;
                    }));
        } 

        public IEnumerable<FacetCount> GetNonEmpty()
        {
            return Counts.Select(f => new FacetCount(FacetMap.Keys[f.Key], f.Value));
        }

        public IEnumerator<FacetCount> GetEnumerator()
        {
            var n = Counts.Length;
            foreach( var f in FacetMap)
            {
                if (f.Key < n)
                {
                    yield return new FacetCount(f.Value, Counts[f.Key]);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}