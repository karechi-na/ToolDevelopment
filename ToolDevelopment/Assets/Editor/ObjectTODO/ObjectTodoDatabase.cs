using System.Collections.Generic;
using UnityEditor;

[FilePath("ProjectSettings/ObjectTodoDatabase.asset", FilePathAttribute.Location.ProjectFolder)]
public class ObjectTodoDatabase : ScriptableSingleton<ObjectTodoDatabase>
{
    public List<ObjectTodoData> todoDataList = new();

    public void SaveDatabase()
    {
        Save(true);
    }
}
