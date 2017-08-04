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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Netfox.Core.Database
{
    public interface IRepository<T> //: IEnumerable<T>
    {
        T Single(Guid key);
        T SingleOrDefault(Guid key);
        bool Exists(Guid key);
        Task<T> InsertAsync(T entity);
        T Insert(T entity);
        Task UpdateAsync(T entity);
        void Update(T entity);
        void Delete(T entity);

        //IEnumerable<T> List { get; }
        T FindById(Guid Id);
    }
}