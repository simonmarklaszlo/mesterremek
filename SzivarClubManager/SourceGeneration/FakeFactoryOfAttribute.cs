using System;

namespace SzivarClubManager.SourceGeneration;

[AttributeUsage(AttributeTargets.Class)]
public class FakeFactoryOfAttribute : FactoryOfAttribute
{
    public FakeFactoryOfAttribute(Type modelType, params Type[] dependencies) : base(modelType, false, dependencies) { }
}