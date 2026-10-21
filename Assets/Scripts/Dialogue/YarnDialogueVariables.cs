using Yarn.Unity;

namespace Havengard.Dialogue
{
    /// <summary>Read-only view of Yarn variables (declared as $name in .yarn scripts).</summary>
    public sealed class YarnDialogueVariables : IDialogueVariables
    {
        private readonly VariableStorageBehaviour storage;

        public YarnDialogueVariables(VariableStorageBehaviour storage)
        {
            this.storage = storage;
        }

        public bool GetBool(string name)
        {
            bool value;
            return Read(name, out value) && value;
        }

        public int GetInt(string name)
        {
            float value;
            return Read(name, out value) ? (int)value : 0;
        }

        public string GetString(string name)
        {
            string value;
            return Read(name, out value) ? value : null;
        }

        private bool Read<T>(string name, out T value)
        {
            value = default(T);
            if (storage == null) return false;

            // Yarn variables always start with '$'.
            string key = name.StartsWith("$") ? name : "$" + name;
            return storage.TryGetValue(key, out value);
        }
    }
}