using System;
using System.Linq;
using Ninject.Activation;
using Ninject.Planning.Targets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheGoldenMule
{
    /// <summary>
    /// Resolves implementations from the Unity project hierarchy.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HierarchyResolver<T> : Provider<T> where T : Object
    {
        /// <summary>
        /// The tag at which to begin the search.
        /// </summary>
        private readonly string _tag;

        public HierarchyResolver() : this(String.Empty)
        {
            // nothing
        }

        /// <summary>
        /// A tag at which to start the recursive search.
        /// </summary>
        /// <param name="tag"></param>
        public HierarchyResolver(string tag)
        {
            _tag = tag;
        }

        /// <summary>
        /// Provider implementation.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override T CreateInstance(IContext context)
        {
            if (!String.IsNullOrEmpty(_tag))
            {
                return GetInstanceByTag(_tag);
            }

            PropertyTarget propertyTarget = context.Request.Target as PropertyTarget;
            if (null != propertyTarget)
            {
                string propertyName = propertyTarget.Name;
                Type type = propertyTarget.Site.DeclaringType;

                if (null != type)
                {
                    InjectFromHierarchy linkage = Attribute
                        .GetCustomAttributes(type.GetProperty(propertyName))
                        .OfType<InjectFromHierarchy>()
                        .FirstOrDefault();
                    if (null != linkage)
                    {
                        return Retrieve(linkage, typeof (T)) as T;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves a GameObject or MonoBehaviour from an attribute.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Object Retrieve(InjectFromHierarchy attribute, Type type)
        {
            GameObject gameObject;

            GameObject root = GameObject.FindGameObjectWithTag(attribute.Tag);
            if (null == root)
            {
                return null;
            }

            Transform transform = root.transform;

            // if there is no query, use this GameObject
            if (null == attribute.Query)
            {
                gameObject = transform.gameObject;
            }
            else
            {
                gameObject = GetGameObject(attribute.Query, transform);
            }

            if (null == gameObject)
            {
                return null;
            }

            if (typeof(GameObject) == type)
            {
                return gameObject;
            }

            return gameObject.GetComponent(type) ?? gameObject.AddComponent(type);
        }

        /// <summary>
        /// Gets a GameObject from a query string. This differs from Transform.Find() in that
        /// the GameObjects specified do not have to be direct decendents.
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        private static GameObject GetGameObject(string queryString, Transform transform)
        {
            // split by /s
            string[] findQueries = queryString.Split(new[] { '/' });
            for (int i = 0, len = findQueries.Length; i < len; i++)
            {
                // we have now split the query string into a list of recursive queries
                string findQuery = findQueries[i];

                // retrieve the Transform
                transform = GetGameObjectRecursively(findQuery, transform);

                // couldn't find it!
                if (null == transform)
                {
                    return null;
                }
            }

            return transform.gameObject;
        }

        /// <summary>
        /// A recursive method that returns a Transform by a name down the hierarchy.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ancestor"></param>
        /// <returns></returns>
        private static Transform GetGameObjectRecursively(string name, Transform ancestor)
        {
            foreach (Transform child in ancestor)
            {
                // see if this immediate child is a match
                if (child.gameObject.name.Equals(name))
                {
                    return child;
                }

                // not a match, so recurse on its children
                Transform transform = GetGameObjectRecursively(name, child);

                // was it a match?!
                if (null != transform)
                {
                    return transform;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves a T by GameObject tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private static T GetInstanceByTag(string tag)
        {
            GameObject taggedObject = GameObject.FindGameObjectWithTag(tag);
            if (null != taggedObject)
            {
                if (typeof (Component).IsAssignableFrom(typeof (T)))
                {
                    return taggedObject.GetComponent(typeof (T)) as T;
                }

                if (typeof (GameObject) == typeof (T))
                {
                    return taggedObject as T;
                }
            }

            return null;
        }
    }
}