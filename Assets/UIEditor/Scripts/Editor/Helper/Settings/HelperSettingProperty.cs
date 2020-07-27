
namespace Editor.UIEditor
{
    public class HelperSettingProperty<T>
    {
        private FieldReadWriteHelper _readWrite;
        private T _value;

        public HelperSettingProperty(string storeKey = null, T defaultValue = default(T))
        {
            if (PropertyReadWriteGenerator.GetOne(typeof(T), storeKey, out _readWrite))
            {
                _value = (T)_readWrite.Read(defaultValue);
            }
        }
        
        public T Get()
        {
            return _value;
        }

        public void Set(T value)
        {
            _value = value;
            _readWrite.Write(value);
        }
    }
}