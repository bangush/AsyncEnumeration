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
using UtilPack;


namespace AsyncEnumeration.Abstractions
{
   public partial interface IAsyncProvider
   {
      /// <summary>
      /// This extension method will return <see cref="IAsyncEnumerable{T}"/> which will return only those items which are of given type.
      /// </summary>
      /// <typeparam name="T">The type of source enumerable items.</typeparam>
      /// <typeparam name="U">The type of target items.</typeparam>
      /// <param name="enumerable">This <see cref="IAsyncEnumerable{T}"/>.</param>
      /// <returns><see cref="IAsyncEnumerable{T}"/> which will return only those items which are of given type.</returns>
      /// <exception cref="NullReferenceException">If this <see cref="IAsyncEnumerable{T}"/> is <c>null</c>.</exception>
      /// <seealso cref="System.Linq.Enumerable.OfType{TResult}(System.Collections.IEnumerable)"/>
      IAsyncEnumerable<U> OfType<T, U>( IAsyncEnumerable<T> enumerable );
   }

   /// <summary>
   /// This struct exists to make life easier when using async variation of <see cref="System.Linq.Enumerable.OfType"/>, the <see cref="E_AsyncEnumeration.Of"/>.
   /// </summary>
   /// <typeparam name="T"></typeparam>
   public struct OfTypeInvoker<T>
   {
      private readonly IAsyncEnumerable<T> _source;

      /// <summary>
      /// Creates new instance of <see cref="OfTypeInvoker{T}"/> with given <see cref="IAsyncEnumerable{T}"/>.
      /// </summary>
      /// <param name="source">The <see cref="IAsyncEnumerable{T}"/>.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="source"/> is <c>null</c>.</exception>
      public OfTypeInvoker( IAsyncEnumerable<T> source )
      {
         this._source = ArgumentValidator.ValidateNotNull( nameof( source ), source );
      }

      /// <summary>
      /// Calls <see cref="IAsyncProvider.OfType"/> with <typeparamref name="T"/> as first type parameter, and <typeparamref name="U"/> as second type parameter.
      /// </summary>
      /// <typeparam name="U">The type to filter the elements of the <see cref="IAsyncEnumerable{T}"/> on.</typeparam>
      /// <returns>Filtered <see cref="IAsyncEnumerable{T}"/> with all the items of <typeparamref name="U"/>.</returns>
      public IAsyncEnumerable<U> Type<U>()
      {
         return (
            ( this._source ?? throw new InvalidOperationException( "This operation not possible on default-constructed type." ) )
            .AsyncProvider ?? throw AsyncProviderUtilities.NoAsyncProviderException()
            ).OfType<T, U>( this._source );
      }
   }
}

public static partial class E_AsyncEnumeration
{

   /// <summary>
   /// This extension method will return <see cref="IAsyncEnumerable{T}"/> which will return only those items which are of given type.
   /// </summary>
   /// <typeparam name="T">The type of source enumerable items.</typeparam>
   /// <param name="enumerable">This <see cref="IAsyncEnumerable{T}"/>.</param>
   /// <returns><see cref="IAsyncEnumerable{T}"/> which will return only those items which are of given type.</returns>
   /// <exception cref="NullReferenceException">If this <see cref="IAsyncEnumerable{T}"/> is <c>null</c>.</exception>
   /// <seealso cref="System.Linq.Enumerable.OfType{TResult}(System.Collections.IEnumerable)"/>
   public static OfTypeInvoker<T> Of<T>( this IAsyncEnumerable<T> enumerable )
      => new OfTypeInvoker<T>( ArgumentValidator.ValidateNotNullReference( enumerable ) );

}