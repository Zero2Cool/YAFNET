using System.Collections.Generic;
using JCG = J2N.Collections.Generic;

namespace YAF.Lucene.Net.Store
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
    /// Implements <see cref="LockFactory"/> for a single in-process instance,
    /// meaning all locking will take place through this one instance.
    /// Only use this <see cref="LockFactory"/> when you are certain all
    /// <see cref="Index.IndexReader"/>s and <see cref="Index.IndexWriter"/>s for a given index are running
    /// against a single shared in-process <see cref="Directory"/> instance.  This is
    /// currently the default locking for <see cref="RAMDirectory"/>.
    /// </summary>
    /// <seealso cref="LockFactory"/>
    public class SingleInstanceLockFactory : LockFactory
    {
        private JCG.HashSet<string> locks = new JCG.HashSet<string>();

        public override Lock MakeLock(string lockName)
        {
            // We do not use the LockPrefix at all, because the private
            // HashSet instance effectively scopes the locking to this
            // single Directory instance.
            return new SingleInstanceLock(locks, lockName);
        }

        public override void ClearLock(string lockName)
        {
            lock (locks)
            {
                if (locks.Contains(lockName))
                {
                    locks.Remove(lockName);
                }
            }
        }
    }

    internal class SingleInstanceLock : Lock
    {
        internal string lockName;
        private JCG.HashSet<string> locks;

        public SingleInstanceLock(JCG.HashSet<string> locks, string lockName)
        {
            this.locks = locks;
            this.lockName = lockName;
        }

        public override bool Obtain()
        {
            lock (locks)
            {
                return locks.Add(lockName);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (locks)
                {
                    locks.Remove(lockName);
                }
            }
        }

        public override bool IsLocked()
        {
            lock (locks)
            {
                return locks.Contains(lockName);
            }
        }

        public override string ToString()
        {
            return base.ToString() + ": " + lockName;
        }
    }
}