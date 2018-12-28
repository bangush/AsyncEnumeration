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
   public class OfTypeTests
   {
      private readonly IEnumerable<String> _enumerable;
      private readonly IEnumerable<Object> _diffTypeEnumerable;

      public OfTypeTests()
      {
         this._enumerable = Enumerable.Repeat( "Test", 1 );
         this._diffTypeEnumerable = new Object[] { "StringValue", 42 };
      }

      [TestMethod]
      public async Task TestOfType()
      {
         IEnumerable<Object> source = this._enumerable;
         Assert.IsTrue(
            source.OfType<String>().SequenceEqual(
               await source.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).Of().Type<String>().ToArrayAsync()
               )
            );
      }

      [TestMethod]
      public void TestOfTypeSameType()
      {
         var asyncEnumerable = this._enumerable.AsAsyncEnumerable( DefaultAsyncProvider.Instance );
         Assert.AreSame(
            asyncEnumerable.Of().Type<Object>(),
            asyncEnumerable
            );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestOfTypeDifferentTypes()
      {
         Assert.IsTrue(
            this._diffTypeEnumerable.OfType<String>().SequenceEqual(
               await this._diffTypeEnumerable.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).Of().Type<String>().ToArrayAsync()
               )
            );

         Assert.IsTrue(
         this._diffTypeEnumerable.OfType<Int32>().SequenceEqual(
            await this._diffTypeEnumerable.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).Of().Type<Int32>().ToArrayAsync()
            )
         );

         Assert.IsTrue(
         this._diffTypeEnumerable.OfType<Char>().SequenceEqual(
            await this._diffTypeEnumerable.AsAsyncEnumerable( DefaultAsyncProvider.Instance ).Of().Type<Char>().ToArrayAsync()
            )
         );
      }
   }
}
