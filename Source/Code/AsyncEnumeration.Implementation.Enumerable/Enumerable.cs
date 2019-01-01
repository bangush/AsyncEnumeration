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
using AsyncEnumeration.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UtilPack;

namespace AsyncEnumeration.Implementation.Enumerable
{
   internal sealed class AsyncSequentialOnlyEnumerable<T> : IAsyncEnumerable<T>
   {
      private readonly Func<SequentialEnumerationStartInfo<T>> _enumerationStart;

      public AsyncSequentialOnlyEnumerable(
         Func<SequentialEnumerationStartInfo<T>> enumerationStart,
         IAsyncProvider asyncProvider
         )
      {
         this._enumerationStart = ArgumentValidator.ValidateNotNull( nameof( enumerationStart ), enumerationStart );
         this.AsyncProvider = ArgumentValidator.ValidateNotNull( nameof( asyncProvider ), asyncProvider );
      }

      public IAsyncEnumerator<T> GetAsyncEnumerator()
      {
         var startInfo = this._enumerationStart();
         return AsyncEnumerationFactory.CreateSequentialEnumerator( startInfo.MoveNext, startInfo.Dispose );
      }

      public IAsyncProvider AsyncProvider { get; }
   }

   internal abstract class AbstractAsyncEnumerator<T> : IAsyncEnumerator<T>
   {
      private const Int32 STATE_INITIAL = 0;
      private const Int32 MOVE_NEXT_STARTED = 1;
      private const Int32 MOVE_NEXT_ENDED = 2;
      private const Int32 STATE_ENDED = 3;
      private const Int32 DISPOSING = 4;
      private const Int32 DISPOSED = 5;

      private const Int32 MOVE_NEXT_STARTED_CURRENT_NOT_READ = 6;
      private const Int32 MOVE_NEXT_STARTED_CURRENT_READING = 7;

      private Int32 _state;
      private readonly SequentialEnumeratorCurrentInfo<T> _current;

      public AbstractAsyncEnumerator(
         SequentialEnumeratorCurrentInfo<T> currentInfo
         )
      {
         this._state = STATE_INITIAL;
         this._current = ArgumentValidator.ValidateNotNull( nameof( currentInfo ), currentInfo );
      }

      //public Boolean IsConcurrentEnumerationSupported => false;

      public async Task<Boolean> WaitForNextAsync()
      {
         // We can call move next only in initial state, or after we have called it once
         var success = false;
         //Int32 prevState;
         if (
            /*( prevState = */Interlocked.CompareExchange( ref this._state, MOVE_NEXT_STARTED, MOVE_NEXT_ENDED )/* ) */== MOVE_NEXT_ENDED // TryGetNext was called and returned false
            || /*( prevState = */Interlocked.CompareExchange( ref this._state, MOVE_NEXT_STARTED, MOVE_NEXT_STARTED_CURRENT_NOT_READ ) /* ) */ == MOVE_NEXT_STARTED_CURRENT_NOT_READ // TryGetNext was not called
            || /*( prevState = */Interlocked.CompareExchange( ref this._state, MOVE_NEXT_STARTED, STATE_INITIAL ) /* ) */ == STATE_INITIAL // Initial call
            )
         {
            T current = default;
            try
            {

               var moveNext = this._current.MoveNext;
               if ( moveNext == null )
               {
                  success = false;
               }
               else
               {
                  (success, current) = await moveNext();
                  if ( success )
                  {
                     this._current.Current = current;
                  }
               }
            }
            finally
            {
               Interlocked.Exchange( ref this._state, success ? MOVE_NEXT_STARTED_CURRENT_NOT_READ : STATE_ENDED );
            }
         }
         else
         {
            // Re-entrancy or concurrent with Reset -> exception
            // TODO -> Maybe use await + Interlocked.CompareExchange-loop to wait... ? Waiting is always prone to deadlocks though.
            throw new InvalidOperationException( "Tried to concurrently move to next or reset." );
         }

         return success;
      }

      public T TryGetNext( out Boolean success )
      {
         var cur = this._current;
         success = Interlocked.CompareExchange( ref this._state, MOVE_NEXT_STARTED_CURRENT_READING, MOVE_NEXT_STARTED_CURRENT_NOT_READ ) == MOVE_NEXT_STARTED_CURRENT_NOT_READ;
         if ( success )
         {
            try
            {
               return cur.Current;
            }
            finally
            {
               Interlocked.Exchange( ref this._state, MOVE_NEXT_ENDED );
            }
         }
         else
         {
            return default;
         }

      }

      public virtual async Task DisposeAsync()
      {
         // We can dispose from STATE_INITIAL, MOVE_NEXT_ENDED, STATE_ENDED, and MOVE_NEXT_STARTED_CURRENT_NOT_READ states
         Int32 prevState;
         if (
            ( prevState = Interlocked.CompareExchange( ref this._state, DISPOSING, STATE_ENDED ) ) == STATE_ENDED
            || ( prevState = Interlocked.CompareExchange( ref this._state, DISPOSING, MOVE_NEXT_ENDED ) ) == MOVE_NEXT_ENDED
            || ( prevState = Interlocked.CompareExchange( ref this._state, DISPOSING, STATE_INITIAL ) ) == STATE_INITIAL
            || ( prevState = Interlocked.CompareExchange( ref this._state, DISPOSING, MOVE_NEXT_STARTED_CURRENT_NOT_READ ) ) == MOVE_NEXT_STARTED_CURRENT_NOT_READ
            )
         {
            try
            {
               var task = this._current.Dispose?.Invoke();
               if ( task != null )
               {
                  await task;
               }
            }
            finally
            {
               Interlocked.Exchange( ref this._state, DISPOSED );
            }
         }
         else
         {
            throw prevState == DISPOSED ?
               new ObjectDisposedException( this.GetType().FullName ) :
               new InvalidOperationException( "Enumerator can not be disposed at this stage." );
         }
      }

   }

