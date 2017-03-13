using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AntServiceStack.Text;
using AntServiceStack.Text.Common;
using AntServiceStack.Text.Support;

namespace AntServiceStackSwagger.Extentions
{
    public static class TranslateListWithElements
    {
        private static Dictionary<Type, ConvertInstanceDelegate> TranslateICollectionCache = new Dictionary<Type, ConvertInstanceDelegate>();
        private static Dictionary<ConvertibleTypeKey, ConvertInstanceDelegate> TranslateConvertibleICollectionCache = new Dictionary<ConvertibleTypeKey, ConvertInstanceDelegate>();

        public static object TranslateToGenericICollectionCache(object from, Type toInstanceOfType, Type elementType)
        {
            ConvertInstanceDelegate instanceDelegate1;
            if (TranslateListWithElements.TranslateICollectionCache.TryGetValue(toInstanceOfType, out instanceDelegate1))
                return instanceDelegate1(from, toInstanceOfType);
            ConvertInstanceDelegate instanceDelegate2 = (ConvertInstanceDelegate)typeof(TranslateListWithElements<>).MakeGenericType(elementType).GetStaticMethod("LateBoundTranslateToGenericICollection").MakeDelegate(typeof(ConvertInstanceDelegate), true);
            Dictionary<Type, ConvertInstanceDelegate> icollectionCache;
            Dictionary<Type, ConvertInstanceDelegate> dictionary;
            do
            {
                icollectionCache = TranslateListWithElements.TranslateICollectionCache;
                dictionary = new Dictionary<Type, ConvertInstanceDelegate>((IDictionary<Type, ConvertInstanceDelegate>)TranslateListWithElements.TranslateICollectionCache);
                dictionary[elementType] = instanceDelegate2;
            }
            while (Interlocked.CompareExchange<Dictionary<Type, ConvertInstanceDelegate>>(ref TranslateListWithElements.TranslateICollectionCache, dictionary, icollectionCache) != icollectionCache);
            return instanceDelegate2(from, toInstanceOfType);
        }

        public static object TranslateToConvertibleGenericICollectionCache(object from, Type toInstanceOfType, Type fromElementType)
        {
            ConvertibleTypeKey key = new ConvertibleTypeKey(toInstanceOfType, fromElementType);
            ConvertInstanceDelegate instanceDelegate1;
            if (TranslateListWithElements.TranslateConvertibleICollectionCache.TryGetValue(key, out instanceDelegate1))
                return instanceDelegate1(from, toInstanceOfType);
            Type genericTypeArgument = toInstanceOfType.FirstGenericType().GenericTypeArguments()[0];
            ConvertInstanceDelegate instanceDelegate2 = (ConvertInstanceDelegate)typeof(TranslateListWithConvertibleElements<,>).MakeGenericType(fromElementType, genericTypeArgument).GetStaticMethod("LateBoundTranslateToGenericICollection").MakeDelegate(typeof(ConvertInstanceDelegate), true);
            Dictionary<ConvertibleTypeKey, ConvertInstanceDelegate> icollectionCache;
            Dictionary<ConvertibleTypeKey, ConvertInstanceDelegate> dictionary;
            do
            {
                icollectionCache = TranslateListWithElements.TranslateConvertibleICollectionCache;
                dictionary = new Dictionary<ConvertibleTypeKey, ConvertInstanceDelegate>((IDictionary<ConvertibleTypeKey, ConvertInstanceDelegate>)TranslateListWithElements.TranslateConvertibleICollectionCache);
                dictionary[key] = instanceDelegate2;
            }
            while (Interlocked.CompareExchange<Dictionary<ConvertibleTypeKey, ConvertInstanceDelegate>>(ref TranslateListWithElements.TranslateConvertibleICollectionCache, dictionary, icollectionCache) != icollectionCache);
            return instanceDelegate2(from, toInstanceOfType);
        }

        public static object TryTranslateCollections(Type fromPropertyType, Type toPropertyType, object fromValue)
        {
            Type[] typeAndArguments1 = typeof(IEnumerable<>).GetGenericArgumentsIfBothHaveSameGenericDefinitionTypeAndArguments(fromPropertyType, toPropertyType);
            if (typeAndArguments1 != null)
                return TranslateListWithElements.TranslateToGenericICollectionCache(fromValue, toPropertyType, typeAndArguments1[0]);
            TypePair typeAndArguments2 = typeof(IEnumerable<>).GetGenericArgumentsIfBothHaveConvertibleGenericDefinitionTypeAndArguments(fromPropertyType, toPropertyType);
            if (typeAndArguments2 != null)
                return TranslateListWithElements.TranslateToConvertibleGenericICollectionCache(fromValue, toPropertyType, typeAndArguments2.Args1[0]);
            Type collectionType1 = fromPropertyType.GetCollectionType();
            Type collectionType2 = toPropertyType.GetCollectionType();
            if (collectionType1 == (Type)null || collectionType2 == (Type)null)
                return (object)null;
            if (collectionType1 == typeof(object) || collectionType2.IsAssignableFromType(collectionType1))
                return TranslateListWithElements.TranslateToGenericICollectionCache(fromValue, toPropertyType, collectionType2);
            return (object)null;
        }
    }
}
