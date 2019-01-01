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
using UtilPack;

namespace Tests.AsyncEnumeration.Implementation.Enumerable
{
   [TestClass]
   public class EnumerableTests
   {
      [TestMethod, Timeout( 1000 )]
      public Task TestSequentialEnumerationStartInfoInt32()
      {
         return TestSequentialEnumerationStartInfo( 123 );
      }

      [TestMethod, Timeout( 1000 )]
      public Task TestSequentialEnumerationStartInfoInt64()
      {
         return TestSequentialEnumerationStartInfo( 123L );
      }

      [TestMethod, Timeout( 1000 )]
      public Task TestSequentialEnumerationStartInfoSingle()
      {
         return TestSequentialEnumerationStartInfo( 12.3f );
      }

      [TestMethod, Timeout( 1000 )]
      public Task TestSequentialEnumerationStartInfoDouble()
      {
         return TestSequentialEnumerationStartInfo( 12.3d );
      }

      [TestMethod, Timeout( 1000 )]
      public Task TestSequentialEnumerationStartInfoObject()
      {
         return TestSequentialEnumerationStartInfo( new Object() );
      }

      private static async Task TestSequentialEnumerationStartInfo<T>(
         T val,
         Func<SequentialEnumerationStartInfo<T>> createStartInfo = null
         )
      {
         var continueLoop = true;
         var disposeCalled = false;
         var seen = await AsyncEnumerationFactory.CreateSequentialEnumerable(
            () => AsyncEnumerationFactory.CreateSequentialStartInfo( () => { var doContinueLoop = continueLoop; continueLoop = false; return new ValueTask<(Boolean, T)>( (doContinueLoop, val) ); }, () => { disposeCalled = true; return Task.CompletedTask; } ),
            DefaultAsyncProvider.Instance
            ).FirstAsync();
         Assert.IsFalse( continueLoop );
         Assert.IsTrue( disposeCalled );
         Assert.AreEqual( val, seen );
      }
   }

   [TestClass]
   public class ExclusiveEnumerableTests
   {
      [TestMethod, Timeout( 1000 )]
      public async Task TestExclusiveEnumerable()
      {
         var continueLoop = true;
         var disposeCalled = false;
         var val = 123;
         var seen = await AsyncEnumerationFactory.CreateExclusiveSequentialEnumerable(
            AsyncEnumerationFactory.CreateSequentialStartInfo( () => { var doContinueLoop = continueLoop; continueLoop = false; return new ValueTask<(Boolean, Int32)>( (doContinueLoop, val) ); }, () => { disposeCalled = true; return Task.CompletedTask; } ),
            DefaultAsyncProvider.Instance
            ).FirstAsync();
         Assert.IsFalse( continueLoop );
         Assert.IsTrue( disposeCalled );
         Assert.AreEqual( val, seen );
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestExclusiveEnumerableConcurrent()
      {
         using ( var sema = new SemaphoreSlim( 0, 1 ) )
         {
            var enumerator = CreateAwaitingEnumerator( sema );
            var concurrentInvocation = InvokeEnumerableConcurrently( sema, enumerator );
            var completedTask = await Task.WhenAny( EnumerateEnumerator( enumerator ), concurrentInvocation );
            Assert.AreSame( concurrentInvocation, completedTask );
            if ( !concurrentInvocation.IsCompletedSuccessfully )
            {
               Assert.IsTrue( false, "Concurrent invocation failed with " + concurrentInvocation.Exception );
            }
         }
      }

      [TestMethod, Timeout( 1000 )]
      public async Task TestStatelessFunctionalWrapper()
      {
         var array = await AsyncEnumerationFactory.FromGeneratorCallback( () =>
         {
            var isContinuing = true;
            return AsyncEnumerationFactory.CreateWrappingEnumerator( AsyncEnumerationFactory.CreateWrappingStartInfo(
               () => TaskUtils.TaskFromBoolean( isContinuing ),
               ( out Boolean success ) => { isContinuing = false; success = false; return 0; },
               () => Task.CompletedTask ) );
         }, DefaultAsyncProvider.Instance )
         .ToArrayAsync();
         Assert.AreEqual( 0, array.Length );

      }

      private static IAsyncEnumerator<Int32> CreateAwaitingEnumerator( SemaphoreSlim sema )
      {
         return AsyncEnumerationFactory.CreateExclusiveSequentialEnumerable(
            AsyncEnumerationFactory.CreateSequentialStartInfo<Int32>( async () =>
            {
               // Signal the other task that we are inside the call now
               sema.Release();

               // Now just wait 
               while ( true )
               {
                  await Task.Delay( 100 );
               }
               throw new NotSupportedException( "This method should never be allowed to complete." );
            },
            () =>
            {
               throw new NotSupportedException( "This method should never be called" );
            } ),
            DefaultAsyncProvider.Instance
            ).GetAsyncEnumerator();
      }

      private static async Task EnumerateEnumerator( IAsyncEnumerator<Int32> enumerator )
      {
         try
         {
            while ( await enumerator.WaitForNextAsync() )
            {
               Boolean success;
               do
               {
                  var item = enumerator.TryGetNext( out success );
               } while ( success );
            }
         }
         finally
         {
            await enumerator.DisposeAsync();
         }
      }

      private static async Task InvokeEnumerableConcurrently( SemaphoreSlim sema, IAsyncEnumerator<Int32> enumerator )
      {
         // Wait till enumerator starts
         await sema.WaitAsync();

         // Now invoke the method that should throw
         await Assert.ThrowsExceptionAsync<InvalidOperationException>( () => enumerator.WaitForNextAsync() );

         // Also check the non-throwing method
         enumerator.TryGetNext( out var success );
         Assert.IsFalse( success );
      }
   }
}
