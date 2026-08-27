using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Framework.TemplateParser.Models
{
    internal class SpecialOperationPlaceHolderCollection : IEnumerable<KeyValuePair<string, SpecialOperationPlaceHolder>>
    {
        private ConcurrentDictionary<string, SpecialOperationPlaceHolder> concurrentDictionaryCollection = new ConcurrentDictionary<string, SpecialOperationPlaceHolder>();

        public int Count { get { return this.concurrentDictionaryCollection.Count; } }

        public void Add(string content, SpecialOperationPlaceHolder specialOperationPlaceHolder)
        {
            if (!concurrentDictionaryCollection.ContainsKey(content))
            {
                concurrentDictionaryCollection.TryAdd(content, specialOperationPlaceHolder);
            }
        }

        public void Add(SpecialOperationPlaceHolder specialOperationPlaceHolder)
        {
            if (!string.IsNullOrWhiteSpace(specialOperationPlaceHolder.Content))
            {
                if (!concurrentDictionaryCollection.ContainsKey(specialOperationPlaceHolder.Content))
                {
                    concurrentDictionaryCollection.TryAdd(specialOperationPlaceHolder.Content, specialOperationPlaceHolder);
                }
            }
            else
            {
                throw new Exception("specialOperationPlaceHolder content can't be empty");
            }
        }

        public bool ContainsKey(string key)
        {
            return this.concurrentDictionaryCollection.ContainsKey(key);
        }

        public bool Contains(Func<KeyValuePair<string, SpecialOperationPlaceHolder>, bool> predicate)
        {
            return !this.concurrentDictionaryCollection.FirstOrDefault(predicate).Equals(default(KeyValuePair<string, SpecialOperationPlaceHolder>));
        }

        public IEnumerator<KeyValuePair<string, SpecialOperationPlaceHolder>> GetEnumerator()
        {
            return this.concurrentDictionaryCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
