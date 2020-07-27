namespace Editor.UIEditor
{
    interface IPropertyFieldReadWrite
    {
        object Read(string key, object defaultValue);
        void Write(string key, object value);
    }
}