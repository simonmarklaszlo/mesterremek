using System;

namespace SzivarClubManager.SourceGeneration;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class FakeFactoryOfAttribute : FactoryOfAttribute
{
    public FakeFactoryOfAttribute(Type modelType) : base(modelType, false) { }
}