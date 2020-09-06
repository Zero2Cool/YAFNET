﻿using YAF.Lucene.Net.Search;
using System.Collections.Generic;

namespace YAF.Lucene.Net.QueryParsers.Surround.Query
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
    /// Factory for prohibited clauses
    /// </summary>
    public class NotQuery : ComposedQuery
    {
        public NotQuery(IList<SrndQuery> queries, string opName)
            : base(queries, true /* infix */, opName)
        {
        }

        public override Search.Query MakeLuceneQueryFieldNoBoost(string fieldName, BasicQueryFactory qf)
        {
            var luceneSubQueries = MakeLuceneSubQueriesField(fieldName, qf);
            BooleanQuery bq = new BooleanQuery
            {
                { luceneSubQueries.Count > 0 ? luceneSubQueries[0] : null, Occur.MUST }
            };

            // LUCENENET: SubList() is slow, so we do an array copy operation instead
            var luceneSubQueriesArray = new Search.Query[luceneSubQueries.Count - 1];
            for (int i = 1, j = 0; i < luceneSubQueries.Count; i++, j++)
                luceneSubQueriesArray[j] = luceneSubQueries[i];

            SrndBooleanQuery.AddQueriesToBoolean(bq,
                    // FIXME: do not allow weights on prohibited subqueries.
                    luceneSubQueriesArray,
                // later subqueries: not required, prohibited
                    Occur.MUST_NOT);
            return bq;
        }
    }
}
