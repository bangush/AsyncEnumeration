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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TTypeInfo =
#if NET40
   System.Type
#else
   System.Reflection.TypeInfo
#endif
   ;

namespace AsyncEnumeration.Abstractions
{
   /// <summary>
   /// This interface contains methods of <see cref="IAsyncEnumerable{T}"/> which do not require type argument, and are not directly related to enumeration.
   /// </summary>
   public interface IAsyncEnumerable
   {
      /// <summary>
      /// Gets the <see cref="IAsyncProvider"/> of this<see cref="IAsyncEnumerable"/>.
      /// </summary>
      /// <value>The <see cref="IAsyncProvider"/> of this<see cref="IAsyncEnumerable"/>.</value>
      /// <seealso cref="IAsyncProvider"/>
      IAsyncProvider AsyncProvider { get; }

   }

   /// <summary>
   /// This interface contains method which are like LINQ but for <see cref="IAsyncEnumerable{T}"/>.
   /// These methods allow the creators of <see cref="IAsyncEnumerable{T}"/> to customize how the created <see cref="IAsyncEnumerable{T}"/> behaves when LINQ methods are called on it.
   /// </summary>
   public partial interface IAsyncProvider
   {

   }

   /// <summary>
   /// This class contains common utilities for implementations of <see cref="IAsyncProvider"/>.
   /// </summary>
   public static class AsyncProviderUtilities
   {
      /// <summary>
      /// This method creates a new instance of exception with message informing that there are no elements in the sequence.
      /// </summary>
      /// <returns>A new instance of <see cref="InvalidOperationException"/>.</returns>
      public static InvalidOperationException EmptySequenceException() => new InvalidOperationException( "Empty sequence" );

      /// <summary>
      /// This method creates a new instance of exception with message informing that <see cref="IAsyncEnumerable.AsyncProvider"/> property was <c>null</c>.
      /// </summary>
      /// <returns>A new instance of <see cref="InvalidOperationException"/>.</returns>
      public static InvalidOperationException NoAsyncProviderException() => new InvalidOperationException( "No async provider" );

      /// <summary>
      /// This method checks whether type given as second parameter is assignable from type given as first parameter (or both are the same type).
      /// This check, however, only results to <c>true</c> when both are not structs and not generic parameters.
      /// </summary>
      /// <param name="t">The first type.</param>
      /// <param name="u">The second type.</param>
      /// <returns><c>true</c> if <paramref name="t"/> is same as <paramref name="u"/>, or if neither are structs and generic parameters, and <paramref name="u"/> is assignable from <paramref name="t"/>.</returns>
      /// <exception cref="NullReferenceException">If either of <paramref name="t"/> or <paramref name="u"/> is <c>null</c>.</exception>
      public static Boolean IsOfType(
         TTypeInfo t,
         TTypeInfo u
         )
      {
         // When both types are non-structs and non-generic-parameters, and u is supertype of t, then we don't need new enumerable/enumerator
         return Equals( t, u ) || ( !t.IsValueType && !u.IsValueType && !t.IsGenericParameter && !u.IsGenericParameter && u.IsAssignableFrom( t ) );
      }
   }

}