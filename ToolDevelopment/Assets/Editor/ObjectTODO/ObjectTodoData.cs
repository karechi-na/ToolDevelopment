using System.Collections.Generic;

[System.Serializable]
public class ObjectTodoData
{
    public string objectId;
    public string objectName;
    public List<TodoItem> todos = new();
}
