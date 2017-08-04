// Copyright (c) 2017 Jan Pluskal
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System.Collections.Generic;

namespace Netfox.AppIdent.Misc
{
    public sealed class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
    {
        // You could make this a per-instance field with a constructor parameter
        private static readonly EqualityComparer<T> elementComparer = EqualityComparer<T>.Default;

        public bool Equals(T[] first, T[] second)
        {
            if(first == second) { return true; }
            if(first == null || second == null) { return false; }
            if(first.Length != second.Length) { return false; }
            for(var i = 0; i < first.Length; i++) { if(!elementComparer.Equals(first[i], second[i])) { return false; } }
            return true;
        }

        public int GetHashCode(T[] array)
        {
            unchecked
            {
                if(array == null) { return 0; }
                var hash = 17;
                foreach(var element in array) { hash = hash * 31 + elementComparer.GetHashCode(element); }
                return hash;
            }
        }
    }
}