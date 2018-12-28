/*
 * Copyright 2018 Stanislav Muhametsin. All rights Reserved.
 *
 * Licensed  under the  Apache License,  Version 2.0  (the "License");
 * you may not use  this file  except in  compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed  under the  License is distributed on an "AS IS" BASIS,
 * WITHOUT  WARRANTIES OR CONDITIONS  OF ANY KIND, either  express  or
 * implied.
 *
 * See the License for the specific language governing permissions and
 * limitations under the License. 
 */
using AsyncEnumeration;
using AsyncEnumeration.Implementation.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilPack;

namespace Tests.AsyncEnumeration.Implementation.Provider
{
   [TestClass]
   public class FirstTests
   {
      private readonly IEnumerable<Int32> _notEmpty;
      private readonly IEnumerable<Int32> _empty;

      public FirstTests()
      {
         this._notEmpty = Enumerable.Repeat( 1, 1 );
         this._empty = Empty<Int32>.Enumerable;
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestFirst()
      {
         Assert.AreEqual(
            this._notEmpty.First(),
            await this._notEmpty.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).FirstAsync()
            );

         Assert.ThrowsException<InvalidOperationException>( () => this._empty.First() );
         await Assert.ThrowsExceptionAsync<InvalidOperationException>( async () => await this._empty.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).FirstAsync() );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestFirstOrDefault()
      {
         Assert.AreEqual(
            this._notEmpty.FirstOrDefault(),
            await this._notEmpty.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).FirstOrDefaultAsync()
            );

         Assert.AreEqual(
            this._empty.FirstOrDefault(),
            await this._empty.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).FirstOrDefaultAsync()
            );
      }
   }
}
