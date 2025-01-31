using System.Xml.Linq;

namespace RuntimeOCD
{
    public class EvaluatorSet
    {
        protected readonly Dictionary<XElement, XElementEvaluator> _items;

        public EvaluatorSet()
        {
            _items = new Dictionary<XElement, XElementEvaluator>();
        }
        public EvaluatorSet(IEnumerable<XElement> elements) : this()
        {
            AddRange(elements);
        }
        public EvaluatorSet(IEnumerable<XObject> objects) : this()
        {
            AddRange(objects);
        }

        public virtual IEnumerable<XElementEvaluator> Items => _items.Values;
        public virtual IEnumerable<XElement> Keys => _items.Keys;
        public virtual int Count => _items.Count;

        public virtual XElementEvaluator this[XElement key]
        {
            get
            {
                if (!_items.TryGetValue(key, out var evaluator))
                {
                    evaluator = new XElementEvaluator(key);
                    _items[key] = evaluator;
                }
                return evaluator;
            }
        }

        public virtual Dictionary<XElement, XElementEvaluator>.ValueCollection.Enumerator GetEnumerator()
        {
            return _items.Values.GetEnumerator();
        }

        public virtual bool Contains(XElement key) => _items.ContainsKey(key);

        public virtual void Add(XObject key)
        {
            if (key is not XElement e) return;
            if (!_items.ContainsKey(e))
                _items[e] = new XElementEvaluator(e);
        }
        public virtual void Add(XElementEvaluator evaluator)
        {
            if (!_items.ContainsKey(evaluator.Element))
            {
                _items[evaluator.Element] = evaluator;
            }
        }
        public virtual void AddRange(IEnumerable<XElement> elements)
        {
            foreach (var element in elements)
            {
                Add(element);
            }
        }
        public virtual void AddRange(IEnumerable<XObject> objects)
        {
            foreach (var obj in objects)
            {
                if (obj is XElement element)
                {
                    Add(element);
                }
                else if (obj is XAttribute attribute)
                {
                    var parent = attribute.Parent;
                    if (parent != null)
                    {
                        Add(parent);
                    }
                }
            }
        }
        public virtual void Merge(EvaluatorSet other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            foreach (var pair in other._items)
            {
                if (!_items.ContainsKey(pair.Key))
                {
                    _items.Add(pair.Key, pair.Value);
                }
            }
        }
        public virtual void Merge(IEnumerable<XElement> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            foreach (var e in other)
                Add(e);
        }
        public virtual void Clear() => _items.Clear();
        public virtual bool Remove(XElement key) => _items.Remove(key);
        public virtual bool TryGetValue(XElement key, out XElementEvaluator evaluator) => _items.TryGetValue(key, out evaluator);
    }
}
