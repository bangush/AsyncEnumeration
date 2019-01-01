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

namespace Tests.AsyncEnumeration.Implementation.Enumerable
{
   [TestClass]
   public class SingletonTests
   {
      [TestMethod, Timeout( 1000 )]
      public Task TestSingleton()
      {
         var val = 123;
         return AssertSingletonEnumerable( val.AsSingletonAsync( DefaultAsyncProvider.Instance ), val );
      }

      [TestMethod, Timeout( 1000 )]
      public Task TestSingletonAsync()
      {
         var val = 123;
         async Task<Int32> GetVal()
         {
            await Task.Delay( 100 );
            return val;
         }
         return AssertSingletonEnumerable( GetVal().AsSingletonAsync( DefaultAsyncProvider.Instance ), val );
      }

      [TestMethod, Timeout( 1000 )]
      public Task TestSingletonAsyncValueTask()
      {
         var val = 123;
         async ValueTask<Int32> GetVal()
         {
            await Task.Delay( 100 );
            return val;
         }
         return AssertSingletonEnumerable( GetVal().AsSingletonAsync( DefaultAsyncProvider.Instance ), val );
      }

      private static async Task AssertSingletonEnumerable<T>( IAsyncEnumerable<T> enumerable, T value )
      {
         var array = await enumerable.ToArrayAsync();
         Assert.AreEqual( 1, array.Length );
         Assert.AreEqual( value, array[0] );
      }
   }
}