   internal sealed class AsyncEnumerator<T> : AbstractAsyncEnumerator<T>
   {
      public AsyncEnumerator(
         SequentialEnumeratorCurrentInfo<T> currentInfo
         ) : base( currentInfo )
      {
      }
   }

   internal sealed class AsyncEnumerableExclusive<T> : IAsyncEnumerable<T>
   {
      private sealed class Enumerator : AbstractAsyncEnumerator<T>
      {
         private readonly Action _reset;

         public Enumerator(
            SequentialEnumeratorCurrentInfo<T> currentInfo,
            Action reset
            ) : base( currentInfo )
         {
            this._reset = reset;
         }

         public override async Task DisposeAsync()
         {
            try
            {
               await base.DisposeAsync();
            }
            finally
            {
               this._reset();
            }
         }
      }

      private const Int32 STATE_AVAILABLE = 0;
      private const Int32 STATE_RESERVED = 1;

      private Int32 _state;
      private readonly SequentialEnumeratorCurrentInfo<T> _currentInfo;
      private readonly Action _reset;

      public AsyncEnumerableExclusive(
         SequentialEnumeratorCurrentInfo<T> currentInfo,
         IAsyncProvider asyncProvider
         )
      {
         this.AsyncProvider = ArgumentValidator.ValidateNotNull( nameof( asyncProvider ), asyncProvider );
         this._currentInfo = currentInfo;
         this._reset = () =>
         {
            Interlocked.Exchange( ref this._state, STATE_AVAILABLE );
         };
      }

      public IAsyncProvider AsyncProvider { get; }

      public IAsyncEnumerator<T> GetAsyncEnumerator()
      {
         return Interlocked.CompareExchange( ref this._state, STATE_RESERVED, STATE_AVAILABLE ) == STATE_AVAILABLE ?
            new Enumerator( this._currentInfo, this._reset ) :
            throw new InvalidOperationException( "Concurrent enumeration" );
      }
   }

   internal abstract class SequentialEnumeratorCurrentInfo<T>
   {
      public SequentialEnumeratorCurrentInfo(
         MoveNextAsyncDelegate<T> moveNext,
         EnumerationEndedDelegate disposeDelegate
      )
      {
         this.MoveNext = moveNext;
         this.Dispose = disposeDelegate;
      }

      public MoveNextAsyncDelegate<T> MoveNext { get; }
      public EnumerationEndedDelegate Dispose { get; }

      public abstract T Current { get; set; }
   }

   internal sealed class SequentialEnumeratorCurrentInfoWithObject<T> : SequentialEnumeratorCurrentInfo<T>
   {
      private Object _current;

      public SequentialEnumeratorCurrentInfoWithObject(
         MoveNextAsyncDelegate<T> moveNext,
         EnumerationEndedDelegate disposeDelegate
         ) : base( moveNext, disposeDelegate )
      {
      }

      public override T Current
      {
         get => (T) this._current;
         set => Interlocked.Exchange( ref this._current, value );
      }

   }

   internal sealed class SequentialEnumeratorCurrentInfoWithInt32 : SequentialEnumeratorCurrentInfo<Int32>
   {
      private Int32 _current;

      public SequentialEnumeratorCurrentInfoWithInt32(
         MoveNextAsyncDelegate<Int32> moveNext,
         EnumerationEndedDelegate disposeDelegate
         ) : base( moveNext, disposeDelegate )
      {
      }

      public override Int32 Current
      {
         get => this._current;
         set => Interlocked.Exchange( ref this._current, value );
      }
   }

   internal sealed class SequentialEnumeratorCurrentInfoWithInt64 : SequentialEnumeratorCurrentInfo<Int64>
   {
      private Int64 _current;

      public SequentialEnumeratorCurrentInfoWithInt64(
         MoveNextAsyncDelegate<Int64> moveNext,
         EnumerationEndedDelegate disposeDelegate
         ) : base( moveNext, disposeDelegate )
      {
      }

      public override Int64 Current
      {
         get => Interlocked.Read( ref this._current );
         set => Interlocked.Exchange( ref this._current, value );
      }
   }

   internal sealed class SequentialEnumeratorCurrentInfoWithFloat32 : SequentialEnumeratorCurrentInfo<Single>
   {
      private Single _current;

      public SequentialEnumeratorCurrentInfoWithFloat32(
         MoveNextAsyncDelegate<Single> moveNext,
         EnumerationEndedDelegate disposeDelegate
         ) : base( moveNext, disposeDelegate )
      {
      }

      public override Single Current
      {
         get => this._current;
         set => Interlocked.Exchange( ref this._current, value );
      }
   }

   internal sealed class SequentialEnumeratorCurrentInfoWithFloat64 : SequentialEnumeratorCurrentInfo<Double>
   {
      private Int64 _current;

      public SequentialEnumeratorCurrentInfoWithFloat64(
         MoveNextAsyncDelegate<Double> moveNext,
         EnumerationEndedDelegate disposeDelegate
         ) : base( moveNext, disposeDelegate )
      {
      }

      public override Double Current
      {
         get => BitConverter.Int64BitsToDouble( Interlocked.Read( ref this._current ) );
         set => Interlocked.Exchange( ref this._current, BitConverter.DoubleToInt64Bits( value ) );
      }
   }
}
