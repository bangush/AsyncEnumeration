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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilPack;

namespace Tests.AsyncEnumeration.Abstractions
{
   [TestClass]
   public class EmptyTests
   {
      [TestMethod]
      public void TestEmptyEnumerator()
      {
         var enumerator = EmptyAsync<Int32>.Enumerator;
         AssertCompletedTask( enumerator.WaitForNextAsync(), false );
         var value = enumerator.TryGetNext( out var success );
         Assert.IsFalse( success );
         Assert.AreEqual( default, value );
      }

      [TestMethod]
      public async Task TestEmptyEnumerableEnumeration()
      {
         var enumerable = EmptyAsync<Int32>.Enumerable;
         var callbackCalled = false;
         await enumerable.EnumerateAsync( item => callbackCalled = true );
         Assert.IsFalse( callbackCalled );
      }

      [TestMethod]
      public void TestEmptyEnumerableALINQ()
      {
         var enumerable = EmptyAsync<Int32>.Enumerable;
         var val = 123;

         Assert.ThrowsException<InvalidOperationException>( () => enumerable.AggregateAsync( ( x, y ) => x + y ) );
         Assert.ThrowsException<InvalidOperationException>( () => enumerable.AggregateAsync( ( x, y ) => new ValueTask<Int32>( x + y ) ) );
         AssertCompletedTask( enumerable.AggregateAsync( ( x, y ) => x + y, val ), val );
         AssertCompletedTask( enumerable.AggregateAsync( ( x, y ) => new ValueTask<Int32>( x + y ), val ), val );
         AssertCompletedTask( enumerable.AllAsync( x => x == val ), true );
         AssertCompletedTask( enumerable.AllAsync( x => new ValueTask<Boolean>( x == val ) ), true );
         AssertCompletedTask( enumerable.AnyAsync( x => x != val ), false );
         AssertCompletedTask( enumerable.AnyAsync( x => new ValueTask<Boolean>( x != val ) ), false );
         Assert.ThrowsException<InvalidOperationException>( () => enumerable.FirstAsync() );
         AssertCompletedTask( enumerable.FirstOrDefaultAsync(), default );
         Assert.AreSame( enumerable, enumerable.Of().Type<Int32>() );
         Assert.AreSame( EmptyAsync<String>.Enumerable, enumerable.Of().Type<String>() );
         Assert.AreSame( enumerable, enumerable.Select( x => x + 1 ) );
         Assert.AreSame( enumerable, enumerable.Select( x => new ValueTask<Int32>( x + 1 ) ) );
         Assert.AreSame( enumerable, enumerable.SelectMany( x => new[] { x, x + 1 } ) );
         Assert.AreSame( enumerable, enumerable.SelectMany( x => Task.FromResult<IEnumerable<Int32>>( new[] { x, x + 1 } ) ) );
         Assert.AreSame( enumerable, enumerable.SelectMany( x => EmptyAsync<Int32>.Enumerable ) );
         Assert.AreSame( enumerable, enumerable.SelectMany( x => Task.FromResult( EmptyAsync<Int32>.Enumerable ) ) );
         Assert.AreSame( enumerable, enumerable.Skip( 1 ) );
         Assert.AreSame( enumerable, enumerable.Skip( 1L ) );
         Assert.AreSame( enumerable, enumerable.SkipWhile( x => true ) );
         Assert.AreSame( enumerable, enumerable.SkipWhile( x => new ValueTask<Boolean>( true ) ) );
         Assert.AreSame( enumerable, enumerable.Take( 1 ) );
         Assert.AreSame( enumerable, enumerable.Take( 1L ) );
         Assert.AreSame( enumerable, enumerable.TakeWhile( x => true ) );
         Assert.AreSame( enumerable, enumerable.TakeWhile( x => TaskUtils.True ) );
         Assert.AreSame( enumerable, enumerable.Where( x => true ) );
         Assert.AreSame( enumerable, enumerable.Where( x => TaskUtils.True ) );
      }

      private static void AssertCompletedTask<T>( Task<T> task, T value )
      {
         Assert.IsTrue( task.IsCompletedSuccessfully );
         Assert.AreEqual( value, task.Result );
      }
   }
}
