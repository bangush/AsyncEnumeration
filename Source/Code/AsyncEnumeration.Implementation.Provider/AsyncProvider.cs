using AsyncEnumeration.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using UtilPack;

namespace AsyncEnumeration.Implementation.Provider
{
   /// <summary>
   /// This class implements <see cref="IAsyncProvider"/> in such way that all returned <see cref="IAsyncEnumerable{T}"/>s perform within the same process.
   /// Instead of using constructor to create new instances of this class, use the <see cref="Instance"/> static property.
   /// </summary>
   public sealed partial class DefaultAsyncProvider : IAsyncProvider
   {
      /// <summary>
      /// Gets the default, stateless, instance of this <see cref="DefaultAsyncProvider"/>.
      /// </summary>
      public static IAsyncProvider Instance { get; } = new DefaultAsyncProvider();

      private DefaultAsyncProvider()
      {

      }

      private static IAsyncEnumerable<U> FromTransformCallback<T, U>(
         IAsyncEnumerable<T> enumerable,
         Func<IAsyncEnumerator<T>, IAsyncEnumerator<U>> transform
         )
      {
         return new EnumerableWrapper<T, U>( enumerable, transform );
      }

      private static IAsyncEnumerable<U> FromTransformCallback<T, U, TArg>(
         IAsyncEnumerable<T> enumerable,
         TArg arg,
         Func<IAsyncEnumerator<T>, TArg, IAsyncEnumerator<U>> transform
         )
      {
         return new EnumerableWrapper<T, U, TArg>( enumerable, transform, arg );
      }


      private sealed class EnumerableWrapper<T, U> : IAsyncEnumerable<U>
      {
         private readonly IAsyncEnumerable<T> _enumerable;
         private readonly Func<IAsyncEnumerator<T>, IAsyncEnumerator<U>> _getEnumerator;

         public EnumerableWrapper(
            IAsyncEnumerable<T> enumerable,
            Func<IAsyncEnumerator<T>, IAsyncEnumerator<U>> getEnumerator
            )
         {
            this._enumerable = ArgumentValidator.ValidateNotNull( nameof( enumerable ), enumerable );
            this._getEnumerator = ArgumentValidator.ValidateNotNull( nameof( getEnumerator ), getEnumerator );
         }

         IAsyncProvider IAsyncEnumerable.AsyncProvider => this._enumerable.AsyncProvider;

         IAsyncEnumerator<U> IAsyncEnumerable<U>.GetAsyncEnumerator() => this._getEnumerator( this._enumerable.GetAsyncEnumerator() );
      }

      private sealed class EnumerableWrapper<T, U, TArg> : IAsyncEnumerable<U>
      {
         private readonly IAsyncEnumerable<T> _enumerable;
         private readonly Func<IAsyncEnumerator<T>, TArg, IAsyncEnumerator<U>> _getEnumerator;
         private readonly TArg _arg;

         public EnumerableWrapper(
            IAsyncEnumerable<T> enumerable,
            Func<IAsyncEnumerator<T>, TArg, IAsyncEnumerator<U>> getEnumerator,
            TArg arg
            )
         {
            this._enumerable = ArgumentValidator.ValidateNotNull( nameof( enumerable ), enumerable );
            this._getEnumerator = ArgumentValidator.ValidateNotNull( nameof( getEnumerator ), getEnumerator );
            this._arg = arg;
         }

         IAsyncProvider IAsyncEnumerable.AsyncProvider => this._enumerable.AsyncProvider;

         IAsyncEnumerator<U> IAsyncEnumerable<U>.GetAsyncEnumerator() => this._getEnumerator( this._enumerable.GetAsyncEnumerator(), this._arg );
      }

   }
}