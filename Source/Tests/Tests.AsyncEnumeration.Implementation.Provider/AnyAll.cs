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
using AsyncEnumeration.Abstractions;
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
   public class AnyAllTests
   {
      private readonly IEnumerable<Int32> _notEmpty;
      private readonly IEnumerable<Int32> _empty;

      public AnyAllTests()
      {
         this._notEmpty = Enumerable.Repeat( 1, 1 );
         this._empty = Empty<Int32>.Enumerable;
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestAny()
      {
         Assert.AreEqual(
            this._notEmpty.Any(),
            await this._notEmpty.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).AnyAsync()
            );
         Assert.AreEqual(
            this._empty.Any(),
            await this._empty.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).AnyAsync()
            );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestAnyWithArgument()
      {
         Boolean AnyTest( Int32 val )
         {
            return val == this._notEmpty.First();
         }
         Assert.AreEqual(
            this._notEmpty.Any( AnyTest ),
            await this._notEmpty.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).AnyAsync( AnyTest )
            );

         Boolean AnyTestInverted( Int32 val )
         {
            return val != this._notEmpty.First();
         }
         Assert.AreEqual(
            this._notEmpty.Any( AnyTestInverted ),
            await this._notEmpty.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).AnyAsync( AnyTestInverted )
            );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestAnyWithArgumentAsync()
      {
         Assert.AreEqual(
            this._notEmpty.Any( val => val == this._notEmpty.First() ),
            await this._notEmpty.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).AnyAsync( val => new ValueTask<Boolean>( val == this._notEmpty.First() ) )
            );

         Assert.AreEqual(
            this._notEmpty.Any( val => val != this._notEmpty.First() ),
            await this._notEmpty.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).AnyAsync( val => new ValueTask<Boolean>( val != this._notEmpty.First() ) )
            );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestAll()
      {
         Boolean AllTest( Int32 val )
         {
            return val == this._notEmpty.First();
         }
         Assert.AreEqual(
            this._notEmpty.All( AllTest ),
            await this._notEmpty.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).AllAsync( AllTest )
            );

         Boolean AllTestInverted( Int32 val )
         {
            return val == this._notEmpty.First();
         }
         Assert.AreEqual(
            this._notEmpty.All( AllTestInverted ),
            await this._notEmpty.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).AllAsync( AllTestInverted )
            );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestAllAsync()
      {
         Assert.AreEqual(
            this._notEmpty.All( val => val == this._notEmpty.First() ),
            await this._notEmpty.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).AllAsync( val => new ValueTask<Boolean>( val == this._notEmpty.First() ) )
            );

         Assert.AreEqual(
            this._notEmpty.All( val => val != this._notEmpty.First() ),
            await this._notEmpty.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).AllAsync( val => new ValueTask<Boolean>( val != this._notEmpty.First() ) )
            );
      }
   }
}
