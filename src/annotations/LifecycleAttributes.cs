using System;

namespace CheatMenu;

[AttributeUsage(AttributeTargets.Method)]
public class Init : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class Unload : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class OnGui : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class Update : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class EnforceOrderFirst : Attribute {
    public int Order { get; }
    public EnforceOrderFirst(int order = 0){
        Order = order;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class EnforceOrderLast : Attribute { }
