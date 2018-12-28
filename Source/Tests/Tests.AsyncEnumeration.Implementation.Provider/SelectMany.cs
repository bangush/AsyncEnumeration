/*
 * Copyright 2017 Stanislav Muhametsin. All rights Reserved.
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
using AsyncEnumeration.Implementation.Enumerable;
using AsyncEnumeration.Implementation.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UtilPack;

namespace Tests.AsyncEnumeration.Implementation.Provider
{
   [TestClass]
   public class SelectManyTests
   {
      [TestMethod]
      public async Task TestSelectMany1()
      {
         var array = await AsyncEnumerable.Range( 0, 1, DefaultAsyncProvider.Instance ).SelectMany( x => new[] { x, x + 1 } ).ToArrayAsync();
         Assert.IsTrue( ArrayEqualityComparer<Int32>.ArrayEquality( array, new[] { 0, 1 } ) );
      }

      [TestMethod]
      public async Task TestSelectMany2()
      {
         var array = await AsyncEnumerable.Range( 0, 1, DefaultAsyncProvider.Instance ).SelectMany( x => AsyncEnumerable.Range( x, x + 2, DefaultAsyncProvider.Instance ) ).ToArrayAsync();
         Assert.IsTrue( ArrayEqualityComparer<Int32>.ArrayEquality( array, new[] { 0, 1 } ) );
      }

      [TestMethod]
      public async Task TestSelectMany3()
      {
         var array = await AsyncEnumerable.Range( 0, 1, DefaultAsyncProvider.Instance ).SelectMany( async x =>
         {
            await Task.Delay( 100 );
            return new[] { x, x + 1 } as IEnumerable<Int32>;
         } ).ToArrayAsync();
         Assert.IsTrue( ArrayEqualityComparer<Int32>.ArrayEquality( array, new[] { 0, 1 } ) );
      }

      [TestMethod]
      public async Task TestSelectMany4()
      {
         var array = await AsyncEnumerable.Range( 0, 1, DefaultAsyncProvider.Instance ).SelectMany( async x =>
         {
            await Task.Delay( 100 );
            return AsyncEnumerable.Range( x, x + 2, DefaultAsyncProvider.Instance );
         } ).ToArrayAsync();
         Assert.IsTrue( ArrayEqualityComparer<Int32>.ArrayEquality( array, new[] { 0, 1 } ) );
      }
   }
}
