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
using AsyncEnumeration.Implementation.Enumerable;
using AsyncEnumeration.Implementation.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SyncEnumerable = System.Linq.Enumerable;

namespace Tests.AsyncEnumeration.Implementation.Enumerable
{
   [TestClass]
   public class GenerationTests
   {
      [TestMethod, Timeout( 1000 )]
      public async Task TestRepeat()
      {
         var item = 123;
         var count = 10;
         Assert.IsTrue( SyncEnumerable.Repeat( item, count ).SequenceEqual(
            await AsyncEnumerable.Repeat( item, count, DefaultAsyncProvider.Instance ).ToArrayAsync()
            ) );

         Int64 count64 = count;
         Assert.IsTrue( SyncEnumerable.Repeat( item, count ).SequenceEqual(
            await AsyncEnumerable.Repeat( item, count64, DefaultAsyncProvider.Instance ).ToArrayAsync()
            ) );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestRepeatLeq0()
      {
         var item = 123;
         Assert.AreEqual( 0, ( await AsyncEnumerable.Repeat( item, 0, DefaultAsyncProvider.Instance ).ToArrayAsync() ).Length );
         Assert.AreEqual( 0, ( await AsyncEnumerable.Repeat( item, 0L, DefaultAsyncProvider.Instance ).ToArrayAsync() ).Length );
         Assert.AreEqual( 0, ( await AsyncEnumerable.Repeat( item, -1, DefaultAsyncProvider.Instance ).ToArrayAsync() ).Length );
         Assert.AreEqual( 0, ( await AsyncEnumerable.Repeat( item, -1L, DefaultAsyncProvider.Instance ).ToArrayAsync() ).Length );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestRepeatWithGenerator()
      {
         var item = 123;
         var count = 10;
         Assert.IsTrue( SyncEnumerable.Repeat( item, count ).SequenceEqual(
            await AsyncEnumerable.Repeat( () => item, count, DefaultAsyncProvider.Instance ).ToArrayAsync()
            ) );

         Int64 count64 = count;
         Assert.IsTrue( SyncEnumerable.Repeat( item, count ).SequenceEqual(
            await AsyncEnumerable.Repeat( () => item, count64, DefaultAsyncProvider.Instance ).ToArrayAsync()
            ) );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestRepeatWithGeneratorAsync()
      {
         var item = 123;
         var count = 10;
         Assert.IsTrue( SyncEnumerable.Repeat( item, count ).SequenceEqual(
            await AsyncEnumerable.Repeat( () => new ValueTask<Int32>( item ), count, DefaultAsyncProvider.Instance ).ToArrayAsync()
            ) );

         Int64 count64 = count;
         Assert.IsTrue( SyncEnumerable.Repeat( item, count ).SequenceEqual(
            await AsyncEnumerable.Repeat( () => new ValueTask<Int32>( item ), count64, DefaultAsyncProvider.Instance ).ToArrayAsync()
            ) );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestRangeAscending()
      {
         var start = -5;
         var end = 10;
         Assert.IsTrue( SyncEnumerable.Range( start, end - start ).SequenceEqual(
            await AsyncEnumerable.Range( start, end, DefaultAsyncProvider.Instance ).ToArrayAsync()
            ) );

         Assert.IsTrue( SyncEnumerable.Range( start, end - start ).SequenceEqual(
            await AsyncEnumerable.Range( (Int64) start, (Int64) end, DefaultAsyncProvider.Instance ).Select( i => (Int32) i ).ToArrayAsync()
         ) );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestRangeDescending()
      {
         var start = -5;
         var end = 10;
         Assert.IsTrue( SyncEnumerable.Range( start + 1, end - start ).Reverse().SequenceEqual(
            await AsyncEnumerable.Range( end, start, DefaultAsyncProvider.Instance ).ToArrayAsync()
            ) );

         Assert.IsTrue( SyncEnumerable.Range( start + 1, end - start ).Reverse().SequenceEqual(
            await AsyncEnumerable.Range( (Int64) end, (Int64) start, DefaultAsyncProvider.Instance ).Select( i => (Int32) i ).ToArrayAsync()
         ) );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestRangeEmpty()
      {
         Assert.AreEqual( 0, ( await AsyncEnumerable.Range( 0, 0, DefaultAsyncProvider.Instance ).ToArrayAsync() ).Length );
         Assert.AreEqual( 0, ( await AsyncEnumerable.Range( 0L, 0L, DefaultAsyncProvider.Instance ).ToArrayAsync() ).Length );
      }
   }
}
