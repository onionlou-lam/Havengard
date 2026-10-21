using cherrydev;

namespace Havengard.Dialogue
{
    public sealed class CherryDialogueVariables : IDialogueVariables
    {
        private readonly DialogVariablesHandler handler;

        public CherryDialogueVariables(DialogVariablesHandler handler)
        {
            this.handler = handler;
        }

        public bool GetBool(string name) { return Has(name) && handler.GetVariableValue<bool>(name); }
        public int GetInt(string name) { return Has(name) ? handler.GetVariableValue<int>(name) : 0; }
        public string GetString(string name) { return Has(name) ? handler.GetVariableValue<string>(name) : null; }

        private bool Has(string name)
        {
            return handler != null && handler.HasVariable(name);
        }
    }
}