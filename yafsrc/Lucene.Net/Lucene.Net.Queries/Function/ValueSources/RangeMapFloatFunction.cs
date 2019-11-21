﻿using YAF.Lucene.Net.Index;
using YAF.Lucene.Net.Queries.Function.DocValues;
using YAF.Lucene.Net.Search;
using YAF.Lucene.Net.Support;
using System.Collections;

namespace YAF.Lucene.Net.Queries.Function.ValueSources
{
    /*
     * Licensed to the Apache Software Foundation (ASF) under one or more
     * contributor license agreements.  See the NOTICE file distributed with
     * this work for additional information regarding copyright ownership.
     * The ASF licenses this file to You under the Apache License, Version 2.0
     * (the "License"); you may not use this file except in compliance with
     * the License.  You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

    /// <summary>
    /// <see cref="RangeMapSingleFunction"/> implements a map function over
    /// another <see cref="ValueSource"/> whose values fall within <c>min</c> and <c>max</c> inclusive to <c>target</c>.
    /// <para/>
    /// Normally used as an argument to a <see cref="FunctionQuery"/>
    /// <para/>
    /// NOTE: This was RangeMapFloatFunction in Lucene
    /// </summary>
    public class RangeMapSingleFunction : ValueSource
    {
        protected readonly ValueSource m_source;
        protected readonly float m_min;
        protected readonly float m_max;
        protected readonly ValueSource m_target;
        protected readonly ValueSource m_defaultVal;

        public RangeMapSingleFunction(ValueSource source, float min, float max, float target, float? def)
            : this(source, min, max, new ConstValueSource(target), def == null ? null : new ConstValueSource(def.Value))
        {
        }

        public RangeMapSingleFunction(ValueSource source, float min, float max, ValueSource target, ValueSource def)
        {
            this.m_source = source;
            this.m_min = min;
            this.m_max = max;
            this.m_target = target;
            this.m_defaultVal = def;
        }

        public override string GetDescription()
        {
            return "map(" + m_source.GetDescription() + "," + m_min + "," + m_max + "," + m_target.GetDescription() + ")";
        }

        public override FunctionValues GetValues(IDictionary context, AtomicReaderContext readerContext)
        {
            FunctionValues vals = m_source.GetValues(context, readerContext);
            FunctionValues targets = m_target.GetValues(context, readerContext);
            FunctionValues defaults = (this.m_defaultVal == null) ? null : m_defaultVal.GetValues(context, readerContext);
            return new SingleDocValuesAnonymousInnerClassHelper(this, this, vals, targets, defaults);
        }

        private class SingleDocValuesAnonymousInnerClassHelper : SingleDocValues
        {
            private readonly RangeMapSingleFunction outerInstance;

            private readonly FunctionValues vals;
            private readonly FunctionValues targets;
            private readonly FunctionValues defaults;

            public SingleDocValuesAnonymousInnerClassHelper(RangeMapSingleFunction outerInstance, RangeMapSingleFunction @this, FunctionValues vals, FunctionValues targets, FunctionValues defaults)
                : base(@this)
            {
                this.outerInstance = outerInstance;
                this.vals = vals;
                this.targets = targets;
                this.defaults = defaults;
            }

            /// <summary>
            /// NOTE: This was floatVal() in Lucene
            /// </summary>
            public override float SingleVal(int doc)
            {
                float val = vals.SingleVal(doc);
                return (val >= outerInstance.m_min && val <= outerInstance.m_max) ? targets.SingleVal(doc) : (outerInstance.m_defaultVal == null ? val : defaults.SingleVal(doc));
            }
            public override string ToString(int doc)
            {
                return "map(" + vals.ToString(doc) + ",min=" + outerInstance.m_min + ",max=" + outerInstance.m_max + ",target=" + targets.ToString(doc) + ")";
            }
        }

        public override void CreateWeight(IDictionary context, IndexSearcher searcher)
        {
            m_source.CreateWeight(context, searcher);
        }

        public override int GetHashCode()
        {
            int h = m_source.GetHashCode();
            h ^= (h << 10) | ((int)((uint)h >> 23));
            h += Number.SingleToInt32Bits(m_min);
            h ^= (h << 14) | ((int)((uint)h >> 19));
            h += Number.SingleToInt32Bits(m_max);
            h += m_target.GetHashCode();
            if (m_defaultVal != null)
            {
                h += m_defaultVal.GetHashCode();
            }
            return h;
        }

        public override bool Equals(object o)
        {
            if (typeof(RangeMapSingleFunction) != o.GetType())
            {
                return false;
            }
            var other = o as RangeMapSingleFunction;
            if (other == null)
                return false;
            return this.m_min == other.m_min 
                && this.m_max == other.m_max 
                && this.m_target.Equals(other.m_target) 
                && this.m_source.Equals(other.m_source) 
                && (this.m_defaultVal == other.m_defaultVal || (this.m_defaultVal != null && this.m_defaultVal.Equals(other.m_defaultVal)));
        }
    }
}