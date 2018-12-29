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
   public class SkipTests
   {

      private readonly IEnumerable<Int32> _enumerable;

      public SkipTests()
      {
         this._enumerable = Enumerable.Range( 0, 10 );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestSkip()
      {
         Assert.IsTrue(
            this._enumerable.Skip( 5 ).SequenceEqual(
               await this._enumerable.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).Skip( 5 ).ToArrayAsync()
               )
            );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestSkip64()
      {
         Assert.IsTrue(
            this._enumerable.Skip( 5 ).SequenceEqual(
               await this._enumerable.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).Skip( 5L ).ToArrayAsync()
               )
            );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestSkipWhile()
      {
         Boolean SkipTest( Int32 i )
         {
            return i < 5;
         }
         Assert.IsTrue(
            this._enumerable.SkipWhile( SkipTest ).SequenceEqual(
               await this._enumerable.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).SkipWhile( SkipTest ).ToArrayAsync()
               )
            );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestSkipWhileAsync()
      {
         Boolean SkipTest( Int32 i )
         {
            return i < 5;
         }
         Assert.IsTrue(
            this._enumerable.SkipWhile( SkipTest ).SequenceEqual(
               await this._enumerable.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).SkipWhile( i => new ValueTask<Boolean>( SkipTest( i ) ) ).ToArrayAsync()
               )
            );
      }
   }
}
