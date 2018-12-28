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

namespace Tests.AsyncEnumeration.Implementation.Provider
{
   [TestClass]
   public class AggregateTests
   {
      private readonly IEnumerable<Int32> _enumerable;

      public AggregateTests()
      {
         this._enumerable = Enumerable.Range( 0, 10 );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestAggregate()
      {
         Int32 DoAggregate( Int32 prev, Int32 cur )
         {
            return prev + cur;
         }
         Assert.AreEqual(
            this._enumerable.Aggregate( DoAggregate ),
            await this._enumerable.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).AggregateAsync( DoAggregate )
            );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestAggregateAsync()
      {
         Assert.AreEqual(
            this._enumerable.Aggregate( ( prev, cur ) => prev + cur ),
            await this._enumerable.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).AggregateAsync( ( prev, cur ) => new ValueTask<Int32>( prev + cur ) )
            );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestAggregateWithSeed()
      {
         Int32 DoAggregate( Int32 prev, Int32 cur )
         {
            return prev + cur;
         }
         Assert.AreEqual(
            this._enumerable.Aggregate( 0, DoAggregate ),
            await this._enumerable.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).AggregateAsync( DoAggregate, 0 )
            );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestAggregateWithSeedAsync()
      {
         Assert.AreEqual(
            this._enumerable.Aggregate( 0, ( prev, cur ) => prev + cur ),
            await this._enumerable.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).AggregateAsync( ( prev, cur ) => new ValueTask<Int32>( prev + cur ), 0 )
            );
      }
   }
}
