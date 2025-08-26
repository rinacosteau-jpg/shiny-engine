using System.Reflection;
using Articy.World_Of_Red_Moon.GlobalVariables;

public class Skill {
    private readonly string _name;
    private readonly PropertyInfo _articyVar;
    private int _value;

    public Skill(string name) {
        _name = name;
        var gv = ArticyGlobalVariables.Default.PS;
        _articyVar = gv.GetType().GetProperty($"skill_{name}");
        if (_articyVar != null) {
            _value = (int)_articyVar.GetValue(gv);
        } else {
            _value = 42;
        }
    }

    public int Value {
        get => _value;
        set {
            _value = value;
            if (_articyVar != null) {
                _articyVar.SetValue(ArticyGlobalVariables.Default.PS, value);
            }
        }
    }
}
